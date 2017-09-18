using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class BrushPropertyViewModelTests : PropertyViewModelTests<CommonBrush, PropertyViewModel<CommonBrush>>
	{
		protected override PropertyViewModel<CommonBrush> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new BrushPropertyViewModel (property, editors);
		}
	}
}
