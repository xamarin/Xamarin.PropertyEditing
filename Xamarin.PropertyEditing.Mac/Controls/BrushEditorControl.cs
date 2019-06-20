using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorPopUpButton : FocusablePopUpButton
	{
		public ColorPopUpButton (IHostResourceProvider hostResources)
			: base (hostResources)
		{
		}

		public NSPopover Popover { get; set; }

		public override void MouseDown (NSEvent theEvent)
		{
			if (Popover == null)
				return;

			if (!Popover.Shown) {
				Popover.Show (new CGRect (26, this.Frame.Height / 2 - 2, 2, 2), this, NSRectEdge.MinYEdge);
				Window.MakeFirstResponder (Popover);
			}
		}

		public override void KeyDown (NSEvent theEvent)
		{
			if (theEvent.KeyCode == 36 || theEvent.KeyCode == 49) {
				MouseDown (theEvent);
			}
			else {
				base.KeyDown (theEvent);
			}
		}
	}

	internal class BrushEditorControl : PropertyEditorControl<BrushPropertyViewModel>
	{
		public BrushEditorControl (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.previewLayer = new CommonBrushLayer (hostResources) {
				Frame = new CGRect (0, 0, 30, 10)
			};

			this.popover = new NSPopover {
				Behavior = NSPopoverBehavior.Transient,
				ContentViewController = this.brushTabViewController = new BrushTabViewController (hostResources) {
					PreferredContentSize = new CGSize (550, 363)
				}
			};

			this.popUpButton = new ColorPopUpButton (hostResources) {
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.popupButtonList = new NSMenu ();
			this.popUpButton.Menu = this.popupButtonList;

			AddSubview (this.popUpButton);

			this.AddConstraints (new[] {
				NSLayoutConstraint.Create (this.popUpButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this,  NSLayoutAttribute.Width, 1f, 0),
				NSLayoutConstraint.Create (this.popUpButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 16),
				NSLayoutConstraint.Create (this.popUpButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1f, 0),
			});

			AppearanceChanged ();
		}

		readonly ColorPopUpButton popUpButton;
		readonly NSPopover popover;
		readonly BrushTabViewController brushTabViewController;
		readonly NSMenu popupButtonList;
		readonly CommonBrushLayer previewLayer;

		public override NSView FirstKeyView => this.popUpButton;
		public override NSView LastKeyView => this.popUpButton;

		protected override void SetEnabled ()
		{
			this.popUpButton.Enabled = this.ViewModel?.Property.CanWrite ?? false;
		}

		NSAttributedString GetTitle ()
		{
			var title = Properties.Resources.CommonBrushTitleUnknown;
			switch (ViewModel.Value) {
				case CommonSolidBrush solid:
					title = solid.Color.ToString ();
					break;
				case CommonGradientBrush gradient:
					title = Properties.Resources.CommonBrushTitleGradient;
					break;
				default:
					if (ViewModel.Value == null)
						title = Properties.Resources.NoBrush;
					break;
			}

			var color = HostResources.GetNamedColor (NamedResources.ForegroundColor);
			return new NSAttributedString (title, new NSStringAttributes { ForegroundColor = color });
		}

		protected override void UpdateValue ()
		{
			this.brushTabViewController.ViewModel = ViewModel;
			this.popUpButton.Popover = (ViewModel?.Property.CanWrite ?? false) ? this.popover : null;

			if (ViewModel.Solid != null) {
				if (this.popupButtonList.Count == 0)
					this.popupButtonList.AddItem (new NSMenuItem ());

				this.previewLayer.Brush = ViewModel.Value;
				var item = this.popupButtonList.ItemAt (0);
				item.AttributedTitle = GetTitle();
				item.Image = this.previewLayer.RenderPreview ();
			}
		}

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			this.popover.SetAppearance (HostResources.GetVibrantAppearance (EffectiveAppearance));
			this.popover.ContentViewController.View.Appearance = EffectiveAppearance;
		}
	}
}
