using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PanelHeaderLabelControl : NSView
	{
		public const string PanelHeaderLabelIdentifierString = "PanelHeaderLabelIdentifier";
		public PanelHeaderLabelControl ()
		{
			Identifier = PanelHeaderLabelIdentifierString;

			var propertyObjectNameLabel = new UnfocusableTextField {
				Alignment = NSTextAlignment.Right,
				StringValue = Properties.Resources.Name + ":",
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			AddSubview (propertyObjectNameLabel);

			this.DoConstraints (new NSLayoutConstraint[] {
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Top == c.Top),
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Left == c.Left + 182),
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Width == 40),
				propertyObjectNameLabel.ConstraintTo(this, (ol, c) => ol.Height == PropertyEditorControl.DefaultControlHeight),
			});
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

			this.DoConstraints (new NSLayoutConstraint[] {
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Top == c.Top + 2),
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Left == c.Left + 4),
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Width == c.Width - 34),
				this.propertyObjectName.ConstraintTo(this, (on, c) => on.Height == DefaultControlHeight - 3),
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
			if ( e.PropertyName == nameof (PanelViewModel.ObjectName)) {
				UpdateValue ();
			}
		}
	}
}
