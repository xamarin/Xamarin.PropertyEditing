using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RatioEditorControl<T> : PropertyEditorControl<CommonRatio>
	{
		private RatioEditor<T> ratioEditor;

		internal new RatioViewModel ViewModel
		{
			get { return (RatioViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public RatioEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.ratioEditor = new RatioEditor<T> {
				AllowNegativeValues = false,
				AllowRatios = true,
				BackgroundColor = NSColor.Clear,
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			this.ratioEditor.ValueChanged += (sender, e) => {
				if (e is RatioEditor<T>.RatioEventArgs ratioEventArgs) {
					ViewModel.ValueChanged (this.ratioEditor.StringValue, ratioEventArgs.CaretPosition, ratioEventArgs.SelectionLength, ratioEventArgs.IncrementValue);
				}
			};
			AddSubview (this.ratioEditor);

			this.DoConstraints (new[] {
				this.ratioEditor.ConstraintTo (this, (re, c) => re.Top == c.Top - 2),
				this.ratioEditor.ConstraintTo (this, (re, c) => re.Left == c.Left + 4),
				this.ratioEditor.ConstraintTo (this, (re, c) => re.Width == c.Width - 33),
				this.ratioEditor.ConstraintTo (this, (re, c) => re.Height == DefaultControlHeight),
			});

			UpdateTheme ();
		}

		public override NSView FirstKeyView => this.ratioEditor.NumericEditor;
		public override NSView LastKeyView => this.ratioEditor.DecrementButton;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void SetEnabled ()
		{
			this.ratioEditor.Editable = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.ratioEditor.AccessibilityEnabled = this.ratioEditor.Enabled;
			this.ratioEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityNumeric, ViewModel.Property.Name);
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
			this.ratioEditor.StringValue = ViewModel.ValueString;
		}
	}
}
