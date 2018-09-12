using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PanelHeaderLabelControl : NSView
	{
		public const string PanelHeaderLabelIdentifierString = "PanelHeaderLabelIdentifier";

		private NSImageView propertyIcon;

		private PanelViewModel viewModel;

		public PanelHeaderLabelControl (PanelViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.viewModel = viewModel;
			this.viewModel.PropertyChanged += ViewModel_PropertyChanged;

			Identifier = PanelHeaderLabelIdentifierString;

			var propertyObjectNameLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				StringValue = Properties.Resources.Name + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (propertyObjectNameLabel);

			var propertyTypeNameLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				StringValue = Properties.Resources.Type + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (propertyTypeNameLabel);

			this.propertyIcon = new NSImageView {
				ImageScaling = NSImageScale.AxesIndependently,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (this.propertyIcon);

			this.DoConstraints (new NSLayoutConstraint[] {
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Top == c.Top),
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Left == c.Left + 182),
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Width == 40),
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Height == PropertyEditorControl.DefaultControlHeight),

				propertyTypeNameLabel.ConstraintTo(this, (tl, c) => tl.Top == c.Top + 22),
				propertyTypeNameLabel.ConstraintTo(this, (tl, c) => tl.Left == c.Left + 182),
				propertyTypeNameLabel.ConstraintTo(this, (tl, c) => tl.Width == 40),
				propertyTypeNameLabel.ConstraintTo(this, (tl, c) => tl.Height == PropertyEditorControl.DefaultControlHeight),

				this.propertyIcon.ConstraintTo(this, (ico, c) => ico.Top == c.Top + 5),
				this.propertyIcon.ConstraintTo(this, (ico, c) => ico.Left == c.Left + 32),
				this.propertyIcon.ConstraintTo(this, (ico, c) => ico.Width == 32),
				this.propertyIcon.ConstraintTo(this, (ico, c) => ico.Height == 32),
			});

			UpdateIcon ();
		}

		private async void UpdateIcon ()
		{
			Stream icon = await this.viewModel.GetIconAsync ();

			if (icon != null)
				this.propertyIcon.Image = NSImage.FromStream (icon);

			this.propertyIcon.Hidden = (icon == null);
		}

		void ViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.TypeName)) {
				UpdateIcon ();
			}
		}
	}

	internal class PanelHeaderEditorControl : PropertyEditorControl
	{
		private NSTextField propertyObjectName;

		private PanelViewModel viewModel;

		public PanelHeaderEditorControl (PanelViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			this.viewModel = viewModel;
			this.viewModel.PropertyChanged += ViewModel_PropertyChanged;

			NSControlSize controlSize = NSControlSize.Small;
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.propertyObjectName = new NSTextField {
				ControlSize = controlSize,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				PlaceholderString = LocalizationResources.ObjectNamePlaceholder,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.propertyObjectName.Activated += PropertyObjectName_Activated;

			AddSubview (this.propertyObjectName);

			var propertyTypeName = new UnfocusableTextField {
				StringValue = viewModel.TypeName,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (propertyTypeName);

			this.DoConstraints (new NSLayoutConstraint[] {
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Top == c.Top + 2),
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Left == c.Left + 4),
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Width == c.Width - 34),
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Height == DefaultControlHeight - 3),

				propertyTypeName.ConstraintTo(this, (tn, c) => tn.Top == c.Top + 22),
				propertyTypeName.ConstraintTo(this, (tn, c) => tn.Left == c.Left + 4),
				propertyTypeName.ConstraintTo(this, (tn, c) => tn.Width == c.Width - 34),
				propertyTypeName.ConstraintTo(this, (tn, c) => tn.Height == DefaultControlHeight),
			});

			// We won't enable or show the PropertyButton for the header
			this.PropertyButton.Enabled = false;
			this.PropertyButton.Hidden = true;

			UpdateValue ();
		}

		void PropertyObjectName_Activated (object sender, EventArgs e)
		{
			this.viewModel.ObjectName = this.propertyObjectName.StringValue;
		}

		public override NSView FirstKeyView => this.propertyObjectName;

		public override NSView LastKeyView => this.propertyObjectName;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (viewModel.GetErrors (viewModel.GetType ().Name));
		}

		protected override void SetEnabled ()
		{
			this.propertyObjectName.Editable = !this.viewModel.IsObjectNameReadOnly;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.propertyObjectName.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityObjectName, nameof (viewModel.ObjectName));
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (this.viewModel.HasErrors) {
				SetErrors (errors);
			} else {
				SetErrors (null);
				SetEnabled ();
			}
		}

		protected override void UpdateValue ()
		{
			if (this.propertyObjectName != null) {
				this.propertyObjectName.StringValue = this.viewModel.ObjectName ?? string.Empty;
				this.propertyObjectName.Editable = !this.viewModel.IsObjectNameReadOnly;
			}
		}

		void ViewModel_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (PanelViewModel.ObjectName)) {
				UpdateValue ();
			}
		}

		public override nint GetHeight (PropertyViewModel vm)
		{
			return 44;
		}
	}
}
