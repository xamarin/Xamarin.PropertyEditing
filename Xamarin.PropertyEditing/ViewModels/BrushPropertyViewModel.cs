using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class BrushPropertyViewModel : PropertyViewModel<CommonBrush>
	{
		public BrushPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			if (property.Type.IsAssignableFrom (typeof (CommonSolidBrush))) {
				Solid = new SolidBrushViewModel (this,
					property is IColorSpaced colorSpacedPropertyInfo ? colorSpacedPropertyInfo.ColorSpaces :  null);
			}
		}

		public SolidBrushViewModel Solid { get; }
	}
}
