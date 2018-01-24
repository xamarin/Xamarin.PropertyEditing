using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePopOverViewModelControl : BasePopOverControl
	{
		internal IEnumerable<object> Targets => this.viewModel.Editors.Select (ed => ed.Target);

		internal IPropertyInfo Property => this.viewModel.Property;

		private PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel => this.viewModel;

		public BasePopOverViewModelControl (PropertyViewModel viewModel, string title, string imageNamed) : base (title, imageNamed)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.viewModel = viewModel;
		}
	}
}
