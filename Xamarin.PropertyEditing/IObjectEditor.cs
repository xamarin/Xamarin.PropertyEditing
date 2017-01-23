using System;
using System.Collections.Generic;

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

		void SetValue<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null);

		/// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
		ValueInfo<T> GetValue<T> (IPropertyInfo property, PropertyVariation variation = null);
	}
}
