using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CollectionInlineEditorControl
		: PropertyEditorControl<CollectionPropertyViewModel>
	{
		public CollectionInlineEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.label = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = Properties.Resources.CollectionValue,
			};

			AddSubview (this.label);

			this.openCollection = new FocusableButton {
				Title = Properties.Resources.CollectionEditButton,
				BezelStyle = NSBezelStyle.Rounded,
				AccessibilityEnabled = true,
				AccessibilityHelp = Properties.Resources.AccessibilityCollectionHelp
			};

			this.openCollection.Activated += (o, e) => {
				CollectionEditorWindow.EditCollection (EffectiveAppearance, HostResources, ViewModel);
			};

			AddSubview (this.openCollection);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 0),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, 0),
				NSLayoutConstraint.Create (this.openCollection, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this.label, NSLayoutAttribute.Trailing, 1, 12),
				NSLayoutConstraint.Create (this.openCollection, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.openCollection, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1, 70),
				NSLayoutConstraint.Create (this.openCollection, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -6)
			});

			AppearanceChanged ();
		}

		public override NSView FirstKeyView => this.openCollection;
		public override NSView LastKeyView => this.openCollection;

		protected override void SetEnabled ()
		{
			base.SetEnabled ();
			this.openCollection.Enabled = ViewModel?.Property.CanWrite ?? false;
		}

		protected override void UpdateAccessibilityValues ()
		{
			base.UpdateAccessibilityValues ();

			this.openCollection.AccessibilityTitle = (ViewModel != null) ? String.Format (Properties.Resources.AccessibilityCollection, ViewModel.Property.Name) : null;
		}

		private readonly UnfocusableTextField label;
		private readonly NSButton openCollection;

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			this.label.TextColor = HostResources.GetNamedColor (NamedResources.DescriptionLabelColor);
		}
	}
}
