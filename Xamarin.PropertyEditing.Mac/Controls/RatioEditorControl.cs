using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RatioEditorControl<T> : PropertyEditorControl<RatioViewModel>
	{
		RatioEditor<T> ratioEditor;

		public RatioEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			ratioEditor = new RatioEditor<T> {
				AllowNegativeValues = false,
				AllowRatios = true,
				BackgroundColor = NSColor.Clear,
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			ratioEditor.ValueChanged += (sender, e) => {
				if (e is RatioEditor<T>.RatioEventArgs ratioEventArgs) {
					ViewModel.ValueChanged (ratioEditor.StringValue, ratioEventArgs.CaretPosition, ratioEventArgs.SelectionLength, ratioEventArgs.IncrementValue);
				}
			};
			AddSubview (ratioEditor);

			this.DoConstraints (new[] {
				ratioEditor.ConstraintTo (this, (re, c) => re.Top == c.Top - 2),
				ratioEditor.ConstraintTo (this, (re, c) => re.Left == c.Left + 4),
				ratioEditor.ConstraintTo (this, (re, c) => re.Width == c.Width - 33),
				ratioEditor.ConstraintTo (this, (re, c) => re.Height == DefaultControlHeight),
			});

			UpdateTheme ();
		}

		public override NSView FirstKeyView => ratioEditor.NumericEditor;
		public override NSView LastKeyView => ratioEditor.DecrementButton;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void SetEnabled ()
		{
			ratioEditor.Editable = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			ratioEditor.AccessibilityEnabled = ratioEditor.Enabled;
			ratioEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityNumeric, ViewModel.Property.Name);
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

		protected override void UpdateValue ()
		{
			ratioEditor.StringValue = ViewModel.ValueString;
		}
	}
}
