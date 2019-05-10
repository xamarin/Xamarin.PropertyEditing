using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RatioEditorControl<T> : PropertyEditorControl<RatioViewModel>
	{
		private readonly RatioEditor<T> ratioEditor;

		public RatioEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.ratioEditor = new RatioEditor<T> (hostResources);
			this.ratioEditor.SetFormatter (null);

			// update the value on keypress
			this.ratioEditor.ValueChanged += (sender, e) => {
				if (e is RatioEditor<T>.RatioEventArgs ratioEventArgs) {
					ViewModel.ValueChanged (this.ratioEditor.StringValue, ratioEventArgs.CaretPosition, ratioEventArgs.SelectionLength, ratioEventArgs.IncrementValue);
				}
			};
			AddSubview (this.ratioEditor);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Bottom, 1f, BottomOffset),
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0),
			});
		}

		public override NSView FirstKeyView => this.ratioEditor.NumericEditor;
		public override NSView LastKeyView => this.ratioEditor.DecrementButton;

		protected override void SetEnabled ()
		{
			this.ratioEditor.Enabled = ViewModel.IsInputEnabled;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.ratioEditor.AccessibilityEnabled = this.ratioEditor.Enabled;
			this.ratioEditor.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityNumeric, ViewModel.Property.Name);
		}

		protected override void UpdateValue ()
		{
			this.ratioEditor.StringValue = ViewModel.ValueString;
		}
	}
}
