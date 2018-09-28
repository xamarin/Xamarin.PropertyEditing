using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public class InputMode
	{
		public InputMode (string identifier, bool isSingleValue = false)
		{
			if (identifier == null)
				throw new ArgumentNullException (nameof(identifier));

			Identifier = identifier;
			IsSingleValue = isSingleValue;
		}

		public string Identifier
		{
			get;
		}

		public bool IsSingleValue
		{
			get;
		}
	}

	/// <summary>
	/// Indicates a property has input modes.
	/// </summary>
	/// <remarks>
	/// Implemented on <see cref="IPropertyInfo"/> instances.
	/// </remarks>
	public interface IHaveInputModes
	{
		IReadOnlyList<InputMode> InputModes { get; }
	}
}
