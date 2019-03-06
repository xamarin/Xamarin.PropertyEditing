using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AppKit;

using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StringEditorControl
		: PropertyEditorControl<PropertyViewModel<string>>
	{
		public override NSView FirstKeyView => this.stringEditor;
		private NSView lastKeyView;
		public override NSView LastKeyView => this.lastKeyView;

		public StringEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			this.stringEditor = new PropertyTextField {
				BackgroundColor = NSColor.Clear,
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			this.stringEditor.Changed += (sender, e) => {
				ViewModel.Value = this.stringEditor.StringValue;
			};
			AddSubview (this.stringEditor);

			this.lastKeyView = this.stringEditor;
			this.editorRightConstraint = NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0),
				NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				this.editorRightConstraint,
				NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, -6),
			});
		}

		protected override void UpdateValue ()
		{
			this.stringEditor.StringValue = ViewModel.Value ?? string.Empty;
			this.stringEditor.Enabled = CanEnable;
			if (this.inputModePopup != null)
				this.inputModePopup.SelectItem ((ViewModel.InputMode == null) ? string.Empty : ViewModel.InputMode.Identifier);
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			this.editorRightConstraint.Active = !ViewModel.HasInputModes;
			if (ViewModel.HasInputModes) {
				if (this.inputModePopup == null) {
					this.inputModePopup = new FocusablePopUpButton {
						Menu = new NSMenu (),
						ControlSize = NSControlSize.Small,
						Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
						TranslatesAutoresizingMaskIntoConstraints = false
					};

					this.inputModePopup.Activated += (o, e) => {
						var popupButton = o as NSPopUpButton;
						ViewModel.InputMode = this.viewModelInputModes.FirstOrDefault (im => im.Identifier == popupButton.Title);
					};

					AddSubview (this.inputModePopup);

					this.editorInputModeConstraint = NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.inputModePopup, NSLayoutAttribute.Left, 1, -4);

					AddConstraints (new[] {
						this.editorInputModeConstraint,
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this,  NSLayoutAttribute.CenterY, 1f, 0f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, 0),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 80f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.stringEditor, NSLayoutAttribute.Height, 1, 0),
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
			}

			SetEnabled ();
		}

		protected override void SetEnabled ()
		{
			this.stringEditor.Enabled = CanEnable;
			if (this.inputModePopup != null)
				this.inputModePopup.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.stringEditor.AccessibilityEnabled = this.stringEditor.Enabled;
			this.stringEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityString, ViewModel.Property.Name);
			if (this.inputModePopup != null) {
				this.inputModePopup.AccessibilityEnabled = this.inputModePopup.Enabled;
				this.inputModePopup.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityInpueModeEditor, ViewModel.Property.Name);
			}
		}

		private readonly NSTextField stringEditor;
		private NSLayoutConstraint editorRightConstraint, editorInputModeConstraint;
		internal NSPopUpButton inputModePopup;
		private IReadOnlyList<InputMode> viewModelInputModes;

		private bool CanEnable => ViewModel.Property.CanWrite && (((ViewModel.InputMode != null) && !ViewModel.InputMode.IsSingleValue) || (this.inputModePopup == null));
	}
}

