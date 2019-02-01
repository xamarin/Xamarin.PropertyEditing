using System;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CustomExpressionView : BasePopOverViewModelControl
	{
		Type vmType;
		const string CustomExpressionPropertyString = "CustomExpression";
		PropertyInfo customExpressionPropertyInfo;

		public CustomExpressionView (IHostResourceProvider hostResources, PropertyViewModel viewModel)
			: base (hostResources, viewModel,  Properties.Resources.CustomExpression, "pe-custom-expression-32")
		{
			Frame = new CGRect (CGPoint.Empty, new CGSize (250, 80));

			var customExpressionField = new NSTextField {
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.vmType = viewModel.GetType ();
			this.customExpressionPropertyInfo = vmType.GetProperty (CustomExpressionPropertyString);
			var value = this.customExpressionPropertyInfo.GetValue (viewModel);
			if (value != null)
				customExpressionField.StringValue = (string)value;

			customExpressionField.Activated += (sender, e) => {
				this.customExpressionPropertyInfo.SetValue (viewModel, customExpressionField.StringValue);
			};

			AddSubview (customExpressionField);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (customExpressionField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 37f),
				NSLayoutConstraint.Create (customExpressionField, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 38f),
				NSLayoutConstraint.Create (customExpressionField, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -57f),
				NSLayoutConstraint.Create (customExpressionField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),
			});
		}
	}
}
