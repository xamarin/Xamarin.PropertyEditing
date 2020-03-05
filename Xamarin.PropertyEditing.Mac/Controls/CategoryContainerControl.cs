using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CategoryContainerControl
		: PropertyEditorControl
	{
		private NSOutlineView outlineView;
		private NSButton disclosure;

		public CategoryContainerControl (IHostResourceProvider hostResources, NSOutlineView outlineView) : base (hostResources)
		{
			if (outlineView == null)
				throw new ArgumentNullException (nameof (outlineView));

			this.outlineView = outlineView;

			this.disclosure = this.outlineView.MakeView ("NSOutlineViewDisclosureButtonKey", outlineView) as NSButton;
			this.disclosure.TranslatesAutoresizingMaskIntoConstraints = false;
			AddSubview (this.disclosure);

			var label = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			AddSubview (label);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.disclosure, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.disclosure, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 4),
				NSLayoutConstraint.Create (label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.disclosure, NSLayoutAttribute.Right, 1, 0),
				NSLayoutConstraint.Create (label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, 0),
			});
		}

		public override bool IsDynamicallySized => false;

		public override bool NeedsPropertyButton => false;

		public override NSView FirstKeyView => this.disclosure;

		public override NSView LastKeyView => this.disclosure;
	}
}
