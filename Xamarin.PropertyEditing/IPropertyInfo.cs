using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public interface IPropertyInfo
	{
		string Name { get; }

		Type Type { get; }

		/// <summary>
		/// A resource-name of the category the property belongs to.
		/// </summary>
		string Category { get; }

		bool CanWrite { get; }

		/// <summary>
		/// Gets the possible sources of values for this property.
		/// </summary>
		ValueSources ValueSources { get; }

		IReadOnlyList<PropertyVariation> Variations { get; }

		IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }
	}
}
