using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PointEditorControl<T> : PropertyEditorControl
		where T : struct
	{
		internal NSTextField XEditor { get; set; }
		internal NSTextField YEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => YEditor;

		internal new PropertyViewModel<T> ViewModel {
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public PointEditorControl ()
		{
			var typeParameterType = typeof (T);
			var isPoint = typeof (T) == typeof (CGPoint);

			var xLabel = new UnfocusableTextView (new CGRect (0, -5, 25, 20), isPoint ? "X:" : "Width:");

			XEditor = new NSTextField (new CGRect (25, 0, 50, 20));
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.StringValue = string.Empty;
			XEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue);
			};

			var yLabel = new UnfocusableTextView (new CGRect (85, -5, 25, 20), isPoint ? "Y:" : "Height:" );

			YEditor = new NSTextField (new CGRect (110, 0, 50, 20));
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.StringValue = string.Empty;

			// update the value on 'enter'
			YEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue);
			};

			AddSubview (xLabel);
			AddSubview (XEditor);
            AddSubview (yLabel);
            AddSubview (YEditor);
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PropertyViewModel<T>.Value)) {
				UpdateModelValue ();
			}
		}

		protected override void UpdateModelValue ()
		{
			base.UpdateModelValue ();
			if (typeof (T) == typeof (CGPoint)) {
				var pointType = ViewModel.Value.GetType ();
				var xProp = pointType.GetProperty ("X");
				var xValue = (nfloat)xProp.GetValue (ViewModel.Value);
				var yProp = pointType.GetProperty ("Y");
				var yValue = (nfloat)yProp.GetValue (ViewModel.Value);

				CGPoint point = new CGPoint (xValue, yValue);
				XEditor.StringValue = point.X.ToString ();
				YEditor.StringValue = point.Y.ToString ();
			}
			if (typeof (T) == typeof (CGSize)) {
				var pointType = ViewModel.Value.GetType ();
				var widthProp = pointType.GetProperty ("Width");
				var widthValue = (nfloat)widthProp.GetValue (ViewModel.Value);
				var heightProp = pointType.GetProperty ("Height");
				var heightValue = (nfloat)heightProp.GetValue (ViewModel.Value);

				CGSize size = new CGSize (widthValue, heightValue);
				XEditor.StringValue = size.Width.ToString ();
				YEditor.StringValue = size.Width.ToString ();
			}
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
