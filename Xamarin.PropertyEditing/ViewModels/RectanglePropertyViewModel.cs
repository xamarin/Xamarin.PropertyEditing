using System.Collections.Generic;
using System.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class RectanglePropertyViewModel
		: PropertyViewModel<Rectangle>
	{
		public RectanglePropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
		}
	}
}
