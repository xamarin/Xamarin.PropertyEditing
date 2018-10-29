using System;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateVariationEventArgs
		:  EventArgs
	{
		public PropertyVariation Variation
		{
			get;
			set;
		}
	}
}