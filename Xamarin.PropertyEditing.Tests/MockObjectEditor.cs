using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public object Target
		{
			get;
			set;
		}

		public string TypeName
		{
			get;
			set;
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public Func<IPropertyInfo, object, object> ValueEvaluator
		{
			get;
			set;
		}

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

		public void RaisePropertyChanged (IPropertyInfo property)
		{
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null)
		{
			if (variation != null)
				throw new NotSupportedException(); // TODO

			if (value.Source != ValueSource.Local && ValueEvaluator != null) {
				value.Value = (T)ValueEvaluator (property, value.ValueDescriptor);
			}

			this.values[property] = value;
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}

		public async Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variation = null)
		{
			if (variation != null)
				throw new NotSupportedException (); // TODO

			object value;
			if (this.values.TryGetValue (property, out value)) {
				ValueInfo<T> info = value as ValueInfo<T>;
				if (info != null)
					return info;
				else if (value != null) {
					return new ValueInfo<T> {
						Value = (T)value,
						Source = ValueSource.Local
					};
				}
			}

			return new ValueInfo<T> {
				Source = ValueSource.Local,
				Value = default(T)
			};
		}
#pragma warning restore CS1998

		internal readonly Dictionary<IPropertyInfo, object> values = new Dictionary<IPropertyInfo, object> ();
	}
}