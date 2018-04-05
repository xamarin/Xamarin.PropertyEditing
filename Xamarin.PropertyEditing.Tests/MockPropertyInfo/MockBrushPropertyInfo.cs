using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockBrushPropertyInfo : IPropertyInfo, IColorSpaced, IEquatable<MockBrushPropertyInfo>
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

		public bool Equals (MockBrushPropertyInfo other)
		{
			if (other is null)
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return Name == other.Name
				&& Category == other.Category
				&& CanWrite == other.CanWrite;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (GetType () != obj.GetType ())
				return false;

			return Equals ((MockBrushPropertyInfo)obj);
		}

		public override int GetHashCode ()
		{
			var hashCode = -315478821;
			unchecked {
				if (Name != null)
					hashCode = hashCode * -1521134295 + Name.GetHashCode ();
				if (Category != null)
					hashCode = hashCode * -1521134295 + Category.GetHashCode ();
				hashCode = hashCode * -1521134295 + CanWrite.GetHashCode ();
			}
			return hashCode;
		}
	}
}
