using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AppKit;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StringEditorControl
		: EntryPropertyEditor<string>
	{
		public StringEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.lastKeyView = Entry;
		}

		public override NSView LastKeyView => this.lastKeyView;

		protected override void UpdateValue ()
		{
			base.UpdateValue ();

			if (this.inputModePopup != null)
				this.inputModePopup.SelectItem ((ViewModel.InputMode == null) ? string.Empty : ViewModel.InputMode.Identifier);

			SetEnabled ();
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			RightEdgeConstraint.Active = !ViewModel.HasInputModes;
			if (ViewModel.HasInputModes) {
				if (this.inputModePopup == null) {
					this.inputModePopup = new FocusablePopUpButton {
						Menu = new NSMenu (),
						ControlSize = NSControlSize.Small,
						Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
						TranslatesAutoresizingMaskIntoConstraints = false
					};
					this.inputModePopup.ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView);
					this.inputModePopup.Activated += (o, e) => {
						var popupButton = o as NSPopUpButton;
						ViewModel.InputMode = this.viewModelInputModes.FirstOrDefault (im => im.Identifier == popupButton.Title);
					};

					AddSubview (this.inputModePopup);

					this.editorInputModeConstraint = NSLayoutConstraint.Create (Entry, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.inputModePopup, NSLayoutAttribute.Left, 1, -4);

					AddConstraints (new[] {
						this.editorInputModeConstraint,
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, DefaultButtonWidth),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, Entry, NSLayoutAttribute.Height, 1, 0),
					});

					this.lastKeyView = this.inputModePopup;
				}

				this.inputModePopup.Menu.RemoveAllItems ();
				this.viewModelInputModes = ViewModel.InputModes;
				foreach (InputMode item in this.viewModelInputModes) {
					this.inputModePopup.Menu.AddItem (new NSMenuItem (item.Identifier));
				}
			}

			// If we are reusing the control we'll have to hid the inputMode if this doesn't have InputMode.
			if (this.inputModePopup != null) {
				this.inputModePopup.Hidden = !ViewModel.HasInputModes;
				this.editorInputModeConstraint.Active = ViewModel.HasInputModes;
				UpdateAccessibilityValues ();
			}

			SetEnabled ();
		}

		protected override void SetEnabled ()
		{
			Entry.Enabled = ViewModel.Property.CanWrite && (((ViewModel.InputMode != null) && !ViewModel.InputMode.IsSingleValue) || (this.inputModePopup == null));

			if (this.inputModePopup != null)
				this.inputModePopup.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			base.UpdateAccessibilityValues ();
			Entry.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityString, ViewModel.Property.Name);

			if (this.inputModePopup != null) {
				this.inputModePopup.AccessibilityEnabled = this.inputModePopup.Enabled;
				this.inputModePopup.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityInputModeEditor, ViewModel.Property.Name);
			}
		}

		private NSView lastKeyView;
		private NSLayoutConstraint editorInputModeConstraint;
		private FocusablePopUpButton inputModePopup;
		private IReadOnlyList<InputMode> viewModelInputModes;
	}
}

