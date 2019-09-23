using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IAvailabilityConstraint
	{
		/// <summary>
		/// Gets a property that serves as the parent of this property for visual organization.
		/// </summary>
		/// <remarks>
		/// <p>Return <c>null</c> if there is no parent organizing property.</p>
		/// <p>If the value of <c>ParentProperty</c> affects the constraint itself, it still must be included in
		/// <see cref="ConstrainingProperties"/>.</p>
		/// </remarks>
		IPropertyInfo ParentProperty { get; }

		/// <summary>
		/// Gets a list of properties associated with the constraint.
		/// </summary>
		/// <remarks>
		/// This list can be empty (or <c>null</c>) if the constraint is not based on regular properties. Properties listed here will
		/// be monitored for changes and the availability re-queried when they do change. The list of properties will not be monitored
		/// for changes.
		/// </remarks>
		IReadOnlyList<IPropertyInfo> ConstrainingProperties { get; }

		/// <exception cref="ArgumentNullException"><paramref name="editor"/> is <c>null</c>.</exception>
		Task<bool> GetIsAvailableAsync (IObjectEditor editor);
	}
}