using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverViewModelControl : BasePopOverControl
	{
		internal EditorViewModel ViewModel { get; }

		public BasePopOverViewModelControl (IHostResourceProvider hostResources, EditorViewModel viewModel, string title, string imageNamed)
			: base (hostResources, title, imageNamed)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			ViewModel = viewModel;
		}
	}
}
