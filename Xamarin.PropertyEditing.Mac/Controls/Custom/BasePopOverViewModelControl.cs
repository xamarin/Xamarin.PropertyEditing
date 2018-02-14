using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverViewModelControl : BasePopOverControl
	{
		internal IEnumerable<object> Targets => this.ViewModel.Editors.Select (ed => ed.Target);

		internal IPropertyInfo Property => this.ViewModel.Property;

		internal PropertyViewModel ViewModel { get; }

		public BasePopOverViewModelControl (PropertyViewModel viewModel, string title, string imageNamed) : base (title, imageNamed)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.ViewModel = viewModel;
		}
	}
}
