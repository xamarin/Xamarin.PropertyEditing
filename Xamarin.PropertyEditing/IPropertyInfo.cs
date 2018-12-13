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
		/// Gets whether the property is an uncommonly used property.
		/// </summary>
		/// <remarks>
		/// This acts as a hint to hide the property behind disclosures when appropriate.
		/// </remarks>
		bool IsUncommon { get; }

		/// <summary>
		/// Gets the possible sources of values for this property.
		/// </summary>
		ValueSources ValueSources { get; }

		/// <summary>
		/// Gets a list of possible variations of the property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// These are essentially conditions that can be applied for when a specific value for a property is used.
		/// They should not include a neutral state.
		/// </para>
		/// </remarks>
		IReadOnlyList<PropertyVariationOption> Variations { get; }

		IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints { get; }
	}
}
