using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing
{
	public class BrushPropertyInfo : IPropertyInfo, IColorSpaced
	{
		public BrushPropertyInfo (string name, string category, bool canWrite,
			IReadOnlyList<string> colorSpaces = null, ValueSources valueSources = ValueSources.Local | ValueSources.Default,
			IReadOnlyList<PropertyVariation> variations = null,
			IReadOnlyList<IAvailabilityConstraint> availabilityConstraints = null)
		{
			Name = name;
			Category = category;
			CanWrite = canWrite;
			ColorSpaces = colorSpaces;
			ValueSources = valueSources;
			Variations = variations;
			AvailabilityConstraints = availabilityConstraints;
		}

		public IReadOnlyList<string> ColorSpaces { get; }

		public string Name { get; }

		public Type Type => typeof(CommonBrush);

		public string Category { get; }

		public bool CanWrite { get; }

		public ValueSources ValueSources { get; }

		public IReadOnlyList<PropertyVariation> Variations { get; }

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }
	}
}
