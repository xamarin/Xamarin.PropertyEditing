using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	internal class MockNameableEditor :MockObjectEditor, INameableObject
	{
		public string ObjectName { get; set; } = "Nameable";

		public Task<string> GetNameAsync ()
		{
			return Task.FromResult (ObjectName);
		}

		public Task SetNameAsync (string name)
		{
			ObjectName = name;
			return Task.FromResult (false);
		}

		public MockNameableEditor (MockControl control) : base (control)
		{ }
	}

	internal class MockObjectEditor
		: IObjectEditor, IObjectEventEditor, ICompleteValues
	{
		public MockObjectEditor ()
		{
			TargetType = Target.GetType ().ToTypeInfo ();
		}

		public MockObjectEditor (params IPropertyInfo[] properties)
			: this()
		{
			Properties = properties.ToArray ();
		}

		public MockObjectEditor (IReadOnlyList<IPropertyInfo> properties,
			IReadOnlyDictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> assignableTypes)
			: this ()
		{
			Properties = properties;
			this.assignableTypes = assignableTypes;
		}

		public MockObjectEditor (MockControl control)
		{
			Properties = control.Properties.Values.ToArray();
			Events = control.Events.Values.ToArray();
			Target = control;
			TargetType = Target.GetType ().ToTypeInfo ();
		}

		public object Target
		{
			get;
			set;
		} = new object ();

		public ITypeInfo TargetType
		{
			get;
			set;
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		/// <summary>
		/// Test helper for non-local values, passes in the property, <see cref="ValueInfo{T}.ValueDescriptor"/>, <see cref="ValueInfo{T}.SourceDescriptor"/>
		/// </summary>
		public Func<IPropertyInfo, object, object, object> ValueEvaluator
		{
			get;
			set;
		}

		public IReadOnlyCollection<IPropertyInfo> Properties
		{
			get;
			set;
		} = new IPropertyInfo[0];

		public IReadOnlyDictionary<IPropertyInfo, KnownProperty> KnownProperties
		{
			get;
			set;
		}

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

		public IResourceProvider Resources
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
			string handler;
			if (this.events.TryGetValue (ev, out handler))
				return Task.FromResult<IReadOnlyList<string>> (new[] { handler });

			return Task.FromResult<IReadOnlyList<string>> (new string[0]);
		}

		public Task<AssignableTypesResult> GetAssignableTypesAsync (IPropertyInfo property, bool childTypes)
		{
			if (this.assignableTypes != null) {
				if (!this.assignableTypes.TryGetValue (property, out IReadOnlyList<ITypeInfo> types))
					return Task.FromResult (new AssignableTypesResult (Enumerable.Empty<ITypeInfo> ().ToArray ()));
				else
					return Task.FromResult (new AssignableTypesResult (types));
			}

			return ReflectionObjectEditor.GetAssignableTypes (property.RealType, childTypes);
		}

		public Task<IReadOnlyCollection<PropertyVariation>> GetPropertyVariantsAsync (IPropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			if (!this.values.TryGetValue (property, out IDictionary<PropertyVariation, object> propertyValues)) {
				return Task.FromResult<IReadOnlyCollection<PropertyVariation>> (new PropertyVariation[0]);
			}

			return Task.FromResult<IReadOnlyCollection<PropertyVariation>> (propertyValues.Keys.Except (new[] { NeutralVariations }).ToList ());
		}

		public Task RemovePropertyVariantAsync (IPropertyInfo property, PropertyVariation variant)
		{
			if (property == null)
				throw new ArgumentNullException (nameof(property));
			if (variant == null)
				throw new ArgumentNullException (nameof(variant));

			if (this.values.TryGetValue (property, out IDictionary<PropertyVariation, object> propertyValues)) {
				propertyValues.Remove (variant);
			}

			return Task.CompletedTask;
		}

		public Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariation variations = null)
		{
			value = new ValueInfo<T> {
				CustomExpression = value.CustomExpression,
				Source = value.Source,
				ValueDescriptor = value.ValueDescriptor,
				SourceDescriptor = value.SourceDescriptor,
				Value = value.Value
			};

			if (!this.values.TryGetValue (property, out IDictionary<PropertyVariation, object> propertyValues)) {
				this.values[property] = propertyValues = new Dictionary<PropertyVariation, object> ();
			}

			if (value.Source != ValueSource.Local && ValueEvaluator != null) {
				value.Value = (T)ValueEvaluator (property, value.ValueDescriptor, value.SourceDescriptor);
			} else if (value.Source == ValueSource.Unset || (property.ValueSources.HasFlag (ValueSources.Default) && Equals (value.Value, default(T))) && value.ValueDescriptor == null && value.SourceDescriptor == null) {
				if (propertyValues.Remove (variations ?? NeutralVariations)) {
					PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property, variations));
					return Task.CompletedTask;
				}
			}

			object softValue = value;

			if (typeof(T) != property.Type) {
				IPropertyConverter converter = property as IPropertyConverter;

				bool changeValueInfo = false;

				object v = value.Value;
				if (ReferenceEquals (value.Value, null) && property.Type.IsValueType) {
					if ((property.ValueSources & ValueSources.Default) == ValueSources.Default) {
						v = Activator.CreateInstance (property.Type);
						changeValueInfo = true;
					}
				} else if (converter != null && converter.TryConvert (value.Value, property.Type, out v)) {
					changeValueInfo = true;
				}

				if (changeValueInfo) {
					var softType = typeof (ValueInfo<>).MakeGenericType (property.Type);
					softValue = Activator.CreateInstance (softType);
					softType.GetProperty ("Value").SetValue (softValue, v);
					softType.GetProperty ("ValueDescriptor").SetValue (softValue, value.ValueDescriptor);
					softType.GetProperty ("Source").SetValue (softValue, value.Source);
					softType.GetProperty ("SourceDescriptor").SetValue (softValue, value.SourceDescriptor);
				}

				if (typeof(T).Name == "IReadOnlyList`1") {
					var list = (IReadOnlyList<int>) value.Value;
					int iv = 0;
					foreach (int flag in list) {
						iv |= flag;
					}

					softValue = new ValueInfo<int> {
						Value = iv,
						Source = value.Source
					};
				}
			}

			// Set to resource won't pass values so we will store it on the info since we just pass it back in GetValue
			if (value.Source == ValueSource.Resource && value.SourceDescriptor is Resource) {
				Type rt = value.SourceDescriptor.GetType();
				if (rt.IsGenericType) {
					Type ta = rt.GetGenericArguments ()[0];
					if (typeof (T).IsAssignableFrom (ta)) {
						PropertyInfo pi = rt.GetProperty ("Value");
						value.Value = (T)pi.GetValue (value.SourceDescriptor);
					} else {
						TypeConverter converter = TypeDescriptor.GetConverter (ta);
						if (converter != null && converter.CanConvertTo(typeof(T))) {
							PropertyInfo pi = rt.GetProperty ("Value");
							value.Value = (T)converter.ConvertTo (pi.GetValue (value.SourceDescriptor), typeof (T));
						}
					}
				}
			}

			propertyValues[variations ?? NeutralVariations] = softValue;
			PropertyChanged?.Invoke (this, new EditorPropertyChangedEventArgs (property, variations));
			return Task.CompletedTask;
		}

		public Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariation variations = null)
		{
			Type tType = typeof(T);

			IDictionary<PropertyVariation, object> propertyValues;
			if (!this.values.TryGetValue (property, out propertyValues) || !propertyValues.TryGetValue (variations ?? NeutralVariations, out object value)) {
				return Task.FromResult (new ValueInfo<T> {
					Source = (property.ValueSources.HasFlag (ValueSources.Default))
						? ValueSource.Default
						: ValueSource.Unset,
					Value = default(T)
				});
			}

			var info = value as ValueInfo<T>;
			if (info != null) {
				return Task.FromResult (new ValueInfo<T> {
					CustomExpression = info.CustomExpression,
					Source = info.Source,
					ValueDescriptor = info.ValueDescriptor,
					SourceDescriptor = info.SourceDescriptor,
					Value = info.Value
				});
			} else if (value == null || value is T) {
				return Task.FromResult (new ValueInfo<T> {
					Value = (T) value,
					Source = ValueSource.Local
				});
			} else if (tType.Name == "IReadOnlyList`1") {
				// start with just supporting ints for now
				var predefined = (IReadOnlyDictionary<string, int>)property.GetType().GetProperty(nameof(IHavePredefinedValues<int>.PredefinedValues)).GetValue(property);

				var underlyingInfo = value as ValueInfo<int>;

				int realValue;
				if (value is int i) {
					realValue = i;
				} else
					realValue = ((ValueInfo<int>) value).Value;

				var flags = new List<int> ();
				foreach (int v in predefined.Values) {
					if (v == 0 && realValue != 0)
						continue;

					if ((realValue & v) == v)
						flags.Add (v);
				}

				return Task.FromResult ((ValueInfo<T>)Convert.ChangeType (new ValueInfo<IReadOnlyList<int>> {
					Value = flags,
					Source = underlyingInfo?.Source ?? ValueSource.Local
				}, typeof(ValueInfo<T>)));
			} else {
				object sourceDescriptor = null, valueDescriptor = null;
				ValueSource source = ValueSource.Local;
				Type valueType = value.GetType ();
				if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition () == typeof(ValueInfo<>)) {
					source = (ValueSource)valueType.GetProperty ("Source").GetValue (value);
					sourceDescriptor = valueType.GetProperty (nameof (ValueInfo<T>.SourceDescriptor)).GetValue (value);
					valueDescriptor = valueType.GetProperty (nameof (ValueInfo<T>.ValueDescriptor)).GetValue (value);
					value = valueType.GetProperty ("Value").GetValue (value);
					valueType = valueType.GetGenericArguments ()[0];
				}

				object newValue;
				IPropertyConverter converter = property as IPropertyConverter;
				if (converter != null && converter.TryConvert (value, tType, out newValue)) {
					return Task.FromResult (new ValueInfo<T> {
						Source = source,
						Value = (T)newValue,
						ValueDescriptor = valueDescriptor,
						SourceDescriptor = sourceDescriptor
					});
				} else if (typeof(T).IsAssignableFrom (valueType)) {
					return Task.FromResult (new ValueInfo<T> {
						Source = source,
						Value = (T)value,
						ValueDescriptor = valueDescriptor,
						SourceDescriptor = sourceDescriptor
					});
				}
			}

			return Task.FromResult (new ValueInfo<T> {
				Source = (property.ValueSources.HasFlag (ValueSources.Default)) ? ValueSource.Default : ValueSource.Unset,
				Value = default(T)
			});
		}

		public Task<ITypeInfo> GetValueTypeAsync (IPropertyInfo property, PropertyVariation variations = null)
		{
			Type type = property.Type;
			if (this.values.TryGetValue (property, out IDictionary<PropertyVariation, object> propertyValues) && propertyValues.TryGetValue (variations ?? NeutralVariations, out object value)) {
				Type valueType = value.GetType ();
				if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition () == typeof(ValueInfo<>)) {
					value = valueType.GetProperty ("Value").GetValue (value);
					type = value.GetType ();
				} else
					type = valueType;
			}

			var asm = new AssemblyInfo (type.Assembly.FullName, true);
			return Task.FromResult<ITypeInfo> (new TypeInfo (asm, type.Namespace, type.Name));
		}

		public bool CanAutocomplete (string input)
		{
			return (input != null && input.Trim ().StartsWith ("@"));
		}

		public async Task<IReadOnlyList<string>> GetCompletionsAsync (IPropertyInfo property, string input, CancellationToken cancellationToken)
		{
			if (Resources == null)
				return Array.Empty<string> ();

			input = input.Trim ().TrimStart('@');
			var resources = await Resources.GetResourcesAsync (Target, property, cancellationToken);
			return resources.Where (r =>
					r.Name.IndexOf (input, StringComparison.OrdinalIgnoreCase) != -1
					&& r.Name.Length > input.Length) // Skip exact matches
				.Select (r => "@" + r.Name).ToList ();
		}

		private static readonly PropertyVariation NeutralVariations = new PropertyVariation();

		private readonly IDictionary<IPropertyInfo,IDictionary<PropertyVariation, object>> values = new Dictionary<IPropertyInfo, IDictionary<PropertyVariation, object>> ();
		internal readonly IDictionary<IEventInfo, string> events = new Dictionary<IEventInfo, string> ();
		internal readonly IReadOnlyDictionary<IPropertyInfo, IReadOnlyList<ITypeInfo>> assignableTypes;
	}
}
