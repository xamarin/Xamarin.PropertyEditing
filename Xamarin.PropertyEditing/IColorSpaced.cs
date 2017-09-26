using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	// TODO: Add support for predefined color values
	public interface IColorSpaced
	{
		IReadOnlyList<string> ColorSpaces { get; }
	}
}
