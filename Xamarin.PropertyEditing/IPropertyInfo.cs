using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public interface IPropertyInfo
	{
		string Name { get; }

		/// <summary>
		/// Gets a summary description of the property.
		/// </summary>
		/// <remarks>
		/// Currently does not support any form of markup.
		/// </remarks>
		string Description { get; }

		/// <summary>
		/// Gets the representative type for the property (Common*, primitive, etc).
		/// </summary>
		Type Type { get; }

		/// <summary>
		/// Gets the real type for the property (ex. Drawable instead of CommonBrush).
		/// </summary>
		ITypeInfo RealType { get; }

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
