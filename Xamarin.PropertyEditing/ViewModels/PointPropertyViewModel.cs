using System.Collections.Generic;
using System.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PointPropertyViewModel
		: PropertyViewModel<Point>
	{
		public PointPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
					: base (property, editors)
		{
		}
	}
}
