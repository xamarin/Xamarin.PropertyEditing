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
		protected UnfocusableTextField XLabel { get; set; }
		protected NumericSpinEditor XEditor { get; set; }
		protected UnfocusableTextField YLabel { get; set; }
		protected NumericSpinEditor YEditor { get; set; }
		protected UnfocusableTextField WidthLabel { get; set; }
		protected NumericSpinEditor WidthEditor { get; set; }
		protected UnfocusableTextField HeightLabel { get; set; }
		protected NumericSpinEditor HeightEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => HeightEditor;

		internal new PropertyViewModel<T> ViewModel {
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public BaseRectangleEditorControl ()
		{
			XLabel = new UnfocusableTextField ();
			XEditor = new NumericSpinEditor ();
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.Value = 0.0f;
			XEditor.ValueChanged += OnInputUpdated;

			YLabel =  new UnfocusableTextField ();
			YEditor = new NumericSpinEditor ();
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.Value = 0.0f;
			YEditor.ValueChanged += OnInputUpdated;

			WidthLabel = new UnfocusableTextField ();
			WidthEditor = new NumericSpinEditor ();
			WidthEditor.BackgroundColor = NSColor.Clear;
			WidthEditor.Value = 0.0f;
			WidthEditor.ValueChanged += OnInputUpdated;

			HeightLabel =  new UnfocusableTextField ();
			HeightEditor = new NumericSpinEditor ();
			HeightEditor.BackgroundColor = NSColor.Clear;
			HeightEditor.Value = 0.0f;
			HeightEditor.ValueChanged += OnInputUpdated;

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
			ViewModel.Value = (T)Activator.CreateInstance (typeof(T), XEditor.Value, YEditor.Value, WidthEditor.Value, HeightEditor.Value);
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

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = ViewModel.Property.Name + " X Editor"; // TODO Localization

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = ViewModel.Property.Name + " Y Editor"; // TODO Localization

			WidthEditor.AccessibilityEnabled = WidthEditor.Enabled;
			WidthEditor.AccessibilityTitle = ViewModel.Property.Name + " Width Editor"; // TODO Localization

			HeightEditor.AccessibilityEnabled = XEditor.Enabled;
			HeightEditor.AccessibilityTitle = ViewModel.Property.Name + " Height Editor"; // TODO Localization
		}
	}
}
