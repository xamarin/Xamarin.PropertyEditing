using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BasePanelWindow : NSPanel
	{
		protected NSButton ButtonDone;
		private NSButton buttonCancel;
		private NSButton buttonHelp;

		protected NSPopUpButton BindingTypePopup;

		protected NSPopUpButton ValueConverterPopup;

		protected NSButton AddConverterButton;

		protected NSView AncestorTypeBox;
		protected NSView PathBox;

		protected NSView MainContainer;
		protected NSView BindingPropertiesView;
		protected NSView FlagsPropertiesView;
		protected NSButton ButtonMoreSettings;

		protected nfloat MoreSettingsViewHeight { get; set; }

		internal BasePanelWindow ()
			: base (new CGRect (0, 0, 728, 445), NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable, NSBackingStore.Buffered, true)
		{
			FloatingPanel = true;

			MaxSize = new CGSize (960, 720); // TODO discuss what the Max/Min Size should be and if we should have one.
			MinSize = new CGSize (320, 240);

			this.MainContainer = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			this.ButtonDone = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				KeyEquivalent = "\r", // Fire when enter pressed
				Highlighted = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.MainContainer.AddSubview (this.ButtonDone);

			this.buttonCancel = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				KeyEquivalent = "0x1b", // TODO Need to double check this is right
				Title = Properties.Resources.Cancel,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.buttonCancel.Activated += (sender, e) => {
				Close ();
			};

			this.MainContainer.AddSubview (this.buttonCancel);

			this.buttonHelp = new NSButton {
				BezelStyle = NSBezelStyle.HelpButton,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.MainContainer.AddSubview (this.buttonHelp);

			var bindingTypeLabel = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Alignment = NSTextAlignment.Right,
			};

			bindingTypeLabel.StringValue = Properties.Resources.BindingType;
			this.MainContainer.AddSubview (bindingTypeLabel);

			this.BindingTypePopup = new FocusablePopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
			};

			var bindingTypeMenuList = new NSMenu ();
			this.BindingTypePopup.Menu = bindingTypeMenuList;
			this.MainContainer.AddSubview (this.BindingTypePopup);

			var valueConverterLabel = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				Alignment = NSTextAlignment.Right,
			};

			this.AncestorTypeBox = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false, 
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.MainContainer.AddSubview (this.AncestorTypeBox);

			this.PathBox = new NSView {
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.MainContainer.AddSubview (this.PathBox);

			valueConverterLabel.StringValue = Properties.Resources.Converter;
			this.MainContainer.AddSubview (valueConverterLabel);

			this.ValueConverterPopup = new FocusablePopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				StringValue = String.Empty,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
			}; 

			var valueConverterMenuList = new NSMenu ();
			this.ValueConverterPopup.Menu = valueConverterMenuList;
			this.MainContainer.AddSubview (this.ValueConverterPopup);

			this.AddConverterButton = new NSButton {
				BezelStyle = NSBezelStyle.Rounded,
				Image = NSImage.ImageNamed (NSImageName.AddTemplate),
				Title = string.Empty,
				ToolTip = Properties.Resources.AddValueConverterEllipsis,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.MainContainer.AddSubview (this.AddConverterButton);

			this.ButtonMoreSettings = new NSButton {
				BezelStyle = NSBezelStyle.Disclosure,
				Title = string.Empty,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			this.ButtonMoreSettings.SetButtonType (NSButtonType.PushOnPushOff);

			this.MainContainer.AddSubview (this.ButtonMoreSettings);

			var labelOtherSettings = new UnfocusableTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.MainContainer.AddSubview (labelOtherSettings);

			this.BindingPropertiesView = new NSView {
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.MainContainer.AddSubview (this.BindingPropertiesView);

			this.FlagsPropertiesView = new NSView {
				Hidden = true,
				TranslatesAutoresizingMaskIntoConstraints = false,
				WantsLayer = true,

				// Layer out of alphabetical order so that WantsLayer creates the layer first
				Layer = {
					CornerRadius = 1.0f,
					BorderColor = new CGColor (.5f, .5f, .5f, .5f),
					BorderWidth = 1,
				},
			};

			this.MainContainer.AddSubview (this.FlagsPropertiesView);

			//Work out the titlebar height
			var titleBarHeight = Frame.Size.Height - ContentRectFor (Frame).Size.Height;

			var ancestorTypeBoxHeightConstraint = NSLayoutConstraint.Create (this.AncestorTypeBox, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Bottom, 1f, -100f);
		 	var heightConstant = ancestorTypeBoxHeightConstraint.Constant;

			var bindingPropertiesViewHeightConstraint = NSLayoutConstraint.Create (this.BindingPropertiesView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 124);

			this.ButtonMoreSettings.Activated += (sender, e) => {
				if (sender is NSButton moreButton) {
					ToggleSettingsLabel (moreButton.State == NSCellStateValue.Off, labelOtherSettings);

					this.BindingPropertiesView.Hidden = moreButton.State == NSCellStateValue.Off;
					this.FlagsPropertiesView.Hidden = this.BindingPropertiesView.Hidden;

					bindingPropertiesViewHeightConstraint.Constant = this.MoreSettingsViewHeight;
					ancestorTypeBoxHeightConstraint.Constant = this.BindingPropertiesView.Hidden ? heightConstant : heightConstant - (MoreSettingsViewHeight + 20);
					this.MainContainer.SetFrameSize (new CGSize (this.MainContainer.Frame.Width, this.BindingPropertiesView.Hidden ? this.MainContainer.Frame.Height - MoreSettingsViewHeight : this.MainContainer.Frame.Height + MoreSettingsViewHeight));
					SetFrame (new CGRect (new CGPoint (Frame.X, Frame.Y), new CGSize (Frame.Width, this.MainContainer.Frame.Height + titleBarHeight)), false, true);
				}
			};

			ToggleSettingsLabel (this.ButtonMoreSettings.State == NSCellStateValue.Off, labelOtherSettings);

			this.MainContainer.AddConstraints (new[] {
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Top, 1f, 10f),
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Left, 1f, 20f),
				NSLayoutConstraint.Create (bindingTypeLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.BindingTypePopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.BindingTypePopup, NSLayoutAttribute.Left, NSLayoutRelation.Equal, bindingTypeLabel, NSLayoutAttribute.Right, 1f, 5f),
				NSLayoutConstraint.Create (this.BindingTypePopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.AncestorTypeBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.BindingTypePopup, NSLayoutAttribute.Bottom, 1f, 10f),
				NSLayoutConstraint.Create (this.AncestorTypeBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Left, 1f, 20f),
				NSLayoutConstraint.Create (this.AncestorTypeBox, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.PathBox,  NSLayoutAttribute.Left, 1f, -10f),
				ancestorTypeBoxHeightConstraint,

				NSLayoutConstraint.Create (this.PathBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.PathBox, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.MainContainer,  NSLayoutAttribute.Right, 1f, -20f),
				NSLayoutConstraint.Create (this.PathBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.AncestorTypeBox,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.PathBox, NSLayoutAttribute.Height, NSLayoutRelation.Equal,this.AncestorTypeBox, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.AncestorTypeBox, NSLayoutAttribute.Bottom, 1f, 10f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Left, 1f, 20f),
				NSLayoutConstraint.Create (valueConverterLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.ValueConverterPopup, NSLayoutAttribute.Top, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.ValueConverterPopup, NSLayoutAttribute.Left, NSLayoutRelation.Equal, valueConverterLabel, NSLayoutAttribute.Right, 1f, 5f),
				NSLayoutConstraint.Create (this.ValueConverterPopup, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.AddConverterButton, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ValueConverterPopup, NSLayoutAttribute.Top, 1f, 2f),
				NSLayoutConstraint.Create (this.AddConverterButton, NSLayoutAttribute.Left, NSLayoutRelation.Equal,this.ValueConverterPopup, NSLayoutAttribute.Right, 1f, 5f),
				NSLayoutConstraint.Create (this.AddConverterButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 20),
				NSLayoutConstraint.Create (this.AddConverterButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 20),

				NSLayoutConstraint.Create (this.ButtonMoreSettings, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ValueConverterPopup, NSLayoutAttribute.Bottom, 1f, 2f),
				NSLayoutConstraint.Create (this.ButtonMoreSettings, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Left, 1f, 16f),
				NSLayoutConstraint.Create (this.ButtonMoreSettings, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
				NSLayoutConstraint.Create (this.ButtonMoreSettings, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (labelOtherSettings, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ButtonMoreSettings, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (labelOtherSettings, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.ButtonMoreSettings, NSLayoutAttribute.Right, 1f, -5f),
				NSLayoutConstraint.Create (labelOtherSettings, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.BindingPropertiesView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ButtonMoreSettings, NSLayoutAttribute.Bottom, 1f, 10f),
				NSLayoutConstraint.Create (this.BindingPropertiesView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Left, 1f, 20f),
				NSLayoutConstraint.Create (this.BindingPropertiesView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.FlagsPropertiesView,  NSLayoutAttribute.Left, 1f, -10f),
				bindingPropertiesViewHeightConstraint,

				NSLayoutConstraint.Create (this.FlagsPropertiesView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.BindingPropertiesView,  NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.FlagsPropertiesView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.MainContainer,  NSLayoutAttribute.Right, 1f, -20f),
				NSLayoutConstraint.Create (this.FlagsPropertiesView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.BindingPropertiesView,  NSLayoutAttribute.Width, 1f, 0f),
				NSLayoutConstraint.Create (this.FlagsPropertiesView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this.BindingPropertiesView, NSLayoutAttribute.Height, 1f, 0f),

				NSLayoutConstraint.Create (this.buttonHelp, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Bottom, 1f, -40f),
				NSLayoutConstraint.Create (this.buttonHelp, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Left, 1f, 20),
				NSLayoutConstraint.Create (this.buttonHelp, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),
				NSLayoutConstraint.Create (this.buttonHelp, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1f, 24),

				NSLayoutConstraint.Create (this.ButtonDone, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Bottom, 1f, -40f),
				NSLayoutConstraint.Create (this.ButtonDone, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.MainContainer, NSLayoutAttribute.Right, 1f, -20f),
				NSLayoutConstraint.Create (this.ButtonDone, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),

				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.ButtonDone, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this.ButtonDone, NSLayoutAttribute.Left, 1f, -10f),
				NSLayoutConstraint.Create (this.buttonCancel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, PropertyEditorControl.DefaultControlHeight),
			});

			// put the MainContainer inside this panel's ContentView
			ContentView.AddSubview (this.MainContainer);

			ContentView.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.MainContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1f, 0f),
				NSLayoutConstraint.Create (this.MainContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (this.MainContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Height, 1f, 0f),
				NSLayoutConstraint.Create (this.MainContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Width, 1f, 0f),

			});
		}

		private static void ToggleSettingsLabel (bool show, UnfocusableTextField labelOtherSettings)
		{
			labelOtherSettings.StringValue = show ? Properties.Resources.ShowSettings : Properties.Resources.HideSettings;
		}
	}
}
