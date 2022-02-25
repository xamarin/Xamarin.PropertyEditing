using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorPopUpButton : FocusablePopUpButton
	{
		public NSPopover Popover { get; set; }

		public override void MouseDown (NSEvent theEvent)
		{
			if (Popover == null)
				return;

			if (!Popover.Shown) {
				Popover.Show (new CGRect (26, this.Frame.Height / 2 - 2, 2, 2), this, NSRectEdge.MinYEdge);
			}
		}

		public override void KeyDown (NSEvent theEvent)
		{
			if (theEvent.KeyCode == (ushort)NSKey.Space) {
				MouseDown (theEvent);
			} else {
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

			this.popover = new AutoClosePopOver (hostResources, hostResources.GetVibrantAppearance (EffectiveAppearance)) {
				ContentViewController = this.brushTabViewController = new BrushTabViewController (hostResources) {
					PreferredContentSize = new CGSize (550, 363)
				}
			};

			this.popUpButton = new ColorPopUpButton {
				ControlSize = NSControlSize.Small,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Small)),
				TranslatesAutoresizingMaskIntoConstraints = false,
				ProxyResponder = new ProxyResponder (this, ProxyRowType.SingleView)
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

		string GetTitle ()
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

			return title;
		}

		protected override void UpdateValue ()
		{
			this.brushTabViewController.ViewModel = ViewModel;
			this.popUpButton.Popover = (ViewModel?.Property.CanWrite ?? false) ? this.popover : null;

			if (ViewModel.Solid != null) {
				var title = GetTitle ();

				if (this.popupButtonList.Count == 0)
					this.popupButtonList.AddItem (new NSMenuItem ());

				this.previewLayer.Brush = ViewModel.Value;
				var item = this.popupButtonList.ItemAt (0);
				if (item.Title != title) {
					item.Title = title;
					item.Image = this.previewLayer.RenderPreview ();
				}
			}
		}

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			this.popover.Appearance = HostResources.GetVibrantAppearance (EffectiveAppearance);
			this.popover.ContentViewController.View.Appearance = EffectiveAppearance;
		}

		protected override void UpdateAccessibilityValues ()
		{
			this.popUpButton.AccessibilityEnabled = this.popUpButton.Enabled;
			this.popUpButton.AccessibilityTitle = string.Format (Properties.Resources.AccessibilityPopUp, ViewModel.Property.Name);
		}
	}
}
