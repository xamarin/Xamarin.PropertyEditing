using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		/// <summary>
		/// Gets the possible sources of values for this property.
		/// </summary>
		ValueSources ValueSources { get; }

		IReadOnlyList<PropertyVariation> Variations { get; }

		IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }
	}
}
