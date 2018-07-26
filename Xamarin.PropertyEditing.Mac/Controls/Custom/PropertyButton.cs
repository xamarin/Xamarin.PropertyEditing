﻿using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.Themes;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyButton : UnfocusableButton
	{
		NSMenu popUpContextMenu;

		PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel
		{
			get { return viewModel; }
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= OnPropertyChanged;
				}

				viewModel = value;
				viewModel.PropertyChanged += OnPropertyChanged;

				// No point showing myself if you can't do anything with me.
				Hidden = !viewModel.Property.CanWrite;

				ValueSourceChanged (viewModel.ValueSource);
			}
		}

		public PropertyButton ()
		{
			AccessibilityTitle = LocalizationResources.AccessibilityPropertiesButton;
			AccessibilityHelp = LocalizationResources.AccessibilityPropertiesButtonDescription;
			Image = NSImage.ImageNamed ("property-button-default-mac-10");
			ToolTip = Properties.Resources.Default;
			TranslatesAutoresizingMaskIntoConstraints = false;

			OnMouseEntered += (sender, e) => {
				ToggleFocusImage (true);
			};

			OnMouseExited += (sender, e) => {
				ToggleFocusImage ();
			};

			OnMouseLeftDown += (sender, e) => {
				PopUpContextMenu ();
			};
		}

		private void PopUpContextMenu ()
		{
			if (this.popUpContextMenu == null) {
				this.popUpContextMenu = new NSMenu ();

				if (this.viewModel.TargetPlatform.SupportsCustomExpressions) {
					var mi = new NSMenuItem (Properties.Resources.CustomExpressionEllipsis) {
						AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.CustomExpressionEllipsis,
						new CoreText.CTStringAttributes () {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
					};

					mi.Activated += OnCustomExpression;

					this.popUpContextMenu.AddItem (mi);
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);
				}

				// TODO If we add more menu items consider making the Label/Command a dictionary that we can iterate over to populate everything.
				this.popUpContextMenu.AddItem (new CommandMenuItem (Properties.Resources.Reset, viewModel.ClearValueCommand) {
					AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.Reset,
						new CoreText.CTStringAttributes () {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
				});
			}

			var menuOrigin = this.Superview.ConvertPointToView (new CGPoint (Frame.Location.X - 1, Frame.Location.Y + Frame.Size.Height + 4), null);

			var popupMenuEvent = NSEvent.MouseEvent (NSEventType.LeftMouseDown, menuOrigin, (NSEventModifierMask)0x100, 0, this.Window.WindowNumber, this.Window.GraphicsContext, 0, 1, 1);

			NSMenu.PopUpContextMenu (popUpContextMenu, popupMenuEvent, this);
		}

		protected override void UpdateTheme ()
		{
			base.UpdateTheme ();

			ToggleFocusImage ();
		}

		private void ToggleFocusImage (bool focused = false)
		{
			if (viewModel != null) {

				switch (viewModel.ValueSource) {
				case ValueSource.Binding:
					switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = focused ? NSImage.ImageNamed ("property-button-bound-mac-active-10~dark") : NSImage.ImageNamed ("property-button-bound-mac-10~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = focused ? NSImage.ImageNamed ("property-button-bound-mac-active-10") : NSImage.ImageNamed ("property-button-bound-mac-10");
						break;

					case PropertyEditorTheme.None:
						Image = focused ? NSImage.ImageNamed ("property-button-bound-mac-active-10") : NSImage.ImageNamed ("property-button-bound-mac-10");
						break;
					}
					break;

				case ValueSource.Default:
					switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = focused ? NSImage.ImageNamed ("property-button-default-mac-active-10~dark") : NSImage.ImageNamed ("property-button-default-mac-10~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = focused ? NSImage.ImageNamed ("property-button-default-mac-active-10") : NSImage.ImageNamed ("property-button-default-mac-10");
						break;

					case PropertyEditorTheme.None:
						Image = focused ? NSImage.ImageNamed ("property-button-default-mac-active-10") : NSImage.ImageNamed ("property-button-default-mac-10");
						break;
					}
					return;

				case ValueSource.Local:
					switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = focused ? NSImage.ImageNamed ("property-button-local-mac-active-10~dark") : NSImage.ImageNamed ("property-button-local-mac-10~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = focused ? NSImage.ImageNamed ("property-button-local-mac-active-10") : NSImage.ImageNamed ("property-button-local-mac-10");
						break;

					case PropertyEditorTheme.None:
						Image = focused ? NSImage.ImageNamed ("property-button-local-mac-active-10") : NSImage.ImageNamed ("property-button-local-mac-10");
						break;
					}
					break;

				case ValueSource.Inherited:
					switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = focused ? NSImage.ImageNamed ("property-button-inherited-mac-active-10~dark") : NSImage.ImageNamed ("property-button-inherited-mac-10~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = focused ? NSImage.ImageNamed ("property-button-inherited-mac-active-10") : NSImage.ImageNamed ("property-button-inherited-mac-10");
						break;

					case PropertyEditorTheme.None:
						Image = focused ? NSImage.ImageNamed ("property-button-inherited-mac-active-10") : NSImage.ImageNamed ("property-button-inherited-mac-10");
						break;
					}
					break;

				case ValueSource.Resource:
					switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = focused ? NSImage.ImageNamed ("property-button-inherited-mac-active-10~dark") : NSImage.ImageNamed ("property-button-inherited-mac-10~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = focused ? NSImage.ImageNamed ("property-button-inherited-mac-active-10") : NSImage.ImageNamed ("property-button-inherited-mac-10");
						break;

					case PropertyEditorTheme.None:
						Image = focused ? NSImage.ImageNamed ("property-button-inherited-mac-active-10") : NSImage.ImageNamed ("property-button-inherited-mac-10");
						break;
					}
					break;

				case ValueSource.Unset:
					switch (PropertyEditorPanel.ThemeManager.Theme) {
					case PropertyEditorTheme.Dark:
						Image = focused ? NSImage.ImageNamed ("property-button-default-mac-active-10~dark") : NSImage.ImageNamed ("property-button-default-mac-10~dark");
						break;

					case PropertyEditorTheme.Light:
						Image = focused ? NSImage.ImageNamed ("property-button-default-mac-active-10") : NSImage.ImageNamed ("property-button-default-mac-10");
						break;

					case PropertyEditorTheme.None:
						Image = focused ? NSImage.ImageNamed ("property-button-default-mac-active-10") : NSImage.ImageNamed ("property-button-default-mac-10");
						break;
					}
					break;

				default:
					// To Handle ValueSource.DefaultStyle, ValueSource.Style etc.
					Image = null;
					break;
				}
			}
		}

		private void ValueSourceChanged (ValueSource valueSource)
		{
			switch (valueSource) {
			case ValueSource.Binding:
				ToolTip = Properties.Resources.Binding;
				break;

			case ValueSource.Default:
				ToolTip = Properties.Resources.Default;
				return;

			case ValueSource.Local:
				ToolTip = Properties.Resources.Local;
				break;

			case ValueSource.Inherited:
				ToolTip = Properties.Resources.Inherited;
				break;

			case ValueSource.Resource:
				ToolTip = (this.viewModel?.Resource?.Name != null) ? String.Format (Properties.Resources.ResourceWithName, this.viewModel.Resource.Name) : Properties.Resources.Resource;
				break;

			case ValueSource.Unset:
				ToolTip = Properties.Resources.Unset;
				break;

			default:
				// To Handle ValueSource.DefaultStyle, ValueSource.Style etc.
				ToolTip = string.Empty;
				break;
			}

			UpdateTheme ();
		}

		private void OnPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (viewModel.ValueSource)) {
				ValueSourceChanged (viewModel.ValueSource);
			}
		}

		private void OnCustomExpression (object sender, EventArgs e)
		{
			var customExpressionView = new CustomExpressionView (viewModel);

			var customExpressionPopOver = new AutoClosePopOver {
				ContentViewController = new NSViewController (null, null) { View = customExpressionView },
			};

			customExpressionPopOver.Show (customExpressionView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}
	}
}
