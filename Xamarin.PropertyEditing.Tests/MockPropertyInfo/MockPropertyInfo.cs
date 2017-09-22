using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockPropertyInfo<T> : IPropertyInfo
	{
		public MockPropertyInfo (string name, string category = "", bool canWrite = true)
		{
			Name = name;
			Category = category;
			CanWrite = canWrite;
		}

		public string Name { get; }
		public Type Type => typeof (T);
		public string Category { get; }
		public bool CanWrite { get; }
		public virtual ValueSources ValueSources => ValueSources.Local;
		static readonly PropertyVariation[] EmptyVariations = new PropertyVariation[0];
		public virtual IReadOnlyList<PropertyVariation> Variations => EmptyVariations;
		static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		public virtual IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;
	}
}
