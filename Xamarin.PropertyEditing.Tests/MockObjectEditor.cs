using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	internal class MockObjectEditor
		: IObjectEditor, IObjectEventEditor
	{
		public MockObjectEditor ()
		{
		}

		public MockObjectEditor (params IPropertyInfo[] properties)
		{
			Properties = properties
				.Union(properties
					.OfType<IComplexPropertyInfo>()
					.SelectMany(p => p.Properties)
				).ToArray ();
		}

		public MockObjectEditor (MockControl control)
		{
			Properties = control.Properties.Values.ToArray();
			Events = control.Events.Values.ToArray();
			Target = control;
		}

		public bool SupportsDefault
		{
			get;
			set;
		}

		public object Target
		{
			get;
			set;
		} = new object ();

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
		} = new IPropertyInfo[0];

		public IReadOnlyCollection<IEventInfo> Events
		{
			get;
			set;
		} = new IEventInfo[0];

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

		public Task AttachHandlerAsync (IEventInfo ev, string handlerName)
		{
			this.events[ev] = handlerName;
			return Task.FromResult (true);
		}

		public Task DetachHandlerAsync (IEventInfo ev, string handlerName)
		{
			this.events.Remove (ev);
			return Task.FromResult (true);
		}

		public Task<IReadOnlyList<string>> GetHandlersAsync (IEventInfo ev)
		{
			if (this.events.TryGetValue (ev, out string handler))
				return Task.FromResult<IReadOnlyList<string>> (new[] { handler });

			return Task.FromResult<IReadOnlyList<string>> (new string[0]);
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variation = null)
		{
			if (variation != null)
				throw new NotSupportedException(); // TODO

			if (value.Source != ValueSource.Local && ValueEvaluator != null) {
				value.Value = (T)ValueEvaluator (property, value.ValueDescriptor);
			}

			object softValue = value;

			if (typeof(T) != property.Type) {
				if (property is IPropertyConverter converter && converter.TryConvert (value.Value, property.Type, out object v)) {
					Type softType = typeof (ValueInfo<>).MakeGenericType (property.Type);
					softValue = Activator.CreateInstance (softType);
					softType.GetProperty ("Value").SetValue (softValue, v);
					softType.GetProperty ("Source").SetValue (softValue, value.Source);
				}
			}
			
			this.values[property] = softValue;
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property));
		}

		public async Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variation = null)
		{
			if (variation != null)
				throw new NotSupportedException (); // TODO

			if (this.values.TryGetValue (property, out object value)) {
				var info = value as ValueInfo<T>;
				if (info != null)
					return info;
				else if (value == null || value is T) {
					return new ValueInfo<T> {
						Value = (T)value,
						Source = ValueSource.Local
					};
				} else {
					ValueSource source = ValueSource.Local;
					Type valueType = value.GetType ();
					if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition () == typeof (ValueInfo<>)) {
						source = (ValueSource)valueType.GetProperty ("Source").GetValue (value);
						value = valueType.GetProperty ("Value").GetValue (value);
					}

					if (property is IPropertyConverter converter && converter.TryConvert (value, typeof (T), out object newValue)) {
						return new ValueInfo<T> {
							Source = source,
							Value = (T)newValue
						};
					}
				}
			}

			return new ValueInfo<T> {
				Source = (SupportsDefault) ? ValueSource.Default : ValueSource.Local,
				Value = default(T)
			};
		}
#pragma warning restore CS1998

		internal readonly IDictionary<IPropertyInfo, object> values = new Dictionary<IPropertyInfo, object> ();
		internal readonly IDictionary<IEventInfo, string> events = new Dictionary<IEventInfo, string> ();
	}
}