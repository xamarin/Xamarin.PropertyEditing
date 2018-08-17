using System;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateVariantEventArgs
		:  EventArgs
	{
		public PropertyVariationSet Variant
		{
			get;
			set;
		}
	}
}