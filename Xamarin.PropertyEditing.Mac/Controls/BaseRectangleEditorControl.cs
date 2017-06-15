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
		protected UnfocusableTextView XLabel { get; set; }
		protected NSTextField XEditor { get; set; }
		protected UnfocusableTextView YLabel { get; set; }
		protected NSTextField YEditor { get; set; }
		protected UnfocusableTextView WidthLabel { get; set; }
		protected NSTextField WidthEditor { get; set; }
		protected UnfocusableTextView HeightLabel { get; set; }
		protected NSTextField HeightEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => HeightEditor;

		internal new PropertyViewModel<T> ViewModel {
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public BaseRectangleEditorControl ()
		{
			XLabel = new UnfocusableTextView ();
			XEditor = new NSTextField ();
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.StringValue = string.Empty;
			XEditor.Activated += OnInputUpdated;
			XEditor.EditingEnded += OnInputUpdated;

			YLabel =  new UnfocusableTextView ();
			YEditor = new NSTextField ();
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.StringValue = string.Empty;
			YEditor.Activated += OnInputUpdated;
			YEditor.EditingEnded += OnInputUpdated;

			WidthLabel = new UnfocusableTextView ();
			WidthEditor = new NSTextField ();
			WidthEditor.BackgroundColor = NSColor.Clear;
			WidthEditor.StringValue = string.Empty;
			WidthEditor.Activated += OnInputUpdated;
			WidthEditor.EditingEnded += OnInputUpdated;

			HeightLabel =  new UnfocusableTextView ();
			HeightEditor = new NSTextField ();
			HeightEditor.BackgroundColor = NSColor.Clear;
			HeightEditor.StringValue = string.Empty;
			HeightEditor.Activated += OnInputUpdated;
			HeightEditor.EditingEnded += OnInputUpdated;

			AddSubview (XLabel);
			AddSubview (XEditor);
			AddSubview (YLabel);
			AddSubview (YEditor);
			AddSubview (WidthLabel);
			AddSubview (WidthEditor);
			AddSubview (HeightLabel);
			AddSubview (HeightEditor);
		}

		protected virtual void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = (T)Activator.CreateInstance (typeof(T), XEditor.FloatValue, YEditor.FloatValue, WidthEditor.FloatValue, HeightEditor.FloatValue);
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
