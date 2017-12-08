using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public class EditorPropertyChangedEventArgs
		: EventArgs
	{
		/// <param name="property">The property that was updated, or <c>null</c> if a full refresh is required.</param>
		public EditorPropertyChangedEventArgs (IPropertyInfo property)
		{
			Property = property;
		}

		/// <summary>
		/// Gets the property that was changed, or <c>null</c> for a full refresh.
		/// </summary>
		public IPropertyInfo Property
		{
			get;
		}
	}

	public interface IObjectEditor
	{
		/// <summary>
		/// Gets the target object being edited.
		/// </summary>
		object Target { get; }

		/// <summary>
		/// Gets the real type name for the object.
		/// </summary>
		/// <remarks>Also used for iconography.</remarks>
		string TypeName { get; }

		/// <remarks>
		/// These properties should be minimally equatable to the same property for another object of the same type. This includes
		/// for base class properties for two different derived types.
		/// </remarks>
		IReadOnlyCollection<IPropertyInfo> Properties { get; }

		/// <summary>
		/// Gets the parent object editor for the object this editor represents or <c>null</c> if none.
		/// </summary>
		IObjectEditor Parent { get; }

		/// <remarks>
		/// These are children that do not participate in the standard element hierarchy, such as segments in a segmented control.
		/// Note that objects defined by editors do not need to match real objects in the real hierarchy, they can be faked. An existing
		/// iOS property chooser itself can be maped to having an object editor for its actual object.
		/// </remarks>
		IReadOnlyList<IObjectEditor> DirectChildren { get; }

		/// <summary>
		/// Raised when a property's value changes. <see cref="EditorPropertyChangedEventArgs.Property"/> can be <c>null</c> for a full refresh.
		/// </summary>
		event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		Task<IReadOnlyList<ITypeInfo>> GetAssignableTypesAsync (IPropertyInfo property);

		/*
		 * Dealing with async values in the context of what's possible across all target platforms is a bit complex.
		 * While implicit safety may be able to be ensured, it would be exhaustive to reason it out and could change
		 * at any moment, so we need an explicit safety strategy. As we make value gets and sets async, we open ourselves
		 * to race conditions even on just the UI thread. Value changes can come from the user, or from the object editor
		 * at any point. So, we break these changes into two categories: Blocking UI & Waiting Values.
		 *
		 * Anything that needs to deal with the current state of the values will Wait asynchronously on prior changes to
		 * finish. These largely involve value changes from the object editor, which just acts as a INPC and does not
		 * contain the value itself, so it will be safe and the latest value when queried upon reaching active state in
		 * the wait queue (see AsyncWorkQueue). Selected objects (when >1) need to wait on values as they need to be
		 * able to calculate the intersection values.
		 *
		 * Anything that needs to deal with user input will need to block its UI. We simply can't reason about the user
		 * changing values while waiting for previous changes, because they value they're changing may change underneath
		 * them or become invalid as a result of a prior pending change.
		 */

		Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null);

		/// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
		Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variation = null);
	}
}
