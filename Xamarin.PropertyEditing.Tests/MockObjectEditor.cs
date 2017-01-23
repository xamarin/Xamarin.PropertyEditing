using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.PropertyEditing.Tests
{
	internal class MockObjectEditor
		: IObjectEditor
	{
		public MockObjectEditor ()
		{
			
		}

		public MockObjectEditor (params IPropertyInfo[] properties)
		{
			Properties = properties.ToArray ();
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public IReadOnlyCollection<IPropertyInfo> Properties
		{
			get;
			set;
		}

		public IObjectEditor Parent
		{
			get;
			set;
		}

		public IReadOnlyList<IObjectEditor> DirectChildren
		{
			get;
			set;
		}

		public void ChangeAllProperties ()
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (null));
		}
		
		public void SetValue<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null)
		{
			if (variation != null)
				throw new NotSupportedException(); // TODO

			this.values[property] = value;
		}

		public ValueInfo<T> GetValue<T> (IPropertyInfo property, PropertyVariation variation = null)
		{
			if (variation != null)
				throw new NotSupportedException (); // TODO

			object value;
			if (this.values.TryGetValue (property, out value)) {
				ValueInfo<T> info = value as ValueInfo<T>;
				if (info != null)
					return info;
			}

			return new ValueInfo<T> {
				Source = ValueSource.Local,
				Value = default(T)
			};
		}

		private readonly Dictionary<IPropertyInfo, object> values = new Dictionary<IPropertyInfo, object> ();
	}
}