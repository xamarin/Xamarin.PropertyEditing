using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
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

			this.ratioEditor = new RatioEditor<T> (hostResources);
			this.ratioEditor.ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView);
			this.ratioEditor.SetFormatter (null);

			// update the value on keypress
			this.ratioEditor.ValueChanged += (sender, e) => {
				if (e is RatioEditor<T>.RatioEventArgs ratioEventArgs) {
					ViewModel.ValueChanged (this.ratioEditor.StringValue, ratioEventArgs.CaretPosition, ratioEventArgs.SelectionLength, ratioEventArgs.IncrementValue);
				}
			};
			AddSubview (this.ratioEditor);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0),
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0),
				NSLayoutConstraint.Create (this.ratioEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -6),
			});
		}

		public override NSView FirstKeyView => ratioEditor.NumericEditor;
		public override NSView LastKeyView => ratioEditor.DecrementButton;

		protected override void SetEnabled ()
		{
			this.ratioEditor.Enabled = ViewModel.Property.CanWrite;
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
