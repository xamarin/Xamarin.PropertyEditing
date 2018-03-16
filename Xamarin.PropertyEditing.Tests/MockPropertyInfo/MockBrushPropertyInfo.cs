using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockBrushPropertyInfo : IPropertyInfo, IColorSpaced
	{
		public MockBrushPropertyInfo (string name, string category, bool canWrite, string description = null,
			IReadOnlyList<string> colorSpaces = null, ValueSources valueSources = ValueSources.Default | ValueSources.Local  |  ValueSources.Resource,
			IReadOnlyList<PropertyVariation> variations = null,
			IReadOnlyList<IAvailabilityConstraint> availabilityConstraints = null)
		{
			Name = name;
			Description = description;
			Category = category;
			CanWrite = canWrite;
			ColorSpaces = colorSpaces;
			ValueSources = valueSources;
			Variations = variations;
			AvailabilityConstraints = availabilityConstraints;
		}

		public IReadOnlyList<string> ColorSpaces { get; }

		public string Name { get; }

		public string Description { get; }

		public Type Type => typeof(CommonBrush);

		public string Category { get; }

		public bool CanWrite { get; }

		public ValueSources ValueSources { get; }

		public IReadOnlyList<PropertyVariation> Variations { get; }

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }
	}
}
