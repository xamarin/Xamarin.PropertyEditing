using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class BrushPropertyViewModel : PropertyViewModel<CommonBrush>
	{
		public BrushPropertyViewModel(IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base(property, editors)
		{
		}
	}
}
