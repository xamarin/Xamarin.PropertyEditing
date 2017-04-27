using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseRectangleEditorControl<T> : PropertyEditorControl
		where T : struct
	{
		protected NSTextView XLabel { get; set; }
		protected NSTextField XEditor { get; set; }
		protected NSTextView YLabel { get; set; }
		protected NSTextField YEditor { get; set; }
		protected NSTextView WidthLabel { get; set; }
		protected NSTextField WidthEditor { get; set; }
		protected NSTextView HeightLabel { get; set; }
		protected NSTextField HeightEditor { get; set; }

		internal new PropertyViewModel<T> ViewModel {
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public BaseRectangleEditorControl ()
		{
			var typeParameterType = typeof (T);

			XLabel = new NSTextView ();
			XEditor = new NSTextField ();
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.StringValue = string.Empty;
			XEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue, WidthEditor.FloatValue, HeightEditor.FloatValue);
			};

			YLabel = new NSTextView ();
			YEditor = new NSTextField ();
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.StringValue = string.Empty;
			YEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue, WidthEditor.FloatValue, HeightEditor.FloatValue);
			};

			WidthLabel = new NSTextView ();
			WidthEditor = new NSTextField ();
			WidthEditor.BackgroundColor = NSColor.Clear;
			WidthEditor.StringValue = string.Empty;
			WidthEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue, WidthEditor.FloatValue, HeightEditor.FloatValue);
			};

			HeightLabel = new NSTextView ();
			HeightEditor = new NSTextField ();
			HeightEditor.BackgroundColor = NSColor.Clear;
			HeightEditor.StringValue = string.Empty;
			HeightEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue, WidthEditor.FloatValue, HeightEditor.FloatValue);
			};

			AddSubview (XLabel);
			AddSubview (XEditor);
			AddSubview (YLabel);
			AddSubview (YEditor);
			AddSubview (WidthLabel);
			AddSubview (WidthEditor);
			AddSubview (HeightLabel);
			AddSubview (HeightEditor);
		}

		protected override abstract void UpdateModelValue ();

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				XEditor.BackgroundColor = NSColor.Red;
				YEditor.BackgroundColor = NSColor.Red;
				WidthEditor.BackgroundColor = NSColor.Red;
				HeightEditor.BackgroundColor = NSColor.Red;
				Debug.WriteLine ("Your input triggered an error:");
				foreach (var error in errors) {
					Debug.WriteLine (error.ToString () + "\n");
				}
			}
			else {
				XEditor.BackgroundColor = NSColor.Clear;
				YEditor.BackgroundColor = NSColor.Clear;
				WidthEditor.BackgroundColor = NSColor.Clear;
				HeightEditor.BackgroundColor = NSColor.Clear;
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			XEditor.Editable = ViewModel.Property.CanWrite;
			YEditor.Editable = ViewModel.Property.CanWrite;
			WidthEditor.Editable = ViewModel.Property.CanWrite;
			HeightEditor.Editable = ViewModel.Property.CanWrite;
		}
	}
}
