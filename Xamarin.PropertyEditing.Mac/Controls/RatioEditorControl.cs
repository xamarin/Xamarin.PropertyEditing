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
		private RatioEditor<T> ratioEditor;

		public RatioEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.ratioEditor = new RatioEditor<T> (hostResources) {
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

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, -2f),
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, -32f),
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight),
			});
		}

		public override NSView FirstKeyView => ratioEditor.NumericEditor;
		public override NSView LastKeyView => ratioEditor.DecrementButton;

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
