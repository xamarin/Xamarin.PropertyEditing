using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using ObjCRuntime;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorEditorControl : PropertyEditorControl
	{
		const string setBezelColorSelector = "setBezelColor:";

		MacColorButton ColorEditor { get; set; }

		PredefinedColor[] predefinedColors;

		public override NSView FirstKeyView => ColorEditor;
		public override NSView LastKeyView => ColorEditor;

		public ColorEditorControl ()
		{
			RefreshPredefinedColors ();

			ColorEditor = new MacColorButton (MacColorButton.Mode.WithText, predefinedColors);

			// update the value on 'enter'
			ColorEditor.CommitEvent += (NSColor color) => {
				Debug.WriteLine ("{0}", color);
				ViewModel.Value = color;
			};
			AddSubview (ColorEditor);

			this.DoConstraints (new[] {
				ColorEditor.ConstraintTo (this, (ce, c) => ce.Width == c.Width),
				ColorEditor.ConstraintTo (this, (ce, c) => ce.Left == c.Left),
			});
		}

		void RefreshPredefinedColors ()
		{
			predefinedColors = new PredefinedColor[] {
				PredefinedColor.New (null, "Black", MacColorButton.ColorToString(NSColor.Black.UsingColorSpace (NSColorSpace.GenericRGBColorSpace), true)),
				PredefinedColor.New (null, "Dark Gray", MacColorButton.ColorToString(NSColor.DarkGray.UsingColorSpace (NSColorSpace.GenericRGBColorSpace), true)),
				PredefinedColor.New (null, "Light Gray", MacColorButton.ColorToString(NSColor.LightGray.UsingColorSpace (NSColorSpace.GenericRGBColorSpace), true)),
				PredefinedColor.New (null, "White", MacColorButton.ColorToString(NSColor.White.UsingColorSpace (NSColorSpace.GenericRGBColorSpace), true)),
				PredefinedColor.New (null, "Clear", MacColorButton.ColorToString(NSColor.Clear.UsingColorSpace (NSColorSpace.GenericRGBColorSpace), true)),
			};
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
			ColorEditor.SetColor (ViewModel.Value ?? NSColor.Clear);
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				if (ColorEditor.RespondsToSelector (new Selector (setBezelColorSelector))) {
					ColorEditor.BezelColor = NSColor.Red;
				}
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				if (ColorEditor.RespondsToSelector (new Selector (setBezelColorSelector))) {
					ColorEditor.BezelColor = NSColor.Clear;
				}
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			ColorEditor.Enabled = ViewModel.Property.CanWrite;
		}
	}
}
