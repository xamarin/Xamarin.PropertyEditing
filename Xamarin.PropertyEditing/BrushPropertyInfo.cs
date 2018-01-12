using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing
{
	public class BrushPropertyInfo : IComplexPropertyInfo, IColorSpaced
	{
		public BrushPropertyInfo (string name, string category, bool canWrite,
			IReadOnlyList<string> colorSpaces = null, ValueSources valueSources = ValueSources.Local,
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

		public Type Type => typeof (CommonBrush);

		public string Category { get; }

		public bool CanWrite { get; }

		public ValueSources ValueSources { get; }

		public IReadOnlyList<PropertyVariation> Variations { get; }

		public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }

		public IReadOnlyCollection<ISubPropertyInfo> Properties =>
			this.properties ?? (
			this.properties = new ISubPropertyInfo[] {
				OpacityInfo,
				ColorSpaceInfo,
				ColorInfo
			});

		public OpacityPropertyInfo OpacityInfo =>
			this.opacityInfo ?? (
			this.opacityInfo = new OpacityPropertyInfo (this));

		public ColorSpacePropertyInfo ColorSpaceInfo =>
			this.colorSpaceInfo ?? (
			this.colorSpaceInfo = new ColorSpacePropertyInfo (this));

		public ColorPropertyInfo ColorInfo =>
			this.colorInfo ?? (
			this.colorInfo = new ColorPropertyInfo (this));

		OpacityPropertyInfo opacityInfo;
		ColorSpacePropertyInfo colorSpaceInfo;
		ColorPropertyInfo colorInfo;
		IReadOnlyCollection<ISubPropertyInfo> properties;

		public class OpacityPropertyInfo : ISubPropertyInfo
		{
			public OpacityPropertyInfo (BrushPropertyInfo parent)
			{
				ParentProperty = parent;
			}

			public IComplexPropertyInfo ParentProperty { get; }

			public string Name => nameof(BrushPropertyViewModel.Opacity);

			public Type Type => typeof (double);

			public string Category => null;

			public bool CanWrite => true;

			public ValueSources ValueSources =>
				ValueSources.Local | ValueSources.Default;

			public IReadOnlyList<PropertyVariation> Variations => null;

			public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => null;
		}

		public class ColorSpacePropertyInfo : ISubPropertyInfo
		{
			public ColorSpacePropertyInfo (BrushPropertyInfo parent)
			{
				ParentProperty = parent;
			}

			public IComplexPropertyInfo ParentProperty { get; }

			public string Name => nameof (SolidBrushViewModel.ColorSpace);

			public Type Type => typeof (string);

			public string Category => null;

			public bool CanWrite => true;

			public ValueSources ValueSources =>
				ValueSources.Local | ValueSources.Default;

			public IReadOnlyList<PropertyVariation> Variations => null;

			public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => null;
		}

		public class ColorPropertyInfo : ISubPropertyInfo
		{
			public ColorPropertyInfo(BrushPropertyInfo parent)
			{
				ParentProperty = parent;
			}

			public IComplexPropertyInfo ParentProperty { get; }

			public string Name => nameof(SolidBrushViewModel.Color);

			public Type Type => typeof(CommonColor);

			public string Category => null;

			public bool CanWrite => true;

			public ValueSources ValueSources => ValueSources.Local | ValueSources.Default;

			public IReadOnlyList<PropertyVariation> Variations => null;

			public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => null;
		}
	}
}
