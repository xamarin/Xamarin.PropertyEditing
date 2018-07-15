using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyButton : NSImageView
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
				if (viewModel == null)
					return;
				
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
			Enabled = true;
			Image = NSImage.ImageNamed ("property-button-default-mac-10");
			ImageScaling = NSImageScale.AxesIndependently;
			ToolTip = Properties.Resources.Default;
			TranslatesAutoresizingMaskIntoConstraints = false;
		}

		public override void MouseDown (NSEvent theEvent)
		{
			if (theEvent.Type == NSEventType.LeftMouseDown) {
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
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			ChangeActiveImage (viewModel.ValueSource, true);
		}

		public override void MouseExited (NSEvent theEvent)
		{
			ChangeActiveImage (viewModel.ValueSource);
		}

		public override void UpdateTrackingAreas ()
		{
			base.UpdateTrackingAreas ();

			foreach (var item in TrackingAreas ()) {
				RemoveTrackingArea (item);
			}

			var options = NSTrackingAreaOptions.MouseEnteredAndExited | NSTrackingAreaOptions.ActiveAlways;

			var trackingArea = new NSTrackingArea (this.Bounds, options, this, null);

			AddTrackingArea (trackingArea);
		}

		private void ChangeActiveImage (ValueSource valueSource, bool activeImage = false)
		{
			switch (valueSource) {
				case ValueSource.Binding:
					Image = activeImage ? NSImage.ImageNamed ("property-button-bound-mac-active-10") : NSImage.ImageNamed ("property-button-bound-mac-10");
					break;

				case ValueSource.Default:
					Image = activeImage ? NSImage.ImageNamed ("property-button-default-mac-active-10") : NSImage.ImageNamed ("property-button-default-mac-10");
					return;

				case ValueSource.Local:
					Image = activeImage ? NSImage.ImageNamed ("property-button-local-mac-active-10") : NSImage.ImageNamed ("property-button-local-mac-10");
					break;

				case ValueSource.Inherited:
					Image = activeImage ? NSImage.ImageNamed ("property-button-inherited-mac-active-10") : NSImage.ImageNamed ("property-button-inherited-mac-10");
					break;

				case ValueSource.Resource:
					Image = activeImage ? NSImage.ImageNamed ("property-button-inherited-mac-active-10") : NSImage.ImageNamed ("property-button-inherited-resource-mac-10");
					break;

				case ValueSource.Unset:
					Image = activeImage ? NSImage.ImageNamed ("property-button-default-mac-active-10") : NSImage.ImageNamed ("property-button-default-mac-10");
					break;

				default:
					// To Handle ValueSource.DefaultStyle, ValueSource.Style etc.
					Image = null;
					break;
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

			ChangeActiveImage (valueSource, false);
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
