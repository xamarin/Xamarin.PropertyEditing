using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SolidBrushPropertyViewModel : PropertyViewModel<CommonSolidBrush>
	{
		public SolidBrushPropertyViewModel(IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base(property, editors)
		{
			var solidBrushPropertyInfo = property as ISolidBrushPropertyInfo;
			if (solidBrushPropertyInfo == null)
				throw new ArgumentException ("Property doesn't implement ISolidBrushPropertyInfo", nameof (property));

			ColorSpaces = solidBrushPropertyInfo.ColorSpaces;
		}

		public IReadOnlyList<string> ColorSpaces { get; }
	}
}
