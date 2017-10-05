using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IAvailabilityConstraint
	{
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