using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.Themes;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public abstract class PropertyButton : UnfocusableButton
	{
		private NSMenu popUpContextMenu;

		private PropertyViewModel viewModel;
		internal PropertyViewModel ViewModel
		{
			get { return this.viewModel; }
			set
			{
				if (this.viewModel != null) {
					this.viewModel.PropertyChanged -= OnPropertyChanged;
				}

				this.viewModel = value;
				this.viewModel.PropertyChanged += OnPropertyChanged;

				// No point showing myself if you can't do anything with me.
				Hidden = !this.viewModel.Property.CanWrite;

				ValueSourceChanged (this.viewModel.ValueSource);
			}
		}

		public PropertyButton ()
		{
			AccessibilityTitle = LocalizationResources.AccessibilityPropertiesButton;
			AccessibilityHelp = LocalizationResources.AccessibilityPropertiesButtonDescription;
			Enabled = true;
			Image = PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-default-mac-10");
			ImageScaling = NSImageScale.None;
			ToolTip = Properties.Resources.Default;

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
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
					};

					mi.Activated += OnCustomExpression;

					this.popUpContextMenu.AddItem (mi);
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);
				}

				if (this.viewModel.SupportsResources) {
					this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);

					var mi2 = new NSMenuItem (Properties.Resources.ResourceEllipsis) {
						AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.ResourceEllipsis,
						new CoreText.CTStringAttributes {
							Font = new CoreText.CTFont (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize + 1),
						})
					};

					mi2.Activated += OnResourceRequested;
					this.popUpContextMenu.AddItem (mi2);
				}

				this.popUpContextMenu.AddItem (NSMenuItem.SeparatorItem);

				// TODO If we add more menu items consider making the Label/Command a dictionary that we can iterate over to populate everything.
				this.popUpContextMenu.AddItem (new CommandMenuItem (Properties.Resources.Reset, viewModel.ClearValueCommand) {
					AttributedTitle = new Foundation.NSAttributedString (
						Properties.Resources.Reset,
						new CoreText.CTStringAttributes {
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
			if (this.viewModel != null) {

				switch (this.viewModel.ValueSource) {
					case ValueSource.Binding:
						Image = focused ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-bound-mac-active-10") : PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-bound-mac-10");
						break;

					case ValueSource.Default:
						Image = focused ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-default-mac-active-10") : PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-default-mac-10");
						break;

					case ValueSource.Local:
						Image = focused ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-local-mac-active-10") : PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-local-mac-10");
						break;

					case ValueSource.Inherited:
						Image = focused ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-inherited-mac-active-10") : PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-inherited-mac-10");
						break;

					case ValueSource.Resource:
						Image = focused ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-inherited-mac-active-10") : PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-inherited-mac-10");
						break;

					case ValueSource.Unset:
						Image = focused ? PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-default-mac-active-10") : PropertyEditorPanel.ThemeManager.GetImageForTheme ("property-button-default-mac-10");
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
			if (e.PropertyName == nameof (this.viewModel.ValueSource)) {
				ValueSourceChanged (this.viewModel.ValueSource);
			}
		}

		protected abstract void OnCustomExpression (object sender, EventArgs e);

		private void OnResourceRequested (object sender, EventArgs e)
		{
			var requestResourceView = new RequestResourceView (this.viewModel);

			var resourceSelectorPopOver = new AutoClosePopOver {
				ContentViewController = new NSViewController (null, null) { View = requestResourceView },
			};

			requestResourceView.PopOver = resourceSelectorPopOver;

			resourceSelectorPopOver.Show (requestResourceView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}
	}

	public class PropertyButton<T> : PropertyButton
	{
		internal new PropertyViewModel<T> ViewModel
		{
			get { return (PropertyViewModel<T>)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		protected override void OnCustomExpression (object sender, EventArgs e)
		{
			var customExpressionView = new CustomExpressionView<T> (ViewModel);

			var customExpressionPopOver = new AutoClosePopOver {
				ContentViewController = new NSViewController (null, null) { View = customExpressionView },
			};

			customExpressionPopOver.Show(customExpressionView.Frame, (NSView)this, NSRectEdge.MinYEdge);
		}
	}
}
