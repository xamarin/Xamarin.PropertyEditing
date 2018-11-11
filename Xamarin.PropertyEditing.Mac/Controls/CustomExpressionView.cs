using System;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.Mac.Controls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CustomExpressionView<T> : BasePopOverViewModelControl
	{
		public CustomExpressionView (PropertyViewModel<T> viewModel) : base (viewModel,  Properties.Resources.CustomExpression, "custom-expression-32")
		{
			Frame = new CGRect (CGPoint.Empty, new CGSize (250, 80));

			NSControl editorControl;

			viewModel.PreviewCustomExpression = string.Empty;
			if (viewModel.AutocompleteItems != null && viewModel.AutocompleteItems.Count > 0) {
				editorControl = new AutocompleteComboBox<T> (viewModel) {
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
			} else {
				editorControl = new NSTextField {
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
			}

			editorControl.StringValue = viewModel.CustomExpression ?? string.Empty;

			editorControl.Activated += (sender, e) => {
				viewModel.CustomExpression = editorControl.StringValue;
			};

			AddSubview (editorControl);

			this.DoConstraints (new[] {
				editorControl.ConstraintTo (this, (s, c) => s.Top == c.Top + 37),
				editorControl.ConstraintTo (this, (s, c) => s.Left == c.Left + 38),
				editorControl.ConstraintTo (this, (s, c) => s.Width == c.Width - 57),
				editorControl.ConstraintTo (this, (s, c) => s.Height == 24),
			});
		}
	}
}
