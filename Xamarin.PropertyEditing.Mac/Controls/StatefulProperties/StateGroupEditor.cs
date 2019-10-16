using System;
using AppKit;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StateGroupEditor : NSControl
	{
		private readonly IHostResourceProvider hostResources;
		private readonly FocusableComboBox stateGroupCombobox;
		private readonly FocusableButton stateGroupEditButton;
		private readonly StatePropertyGroupViewModel statePropertyGroupVM;

		private NSControlSize controlSize;
		public override NSControlSize ControlSize {
			get { return this.controlSize; }
			set {
				if (this.controlSize == value)
					return;

				this.controlSize = value;

				this.stateGroupCombobox.ControlSize = this.controlSize;
				this.stateGroupEditButton.ControlSize = this.controlSize;
			}
		}

		private NSFont font;
		public override NSFont Font {
			get { return this.font; }
			set
			{
				if (this.font == value)
					return;

				this.font = value;

				this.stateGroupCombobox.Font = this.font;
				this.stateGroupEditButton.Font = this.font;
			}
		}

		private bool enabled;
		public override bool Enabled {
			get { return this.enabled; }
			set {
				if (this.enabled == value)
					return;

				this.enabled = value;

				this.stateGroupCombobox.Enabled = this.enabled;
				this.stateGroupEditButton.Enabled = this.enabled;
			}
		}

		internal FocusableButton StateGroupEditButton => this.stateGroupEditButton;
		internal FocusableComboBox StateGroupCombobox => this.stateGroupCombobox;

		public StateGroupEditor (IHostResourceProvider hostResources, StatePropertyGroupViewModel statePropertyGroupVM)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			if (statePropertyGroupVM == null)
				throw new ArgumentNullException (nameof (statePropertyGroupVM));

			this.statePropertyGroupVM = statePropertyGroupVM;

			this.stateGroupCombobox = new FocusableComboBox {
				AllowsExpansionToolTips = true,
				BackgroundColor = NSColor.Clear,
				Cell = {
					LineBreakMode = NSLineBreakMode.TruncatingTail,
					UsesSingleLineMode = true,
				},
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
			};

			AddSubview (this.stateGroupCombobox);

			this.stateGroupEditButton = new FocusableButton {
				BezelStyle = NSBezelStyle.Rounded,
				Title = Properties.Resources.StateGroupEditButton,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.stateGroupEditButton.Activated += EditButtonActivated;

			AddSubview (this.stateGroupEditButton);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.stateGroupCombobox, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.stateGroupCombobox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0f),
				NSLayoutConstraint.Create (this.stateGroupCombobox, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.stateGroupEditButton, NSLayoutAttribute.Left, 1, -4),
				NSLayoutConstraint.Create (this.stateGroupCombobox, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -2),
				NSLayoutConstraint.Create (this.stateGroupEditButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.stateGroupEditButton, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
				NSLayoutConstraint.Create (this.stateGroupEditButton, NSLayoutAttribute.Width, NSLayoutRelation.GreaterThanOrEqual, 1, 70f),
			});

			AppearanceChanged ();
		}

		private void EditButtonActivated (object sender, EventArgs e)
		{
			var statefulPropertyPopOverView = new StatefulPropertyPopOverView (this.hostResources, this.statePropertyGroupVM) {
				Appearance = EffectiveAppearance
			};

			var statefulPropertyPopOver = new AutoClosePopOver (this.hostResources, EffectiveAppearance) {
				CloseOnEnter = false,
				ContentViewController = new NSViewController (null, null) { View = statefulPropertyPopOverView },
			};

			statefulPropertyPopOver.Show (statefulPropertyPopOverView.Frame, this.stateGroupEditButton, NSRectEdge.MinYEdge);
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			AppearanceChanged ();
			base.ViewDidChangeEffectiveAppearance ();
		}

		internal void AppearanceChanged ()
		{
			
		}

		internal void UpdateAccessibilityValues ()
		{
			this.stateGroupCombobox.AccessibilityEnabled = this.stateGroupCombobox.Enabled;
			this.stateGroupCombobox.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityInputModeEditor, this.statePropertyGroupVM.HostedProperty.Property.Name);

			this.stateGroupEditButton.AccessibilityEnabled = this.stateGroupEditButton.Enabled;
			this.stateGroupEditButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityInputModeEditor, this.statePropertyGroupVM.HostedProperty.Property.Name);
		}
	}
}
