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
	internal class CustomExpressionView : BasePopOverViewModelControl
	{
		private const string CustomExpressionPropertyString = "CustomExpression";
		private const string PreviewCustomExpressionString = "PreviewCustomExpression";
		private const string AutocompleteItemsString = "AutocompleteItems";

		public AutoClosePopOver PopOver { get; internal set; }

		public CustomExpressionView (IHostResourceProvider hostResources, PropertyViewModel viewModel)
			: base (hostResources, viewModel,  Properties.Resources.CustomExpression, "pe-custom-expression-32")
		{
			Frame = new CGRect (CGPoint.Empty, new CGSize (250, 80));

			Type vmType = viewModel.GetType ();

			PropertyInfo previewCustomExpressionPropertyInfo = vmType.GetProperty (PreviewCustomExpressionString);
			previewCustomExpressionPropertyInfo.SetValue (viewModel, string.Empty);

			PropertyInfo customExpressionPropertyInfo = vmType.GetProperty (CustomExpressionPropertyString);
			var value = customExpressionPropertyInfo.GetValue (viewModel);

			NSControl editorControl = null;
			PropertyInfo customAutocompleteItemsPropertyInfo = vmType.GetProperty (AutocompleteItemsString);
			if (customAutocompleteItemsPropertyInfo.GetValue (viewModel) is ObservableCollectionEx<string> values) {
				if (values != null && values.Count > 0)
					editorControl = new AutocompleteComboBox (hostResources, viewModel, values, previewCustomExpressionPropertyInfo) {
						AccessibilityEnabled = true,
						AccessibilityTitle = Properties.Resources.AccessibilityCustomExpressionCombobox,
					};
			}

			if (editorControl == null)
				editorControl = new NSTextField {
					AccessibilityEnabled = true,
					AccessibilityTitle = Properties.Resources.AccessibilityCustomExpressionEditControl,
				};

			editorControl.TranslatesAutoresizingMaskIntoConstraints = false;
			editorControl.StringValue = (string)value ?? string.Empty;

			editorControl.Activated += (sender, e) => {
				PopOver.CloseOnEnter = true;
				customExpressionPropertyInfo.SetValue (viewModel, editorControl.StringValue);
			};

			AddSubview (editorControl);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (editorControl, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 37f),
				NSLayoutConstraint.Create (editorControl, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Left, 1f, 38f),
				NSLayoutConstraint.Create (editorControl, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -57f),
				NSLayoutConstraint.Create (editorControl, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),
			});
		}
	}
}
