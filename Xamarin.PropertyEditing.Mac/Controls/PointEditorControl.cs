using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PointEditorControl : PropertyEditorControl
	{
		internal NSTextField XEditor { get; set; }
		internal NSTextField YEditor { get; set; }

		internal new PointPropertyViewModel ViewModel {
			get { return (PointPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public PointEditorControl ()
		{
			var xLabel = new NSTextView (new CGRect (0, 0, 20, 20)) {
				Value = "X:", 
			};

			XEditor = new NSTextField (new CGRect (25, 0, 50, 20));
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.StringValue = string.Empty;

			var yLabel = new NSTextView (new CGRect (75, 0, 20, 20)) {
				Value = "Y:", 
			};

			YEditor = new NSTextField (new CGRect (80, 0, 50, 20));
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.StringValue = string.Empty;

			// update the value on 'enter'
			XEditor.Activated += (sender, e) => {
				ViewModel.Value = new Point (XEditor.IntValue, YEditor.IntValue);
			};

			AddSubview (xLabel);
			AddSubview (XEditor);
            AddSubview (yLabel);
            AddSubview (YEditor);
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PointPropertyViewModel.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			base.UpdateModelValue ();
			XEditor.IntValue = ViewModel.Value.X;
			YEditor.IntValue = ViewModel.Value.Y;
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				XEditor.BackgroundColor = NSColor.Red;
				YEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered 1 or more errors:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				XEditor.BackgroundColor = NSColor.Clear;
				YEditor.BackgroundColor = NSColor.Clear;
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			XEditor.Editable = ViewModel.Property.CanWrite;
			YEditor.Editable = ViewModel.Property.CanWrite;
		}
	}
}
