using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionSolidBrushPropertyInfo : ReflectionPropertyInfo, IColorSpaced
	{
		public ReflectionSolidBrushPropertyInfo (PropertyInfo propertyInfo, IEnumerable<string> colorSpaces = null)
			: base (propertyInfo)
		{
			ColorSpaces = colorSpaces?.ToArray();
		}

		public IReadOnlyList<string> ColorSpaces { get; }
	}
}
