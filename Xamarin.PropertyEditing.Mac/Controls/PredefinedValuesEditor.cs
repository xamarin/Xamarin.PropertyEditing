using System;
using System.ComponentModel;

using Foundation;
using AppKit;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PredefinedValuesEditor<T>
		: PropertyEditorControl<PredefinedValuesViewModel<T>>
	{
		public PredefinedValuesEditor (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;

			this.comboBox = new FocusableComboBox {
				AllowsExpansionToolTips = true,
				BackgroundColor = NSColor.Clear,
				Cell = {
					LineBreakMode = NSLineBreakMode.TruncatingTail,
					UsesSingleLineMode = true,
				},
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
			};

			this.comboBox.SelectionChanged += (sender, e) => {
				ViewModel.ValueName = this.comboBox.SelectedValue.ToString ();
			};

			this.popUpButton = new FocusablePopUpButton {
				AllowsExpansionToolTips = true,
				Cell = {
					LineBreakMode = NSLineBreakMode.TruncatingTail,
					UsesSingleLineMode = true,
				},
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
			};

			popupButtonList = new NSMenu ();
			this.popUpButton.Menu = popupButtonList;

			this.popUpButton.Activated += (o, e) => {
				ViewModel.ValueName = (o as NSPopUpButton).Title;
			};
		}

		public override NSView FirstKeyView => this.firstKeyView;
		public override NSView LastKeyView => this.lastKeyView;

		private readonly NSComboBox comboBox;
		private readonly NSPopUpButton popUpButton;
		private NSMenu popupButtonList;

		private bool dataPopulated;
		private NSView firstKeyView;
		private NSView lastKeyView;

		protected override void SetEnabled ()
		{
			if (ViewModel.IsConstrainedToPredefined) {
				this.popUpButton.Enabled = ViewModel.Property.CanWrite;
			} else {
				this.comboBox.Enabled = ViewModel.Property.CanWrite;
			}
		}

		protected override void OnViewModelChanged (EditorViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			if (!this.dataPopulated) {
				if (ViewModel.IsConstrainedToPredefined) {
					this.popupButtonList.RemoveAllItems ();
					foreach (var item in ViewModel.PossibleValues) {
						this.popupButtonList.AddItem (new NSMenuItem (item));
					}

					AddSubview (this.popUpButton);

					this.AddConstraints (new[] {
						NSLayoutConstraint.Create (this.popUpButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
						NSLayoutConstraint.Create (this.popUpButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
						NSLayoutConstraint.Create (this.popUpButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0),
					});

					this.firstKeyView = this.popUpButton;
					this.lastKeyView = this.popUpButton;
				} else {
					this.comboBox.RemoveAll ();

					// Once the VM is loaded we need a one time population
					foreach (var item in ViewModel.PossibleValues) {
						this.comboBox.Add (new NSString (item));
					}

					AddSubview (this.comboBox);

					this.AddConstraints (new[] {
						NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
						NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
						NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0),
					});

					this.firstKeyView = this.comboBox;
					this.lastKeyView = this.comboBox;
				}

				this.dataPopulated = true;
			}

			SetEnabled ();
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.IsConstrainedToPredefined) {
				this.popUpButton.Title = ViewModel.ValueName ?? String.Empty;
			} else {
				this.comboBox.StringValue = ViewModel.ValueName ?? String.Empty;
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			if (ViewModel.IsConstrainedToPredefined) {
				this.popUpButton.AccessibilityEnabled = this.popUpButton.Enabled;
				this.popUpButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityCombobox, ViewModel.Property.Name);
			} else {
				this.comboBox.AccessibilityEnabled = this.comboBox.Enabled;
				this.comboBox.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityCombobox, ViewModel.Property.Name);
			}
		}
	}
}
