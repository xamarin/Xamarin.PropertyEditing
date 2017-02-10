using System;
using System.Collections;
using System.Diagnostics;
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
			StringEditor.StringValue = string.Empty;

			// update the value on 'enter'
			StringEditor.Activated += (sender, e) => {
				ViewModel.Value = StringEditor.StringValue;
			};
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

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				StringEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			} else {
				StringEditor.BackgroundColor = NSColor.Clear;
			}
		}
	}
}
