using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StringEditorControl : PropertyEditorControl
	{
		internal StringEditorControl ()
		{
			StringEditor = new NSTextField (new CGRect (0, 0, 150, 20));
			StringEditor.BackgroundColor = NSColor.Clear;
			StringEditor.StringValue = "";
			AddSubview (StringEditor);
		}

		internal NSTextField StringEditor { get; set; }
		StringPropertyViewModel viewModel;

		internal StringPropertyViewModel ViewModel {
			get { return viewModel; }
			set {
				viewModel = value;
				if (ViewModel.Value == null)
					ViewModel.Value = string.Empty;
				StringEditor.StringValue = ViewModel.Value;
				value.PropertyChanged += (sender, e) => {
					if (e.PropertyName == nameof (StringPropertyViewModel.Value)) {
						StringEditor.StringValue = ViewModel.Value;
					}
				};
			}
		}
	}
}
