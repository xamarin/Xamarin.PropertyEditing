using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing
{
	public class SolidBrushPropertyInfo : IPropertyInfo, IColorSpaced
	{
		public SolidBrushPropertyInfo (string name, string category, bool canWrite,
			IReadOnlyList<string> colorSpaces, ValueSources valueSources = ValueSources.Local,
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

		public Type Type => typeof(CommonSolidBrush);

		public string Category { get; }

		public bool CanWrite { get; }

		public ValueSources ValueSources { get; }

		public IReadOnlyList<PropertyVariation> Variations { get; }

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }
	}
}
