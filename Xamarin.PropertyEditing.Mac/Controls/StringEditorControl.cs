using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class StringEditorControl
		: PropertyEditorControl<PropertyViewModel<string>>
	{
		private NSTextField stringEditor { get; set; }

		private NSLayoutConstraint stringEditorWidthConstraint;

		public override NSView FirstKeyView => this.stringEditor;
		public override NSView LastKeyView => this.stringEditor;

		internal NSPopUpButton inputModePopup;
		private IReadOnlyList<InputMode> viewModelInputModes;

		public StringEditorControl (IHostResourceProvider hostResource)
			: base (hostResource)
		{
			this.stringEditor = new NSTextField {
				BackgroundColor = NSColor.Clear,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				StringValue = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// update the value on keypress
			this.stringEditor.Changed += (sender, e) => {
				ViewModel.Value = this.stringEditor.StringValue;
			};
			AddSubview (this.stringEditor);

			this.stringEditorWidthConstraint = NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -117f);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
				this.stringEditorWidthConstraint,
				NSLayoutConstraint.Create (this.stringEditor, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 3),
			});
		}

		protected override void UpdateValue ()
		{
			this.stringEditor.StringValue = ViewModel.Value ?? string.Empty;
			this.stringEditor.Enabled = ((ViewModel.InputMode != null) && !ViewModel.InputMode.IsSingleValue) || (this.inputModePopup == null);
			if (this.inputModePopup != null)
				this.inputModePopup.SelectItem ((ViewModel.InputMode == null) ? string.Empty : ViewModel.InputMode.Identifier);
		}

		protected override void HandleErrorsChanged (object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
		{
			UpdateErrorsDisplayed (ViewModel.GetErrors (ViewModel.Property.Name));
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (ViewModel == null)
				return;

			if (ViewModel.HasInputModes) {
				if (this.inputModePopup == null) {
					this.inputModePopup = new NSPopUpButton {
						Menu = new NSMenu (),
						TranslatesAutoresizingMaskIntoConstraints = false,
					};

					this.inputModePopup.Activated += (o, e) => {
						var popupButton = o as NSPopUpButton;
						ViewModel.InputMode = this.viewModelInputModes.FirstOrDefault (im => im.Identifier == popupButton.Title);
					};

					AddSubview (this.inputModePopup);

					this.AddConstraints (new[] {
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Top, 1f, 1f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Right, 1f, -33f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 80f),
						NSLayoutConstraint.Create (this.inputModePopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, DefaultControlHeight - 3 ),
					});
				}

				this.inputModePopup.Menu.RemoveAllItems ();
				this.viewModelInputModes = ViewModel.InputModes;
				foreach (InputMode item in this.viewModelInputModes) {
					this.inputModePopup.Menu.AddItem (new NSMenuItem (item.Identifier));
				}

				this.stringEditorWidthConstraint.Constant = -117f; // Shorten the stringEditor if we have Inputmodes Showing.
			} else {
				this.stringEditorWidthConstraint.Constant = -34f; // Lengthen the stringEditor if we have Inputmodes Hidden.
			}

			// If we are reusing the control we'll have to hid the inputMode if this doesn't have InputMode.
			if (this.inputModePopup != null)
				this.inputModePopup.Hidden = !ViewModel.HasInputModes;
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
			if (ViewModel.HasErrors) {
				SetErrors (errors);
			} else {
				SetErrors (null);
				SetEnabled ();
			}
		}

		protected override void SetEnabled ()
		{
			this.stringEditor.Editable = ViewModel.Property.CanWrite;
			if (this.inputModePopup != null)
				this.inputModePopup.Enabled = ViewModel.Property.CanWrite;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.stringEditor.AccessibilityEnabled = this.stringEditor.Editable;
			this.stringEditor.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityString, ViewModel.Property.Name);
			if (this.inputModePopup != null) {
				this.inputModePopup.AccessibilityTitle = string.Format (LocalizationResources.AccessibilityInpueModeEditor, ViewModel.Property.Name);
			}
		}
	}
}
