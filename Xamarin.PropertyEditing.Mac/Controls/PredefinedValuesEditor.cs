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
		}

		public override NSView FirstKeyView => this.firstKeyView;
		public override NSView LastKeyView => this.lastKeyView;

		protected override void SetEnabled ()
		{
			if (ViewModel.IsConstrainedToPredefined) {
				this.popupButton.Enabled = ViewModel.Property.CanWrite;
			} else {
				this.comboBox.Enabled = ViewModel.Property.CanWrite;
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (ViewModel == null)
				return;

			if (ViewModel.IsConstrainedToPredefined) {
				RequirePopup ();

				this.popupButtonList.RemoveAllItems ();
				foreach (var item in ViewModel.PossibleValues) {
					if (!string.IsNullOrEmpty (ViewModel.SeparatorString) && ViewModel.SeparatorString == item) {
						this.popupButtonList.AddItem (NSMenuItem.SeparatorItem);
					} else {
						this.popupButtonList.AddItem (new NSMenuItem (item));
					}
				}
			} else {
				RequireComboBox ();

				this.comboBox.RemoveAll ();
				foreach (var item in ViewModel.PossibleValues) {
					if (!string.IsNullOrEmpty (ViewModel.SeparatorString) && ViewModel.SeparatorString == item) {
						this.comboBox.Add (NSMenuItem.SeparatorItem);
					} else {
						this.comboBox.Add (new NSString (item));
					}
				}
			}

			base.OnViewModelChanged (oldModel);
		}

		protected override void UpdateValue ()
		{
			if (ViewModel.IsConstrainedToPredefined) {
				this.popupButton.Title = ViewModel.ValueName ?? String.Empty;
			} else {
				this.comboBox.StringValue = ViewModel.ValueName ?? String.Empty;
			}
		}

		protected override void UpdateAccessibilityValues ()
		{
			if (ViewModel.IsConstrainedToPredefined) {
				this.popupButton.AccessibilityEnabled = this.popupButton.Enabled;
				this.popupButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityCombobox, ViewModel.Property.Name);
			} else {
				this.comboBox.AccessibilityEnabled = this.comboBox.Enabled;
				this.comboBox.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityCombobox, ViewModel.Property.Name);
			}
		}

		private NSComboBox comboBox;
		private NSPopUpButton popupButton;
		private NSMenu popupButtonList;

		private NSView firstKeyView;
		private NSView lastKeyView;

		private void RemovePopup()
		{
			if (this.popupButton == null)
				return;

			this.popupButton.RemoveFromSuperview ();
			this.popupButton.Dispose ();
			this.popupButton = null;

			this.popupButtonList.Dispose ();
			this.popupButtonList = null;
		}

		private void RequirePopup()
		{
			if (this.popupButton != null)
				return;

			RemoveComboBox ();

			this.popupButton = new FocusablePopUpButton {
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

			this.popupButtonList = new NSMenu ();
			this.popupButton.Menu = this.popupButtonList;
			this.popupButton.Activated += (o, e) => ViewModel.ValueName = (o as NSPopUpButton).Title;

			AddSubview (this.popupButton);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.popupButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.popupButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.popupButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0),
			});

			this.firstKeyView = this.popupButton;
			this.lastKeyView = this.popupButton;
		}

		private void RemoveComboBox()
		{
			if (this.comboBox == null)
				return;

			this.comboBox.RemoveFromSuperview ();
			this.comboBox.Dispose ();
			this.comboBox = null;
		}

		private void RequireComboBox()
		{
			if (this.comboBox != null)
				return;

			RemovePopup ();

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

			AddSubview (this.comboBox);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0),
			});

			this.firstKeyView = this.comboBox;
			this.lastKeyView = this.comboBox;
		}
	}
}
