using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionPropertyInfo
		: IPropertyInfo, IEquatable<ReflectionPropertyInfo>
	{
		public ReflectionPropertyInfo (PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;

			this.category = new Lazy<string> (() => {
				CategoryAttribute categoryAttribute = this.propertyInfo.GetCustomAttribute<CategoryAttribute> ();
				return categoryAttribute?.Category;
			});

			this.typeConverter = new Lazy<List<TypeConverter>> (() => {
				List<TypeConverter> converters = new List<TypeConverter> ();

				var attributes = this.propertyInfo.GetCustomAttributes<TypeConverterAttribute> ().Concat (this.propertyInfo.PropertyType.GetCustomAttributes<TypeConverterAttribute> ());
				foreach (TypeConverterAttribute attribute in attributes) {
					Type type = System.Type.GetType (attribute.ConverterTypeName);
					if (type == null)
						continue;

					converters.Add ((TypeConverter)Activator.CreateInstance (type));
				}

				return converters;
			});
		}

		public string Name => this.propertyInfo.Name;

		public string Description => null;

		public Type Type => this.propertyInfo.PropertyType;

		public string Category => this.category.Value;

		public bool CanWrite => this.propertyInfo.CanWrite;

		public ValueSources ValueSources => ValueSources.Local;

		public IReadOnlyList<PropertyVariation> Variations => EmtpyVariations;

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public virtual async Task SetValueAsync<T> (object target, T value)
		{
			object realValue = value;
			object converted;
			if (TryConvertFromValue (value, out converted)) {
				realValue = converted;
			} else if (realValue != null && !this.propertyInfo.PropertyType.IsInstanceOfType (value)) {
				realValue = Convert.ChangeType (value, this.propertyInfo.PropertyType);
			}

			this.propertyInfo.SetValue (target, realValue);
		}

		public virtual async Task<T> GetValueAsync<T> (object target)
		{
			object value = this.propertyInfo.GetValue (target);
			T converted;
			if (TryConvertToValue (value, out converted)) {
				value = converted;
			} else if (value != null && !(value is T)) {
				if (typeof(T) == typeof(string))
					value = value.ToString ();
				else
					value = Convert.ChangeType (value, typeof(T));
			}

			return (T)value;
		}
#pragma warning restore CS1998

		public bool Equals (ReflectionPropertyInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return this.propertyInfo.Equals (other.propertyInfo);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != this.GetType ())
				return false;

			return Equals ((ReflectionPropertyInfo)obj);
		}

		public override int GetHashCode ()
		{
			return this.propertyInfo.GetHashCode ();
		}

		public static bool operator == (ReflectionPropertyInfo left, ReflectionPropertyInfo right)
		{
			return Equals (left, right);
		}

		public static bool operator != (ReflectionPropertyInfo left, ReflectionPropertyInfo right)
		{
			return !Equals (left, right);
		}

		protected PropertyInfo PropertyInfo => this.propertyInfo;

		private readonly Lazy<List<TypeConverter>> typeConverter;
		private readonly Lazy<string> category;

		private readonly PropertyInfo propertyInfo;

		private static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		private static readonly PropertyVariation[] EmtpyVariations = new PropertyVariation[0];

		private bool TryConvertToValue<T> (object value, out T converted)
		{
			converted = default(T);
			List<TypeConverter> converters = this.typeConverter.Value;

			for (int i = 0; i < converters.Count; i++) {
				TypeConverter c = converters[i];
				if (c.CanConvertTo (typeof(T))) {
					converted = (T) c.ConvertTo (value, typeof(T));
					return true;
				}
			}

			return false;
		}

		private bool TryConvertFromValue<T> (T value, out object converted)
		{
			converted = null;
			List<TypeConverter> converters = this.typeConverter.Value;
			for (int i = 0; i < converters.Count; i++) {
				TypeConverter c = converters[i];
				if (c.CanConvertFrom (typeof(T))) {
					converted = c.ConvertFrom (value);
					return true;
				}
			}

			return false;
		}
	}
}