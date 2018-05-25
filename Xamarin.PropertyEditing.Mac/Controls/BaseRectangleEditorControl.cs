using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseRectangleEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		protected UnfocusableTextField XLabel { get; set; }
		protected NumericSpinEditor<T> XEditor { get; set; }
		protected UnfocusableTextField YLabel { get; set; }
		protected NumericSpinEditor<T> YEditor { get; set; }
		protected UnfocusableTextField WidthLabel { get; set; }
		protected NumericSpinEditor<T> WidthEditor { get; set; }
		protected UnfocusableTextField HeightLabel { get; set; }
		protected NumericSpinEditor<T> HeightEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => HeightEditor;

		public BaseRectangleEditorControl ()
		{
			using (Performance.StartNew ("X")) {
				XLabel = new UnfocusableTextField ();
				XEditor = new NumericSpinEditor<T> ();
				XEditor.BackgroundColor = NSColor.Clear;
				XEditor.Value = 0.0f;
				XEditor.ValueChanged += OnInputUpdated;
				AddSubview (XLabel);
				AddSubview (XEditor);
			}

			using (Performance.StartNew ("Y")) {
				YLabel = new UnfocusableTextField ();
				YEditor = new NumericSpinEditor<T> ();
				YEditor.BackgroundColor = NSColor.Clear;
				YEditor.Value = 0.0f;
				YEditor.ValueChanged += OnInputUpdated;

				AddSubview (YLabel);
				AddSubview (YEditor);
			}

			using (Performance.StartNew ("Width")) {
				WidthLabel = new UnfocusableTextField ();
				WidthEditor = new NumericSpinEditor<T>();
				WidthEditor.BackgroundColor = NSColor.Clear;
				WidthEditor.Value = 0.0f;
				WidthEditor.ValueChanged += OnInputUpdated;
				AddSubview (WidthLabel);
				AddSubview (WidthEditor);
			}

			using (Performance.StartNew("Height")) {
				HeightLabel = new UnfocusableTextField ();
				HeightEditor = new NumericSpinEditor<T> ();
				HeightEditor.BackgroundColor = NSColor.Clear;
				HeightEditor.Value = 0.0f;
				HeightEditor.ValueChanged += OnInputUpdated;
				AddSubview (HeightLabel);
				AddSubview (HeightEditor);
			}





			using (Performance.StartNew ("Constraints")) {
				this.DoConstraints (new[] {
					XEditor.ConstraintTo (this, (xe, c) => xe.Width == 90),
					XEditor.ConstraintTo (this, (xe, c) => xe.Height == DefaultControlHeight),
					YEditor.ConstraintTo (this, (ye, c) => ye.Width == 90),
					YEditor.ConstraintTo (this, (ye, c) => ye.Height == DefaultControlHeight),
					WidthEditor.ConstraintTo (this, (we, c) => we.Width == 90),
					WidthEditor.ConstraintTo (this, (we, c) => we.Height == DefaultControlHeight),
					HeightEditor.ConstraintTo (this, (he, c) => he.Width == 90),
					HeightEditor.ConstraintTo (this, (he, c) => he.Height == DefaultControlHeight),
				});
			}
		
			UpdateTheme ();
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
				SetErrors (errors);
			} else {
				SetErrors (null);
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
			XEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityXEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityYEditor, ViewModel.Property.Name);

			WidthEditor.AccessibilityEnabled = WidthEditor.Enabled;
			WidthEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityWidthEditor, ViewModel.Property.Name);

			HeightEditor.AccessibilityEnabled = HeightEditor.Enabled;
			HeightEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityHeightEditor, ViewModel.Property.Name);
		}
	}
}
