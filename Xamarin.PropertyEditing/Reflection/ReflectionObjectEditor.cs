using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionObjectEditor
		: IObjectEditor
	{
		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public IReadOnlyCollection<IPropertyInfo> Properties
		{
			get;
		}

		public IObjectEditor Parent
		{
			get;
		}

		public IReadOnlyList<IObjectEditor> DirectChildren
		{
			get;
		}
		
		public Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null)
		{
			throw new NotImplementedException ();
		}

		public Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variation = null)
		{
			throw new NotImplementedException ();
		}
	}
}