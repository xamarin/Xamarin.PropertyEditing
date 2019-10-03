using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public interface IHavePredefinedValues<TValue>
	{
		/// <summary>
		/// Gets whether the value should be constrained to those that are predefined.
		/// </summary>
		/// <remarks>
		/// If this is <c>false</c>, <see cref="ValueInfo{T}.ValueDescriptor"/> should contain a string containing any
		/// value that is not already handled or part of the <see cref="PredefinedValues"/> list.
		/// </remarks>
		bool IsConstrainedToPredefined { get; }

		/// <summary>
		/// Gets whether multiple values can be set together
		/// </summary>
		/// <remarks>
		/// The object editor will need to be able to accept an <see cref="IReadOnlyList{TValue}"/> for the values specified.
		/// </remarks>
		bool IsValueCombinable { get; }

		string SeparatorString { get; }

		IReadOnlyDictionary<string, TValue> PredefinedValues { get; }
	}
}