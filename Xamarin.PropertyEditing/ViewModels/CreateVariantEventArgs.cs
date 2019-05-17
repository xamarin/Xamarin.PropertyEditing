using System;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateVariantEventArgs
		:  EventArgs
	{
		public Task<PropertyVariation> Variation
		{
			get;
			set;
		}
	}
}