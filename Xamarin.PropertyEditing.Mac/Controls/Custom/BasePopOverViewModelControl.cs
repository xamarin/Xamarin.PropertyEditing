using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverViewModelControl : BasePopOverControl
	{
		internal PropertyViewModel ViewModel { get; }

		public BasePopOverViewModelControl (PropertyViewModel viewModel, string title, string imageNamed) : base (title, imageNamed)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			ViewModel = viewModel;
		}
	}
}
