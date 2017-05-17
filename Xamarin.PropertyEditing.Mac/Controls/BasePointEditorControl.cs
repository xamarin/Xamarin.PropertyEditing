using System;
using System.Collections;
using System.Diagnostics;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BasePointEditorControl<T> : PropertyEditorControl
		where T : struct
	{
		internal UnfocusableTextField XLabel { get; set; }
		internal NumericSpinEditor XEditor { get; set; }
		internal UnfocusableTextField YLabel { get; set; }
		internal NumericSpinEditor YEditor { get; set; }

		public override NSView FirstKeyView => XEditor;
		public override NSView LastKeyView => YEditor;

		internal new PropertyViewModel<T> ViewModel
		{
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public BasePointEditorControl ()
		{
			XLabel = new UnfocusableTextField ();

			XEditor = new NumericSpinEditor ();
			XEditor.BackgroundColor = NSColor.Clear;
			XEditor.Value = 0.0f;
			XEditor.ValueChanged += OnInputUpdated;

			YLabel = new UnfocusableTextField ();

			YEditor = new NumericSpinEditor ();
			YEditor.BackgroundColor = NSColor.Clear;
			YEditor.Value = 0.0f;
			YEditor.ValueChanged += OnInputUpdated;

			AddSubview (XLabel);
			AddSubview (XEditor);
			AddSubview (YLabel);
			AddSubview (YEditor);

			this.DoConstraints (new[] {
				XEditor.ConstraintTo (this, (xe, c) => xe.Width == 50),
				YEditor.ConstraintTo (this, (ye, c) => ye.Width == 50),
			});
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
			} else {
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

		protected abstract void OnInputUpdated (object sender, EventArgs e);
	}
}
