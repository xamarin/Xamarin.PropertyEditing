using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorEditorControl : PropertyEditorControl
	{
		internal NSColorWell ColorEditor { get; set; }
		internal NSTextView ColorLabel { get; set; }

		public override NSView FirstKeyView => ColorEditor;
		public override NSView LastKeyView => ColorEditor;

		public ColorEditorControl ()
		{
			ColorEditor = new NSColorWell (new CGRect (0, 0, 40, 20));

			ColorLabel = new NSTextView (new CGRect (45, 0, 250, 20)) {
				Value = string.Empty,
			};

			// update the value on 'enter'
			ColorEditor.Activated += (sender, e) => {
				ViewModel.Value = ColorEditor.Color;
			};
			AddSubview (ColorEditor);
            AddSubview (ColorLabel);
		}

		internal new PropertyViewModel<NSColor> ViewModel {
			get { return (PropertyViewModel<NSColor>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PropertyViewModel<CGColor>.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			base.UpdateModelValue ();
			ColorEditor.Color = ViewModel.Value ?? NSColor.Clear;
			ColorLabel.Value = ColorEditor.Color.ToString ();
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				//ColorEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				// ColorEditor.BackgroundColor = NSColor.Clear;
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			ColorEditor.Enabled = ViewModel.Property.CanWrite;
		}
	}
}
