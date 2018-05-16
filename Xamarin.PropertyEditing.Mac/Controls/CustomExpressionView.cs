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

		public CustomExpressionView (PropertyViewModel viewModel) : base (viewModel,  Properties.Resources.CustomExpression, "custom-expression-32")
		{
			Frame = new CGRect (CGPoint.Empty, new CGSize (250, 80));

			var customExpressionField = new NSTextField {
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			vmType = viewModel.GetType ();
			customExpressionPropertyInfo = vmType.GetProperty (CustomExpressionPropertyString);
			var value = customExpressionPropertyInfo.GetValue (viewModel);
			if (value != null)
				customExpressionField.StringValue = (string)value;

			customExpressionField.Activated += (sender, e) => {
				customExpressionPropertyInfo.SetValue (viewModel, customExpressionField.StringValue);
			};

			AddSubview (customExpressionField);

			this.DoConstraints (new[] {
				customExpressionField.ConstraintTo (this, (s, c) => s.Top == c.Top + 37),
				customExpressionField.ConstraintTo (this, (s, c) => s.Left == c.Left + 38),
				customExpressionField.ConstraintTo (this, (s, c) => s.Width == c.Width - 57),
				customExpressionField.ConstraintTo (this, (s, c) => s.Height == 24),
			});
		}
	}
}
