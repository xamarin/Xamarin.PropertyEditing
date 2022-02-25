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
					this.popupButtonList.AddItem (new NSMenuItem (item));
				}
			} else {
				RequireComboBox ();

				UnhookSelectionChangeAndClearItems ();
				foreach (var item in ViewModel.PossibleValues) {
					this.comboBox.Add (new NSString (item));
				}
				this.comboBox.SelectionChanged += ComboBox_SelectionChanged;
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
				this.popupButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityPopUp, ViewModel.Property.Name);
			} else {
				this.comboBox.AccessibilityEnabled = this.comboBox.Enabled;
				this.comboBox.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityCombobox, ViewModel.Property.Name);
			}
		}

		private FocusableComboBox comboBox;
		private FocusablePopUpButton popupButton;
		private NSMenu popupButtonList;

		private NSView firstKeyView;
		private NSView lastKeyView;

		private void RemovePopup()
		{
			if (this.popupButton == null)
				return;

			this.popupButton.Activated -= PopupButton_Activated;
			this.popupButton.RemoveFromSuperview ();
			this.popupButton.Dispose ();
			this.popupButton = null;

			this.popupButtonList.RemoveAllItems ();
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
				ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView)
			};
			this.popupButton.ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView);
			this.popupButtonList = new NSMenu ();
			this.popupButton.Menu = this.popupButtonList;
			this.popupButton.Activated += PopupButton_Activated; 

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
			UnhookSelectionChangeAndClearItems ();
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
				ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView),
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
			};
			this.comboBox.ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView);
			this.comboBox.SelectionChanged += ComboBox_SelectionChanged;

			AddSubview (this.comboBox);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0f),
				NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.comboBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0),
			});

			this.firstKeyView = this.comboBox;
			this.lastKeyView = this.comboBox;
		}

		private void PopupButton_Activated (object sender, EventArgs e)
		{
			if (ViewModel != null && sender is NSPopUpButton popUpButton) {
				ViewModel.ValueName = popUpButton.Title;
			}
		}

		private void ComboBox_SelectionChanged (object sender, EventArgs e)
		{
			if (ViewModel != null
				&& this.comboBox != null
				&& this.comboBox.SelectedValue != null) {
				ViewModel.ValueName = this.comboBox.SelectedValue.ToString ();
			}
		}

		private void UnhookSelectionChangeAndClearItems ()
		{
			if (this.comboBox != null) {
				this.comboBox.SelectionChanged -= ComboBox_SelectionChanged;
				this.comboBox.RemoveAll ();
			}
		}
	}
}
