using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	/// <summary>
	/// Light-up interface to handle auto-completion of values.
	/// </summary>
	/// <remarks>
	/// To be implemented on <see cref="IObjectEditor"/> implementations. Requires <see cref="TargetPlatform.SupportsCustomExpressions"/>.
	/// </remarks>
	public interface ICompleteValues
	{
		/// <summary>
		/// Gets whether or not the string can be looked at for auto-completion.
		/// </summary>
		/// <returns>
		/// <c>true</c> if <paramref name="input"/> contains auto-completable markers, <c>false</c> otherwise.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This is not an indicator of whether a value to autocomplete the <paramref name="input"/> exists, but
		/// instead to indicate whether it is a special string that has auto-completion implications. Examples
		/// of this are '@' in Android, or '{' in XAML languages.
		/// </para>
		/// <para>
		/// Additionally this is not the only gating mechanic for auto-completion. UI elements that would always
		/// auto-complete may bypass this check altogether. Instead this is for inputs that may not auto-complete,
		/// but could be enabled to given the correct input.
		/// </para>
		/// </remarks>
		bool CanAutocomplete (string input);

		/// <returns>
		/// A list of strings to be used to set the value of the <paramref name="property"/> if selected via <see cref="ValueInfo.CustomExpression"/>.
		/// </returns>
		/// <remarks>
		/// Returned strings are not post-sorted nor count-trimmed. Across multiple editors, only the common elements will be present
		/// in order of the first editor. The general assumption is that order would likely be the same across editors.
		/// </remarks>
		Task<IReadOnlyList<string>> GetCompletionsAsync (IPropertyInfo property, string input, CancellationToken cancellationToken);
	}
}
