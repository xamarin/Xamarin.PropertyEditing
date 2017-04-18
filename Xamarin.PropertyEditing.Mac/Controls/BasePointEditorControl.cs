using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BasePointEditorControl<T> : PropertyEditorControl
		where T : struct
	{
		protected NSTextView XLabel { get; set; }
		protected NSTextField XEditor { get; set; }
		protected NSTextView YLabel { get; set; }
		protected NSTextField YEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => YEditor;

		internal new PropertyViewModel<T> ViewModel {
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public BasePointEditorControl ()
		{
			var typeParameterType = typeof (T);

			XLabel = new NSTextView ();

			XEditor = new NSTextField ();
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.StringValue = string.Empty;
			XEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue);
			};

			YLabel = new NSTextView ();

			YEditor = new NSTextField (new CGRect (110, 0, 50, 20));
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.StringValue = string.Empty;

			// update the value on 'enter'
			YEditor.Activated += (sender, e) => {
				ViewModel.Value = (T)Activator.CreateInstance (typeParameterType, XEditor.FloatValue, YEditor.FloatValue);
			};

			AddSubview (XLabel);
			AddSubview (XEditor);
            AddSubview (YLabel);
            AddSubview (YEditor);
		}

		protected override void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PropertyViewModel<T>.Value)) {
				UpdateModelValue ();
			}
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
