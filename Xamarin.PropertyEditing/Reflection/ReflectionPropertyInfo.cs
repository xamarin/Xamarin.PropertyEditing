using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionPropertyInfo
		: IPropertyInfo
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

		private readonly Lazy<string> category;

		private readonly PropertyInfo propertyInfo;

		private static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		private static readonly PropertyVariation[] EmtpyVariations = new PropertyVariation[0];
	}
}
