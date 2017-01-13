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
		}

		public string Name => this.propertyInfo.Name;

		public Type Type => this.propertyInfo.PropertyType;

		public string Category => this.category.Value;

		public ValueSources ValueSources => ValueSources.Local;

		public IReadOnlyList<PropertyVariation> Variations => EmtpyVariations;

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;

		public void SetValue<T> (object target, T value)
		{
			this.propertyInfo.SetValue (target, value);
		}

		public T GetValue<T> (object target)
		{
			return (T)this.propertyInfo.GetValue (target);
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

		private readonly Lazy<string> category;

		private readonly PropertyInfo propertyInfo;

		private static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		private static readonly PropertyVariation[] EmtpyVariations = new PropertyVariation[0];
	}
}