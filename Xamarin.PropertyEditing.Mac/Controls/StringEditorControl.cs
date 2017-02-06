using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StringEditorControl : PropertyEditorControl
	{
		public StringEditorControl ()
		{
			StringEditor = new NSTextField (new CGRect (0, 0, 150, 20));
			StringEditor.BackgroundColor = NSColor.Clear;
			StringEditor.StringValue = "";
			AddSubview (StringEditor);
		}

		internal NSTextField StringEditor { get; set; }

		internal new StringPropertyViewModel ViewModel {
			get { return (StringPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (StringPropertyViewModel.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			StringEditor.StringValue = ViewModel.Value ?? string.Empty;
		}
	}
}
