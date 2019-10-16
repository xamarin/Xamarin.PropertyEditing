using System;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StatefulPropertyPopOverView
		: BasePopOverViewModelControl
	{
		private readonly StatefulPropertySelectorControl selector;

		public StatefulPropertyPopOverView (IHostResourceProvider hostResources, StatePropertyGroupViewModel viewModel)
			: base (hostResources, viewModel, Properties.Resources.Properties, "pe-custom-expression-32")
		{

			Frame = new CGRect (CGPoint.Empty, new CGSize (250, 160));

			Type vmType = viewModel.HostedProperty.GetType ();

			/* PropertyInfo previewCustomExpressionPropertyInfo = vmType.GetProperty (PreviewCustomExpressionString);
			previewCustomExpressionPropertyInfo.SetValue (viewModel, string.Empty);

			PropertyInfo customExpressionPropertyInfo = vmType.GetProperty (CustomExpressionPropertyString);
			var value = customExpressionPropertyInfo.GetValue (viewModel);*/

			this.selector = new StatefulPropertySelectorControl (hostResources) {
				ViewModel = viewModel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			AddSubview (this.selector);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 37f),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, -2),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -2),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (this.selector, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
			});
		}
	}
}
