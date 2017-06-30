using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public interface IHavePredefinedValues<TValue>
	{
		/// <summary>
		/// Gets whether the value should be constrained to those that are predefined.
		/// </summary>
		bool IsConstrainedToPredefined { get; }

		/// <summary>
		/// Gets whether multiple values can be set together
		/// </summary>
		/// <remarks>
		/// The object editor will need to be able to accept an <see cref="IReadOnlyList{TValue}"/> for the values specified.
		/// </remarks>
		bool IsValueCombinable { get; }

		IReadOnlyDictionary<string, TValue> PredefinedValues { get; }
	}
}