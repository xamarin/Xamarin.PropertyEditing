using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BasePointEditorControl<T> : PropertyEditorControl<PropertyViewModel<T>>
	{
		internal UnfocusableTextField XLabel { get; set; }
		internal NumericSpinEditor<T> XEditor { get; set; }
		internal UnfocusableTextField YLabel { get; set; }
		internal NumericSpinEditor<T> YEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => YEditor.DecrementButton;

		public BasePointEditorControl ()
		{
			XLabel = new UnfocusableTextField ();

			XEditor = new NumericSpinEditor<T> ();
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.Value = 0.0f;
			XEditor.ValueChanged += OnInputUpdated;

			XLabel.AccessibilityElement = false;

			YLabel = new UnfocusableTextField ();

			YEditor = new NumericSpinEditor<T> ();
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.Value = 0.0f;
			YEditor.ValueChanged += OnInputUpdated;

			YLabel.AccessibilityElement = false;

			AddSubview (XLabel);
			AddSubview (XEditor);
			AddSubview (YLabel);
			AddSubview (YEditor);

			this.DoConstraints (new[] {
				XEditor.ConstraintTo (this, (xe, c) => xe.Left == xe.Left - 1),
				XEditor.ConstraintTo (this, (xe, c) => xe.Width == 90),
				XEditor.ConstraintTo (this, (xe, c) => xe.Height == DefaultControlHeight),
				YEditor.ConstraintTo (this, (ye, c) => ye.Width == 90),
				YEditor.ConstraintTo (this, (ye, c) => ye.Height == DefaultControlHeight),
			});

			UpdateTheme ();
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
		}

		protected override void UpdateAccessibilityValues ()
		{
			XEditor.AccessibilityEnabled = XEditor.Enabled;
			XEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityXEditor, ViewModel.Property.Name);

			YEditor.AccessibilityEnabled = YEditor.Enabled;
			YEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityYEditor, ViewModel.Property.Name);
		}

		protected virtual void OnInputUpdated (object sender, EventArgs e)
		{
			ViewModel.Value = (T)Activator.CreateInstance (typeof (T), XEditor.Value, YEditor.Value);
		}
	}
}
