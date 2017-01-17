using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

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

			this.typeConverter = new Lazy<TypeConverter> (() => {
				TypeConverterAttribute attribute = this.propertyInfo.GetCustomAttribute<TypeConverterAttribute> ();
				if (attribute == null)
					return null;

				Type type = System.Type.GetType (attribute.ConverterTypeName);
				if (type == null)
					return null;

				return (TypeConverter) Activator.CreateInstance (type);
			});
		}

		public string Name => this.propertyInfo.Name;

		public Type Type => this.propertyInfo.PropertyType;

		public string Category => this.category.Value;

		public ValueSources ValueSources => ValueSources.Local;

		public IReadOnlyList<PropertyVariation> Variations => EmtpyVariations;

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;

		public void SetValue<T> (object target, T value)
		{
			object realValue = value;
			if (this.typeConverter.Value != null && this.typeConverter.Value.CanConvertFrom (typeof(T))) {
				realValue = this.typeConverter.Value.ConvertFrom (value);
			} else if (realValue != null && !this.propertyInfo.PropertyType.IsInstanceOfType (value)) {
				realValue = Convert.ChangeType (value, this.propertyInfo.PropertyType);
			}

			this.propertyInfo.SetValue (target, realValue);
		}

		public T GetValue<T> (object target)
		{
			object value = this.propertyInfo.GetValue (target);
			if (this.typeConverter.Value != null && this.typeConverter.Value.CanConvertTo (typeof(T))) {
				value = this.typeConverter.Value.ConvertTo (value, typeof(T));
			} else if (value != null && !(value is T)) {
				if (typeof(T) == typeof(string))
					value = value.ToString ();
				else
					value = Convert.ChangeType (value, typeof(T));
			}

			return (T)value;
		}

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

		private readonly Lazy<TypeConverter> typeConverter;
		private readonly Lazy<string> category;

		private readonly PropertyInfo propertyInfo;

		private static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		private static readonly PropertyVariation[] EmtpyVariations = new PropertyVariation[0];
	}
}