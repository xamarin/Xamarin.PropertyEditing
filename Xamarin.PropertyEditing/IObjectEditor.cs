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

		IObjectEditor Parent { get; }

		/// <remarks>
		/// These are children that do not participate in the standard element hierarchy, such as segments in a segmented control.
		/// Note that objects defined by editors do not need to match real objects in the real hierarchy, they can be faked. An existing
		/// iOS property chooser itself can be maped to having an object editor for its actual object.
		/// </remarks>
		IReadOnlyList<IObjectEditor> DirectChildren { get; }

		event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		void SetValue<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null);
		ValueInfo<T> GetValue<T> (IPropertyInfo property, PropertyVariation variation = null);
	}
}
