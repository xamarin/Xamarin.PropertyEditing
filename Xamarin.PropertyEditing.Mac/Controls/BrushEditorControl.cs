using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac.Resources;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorPopUpButton : NSPopUpButton
	{
		public ColorPopUpButton () : base ()
		{
		}

		public ColorPopUpButton (CGRect frame) : base (frame, true)
		{
		}

		public ColorPopUpButton (IntPtr handle) : base (handle)
		{
		}

		public NSPopover Popover { get; set; }

		public override void MouseDown (NSEvent theEvent)
		{
			if (Popover == null)
				return;

			if (!Popover.Shown)
				Popover.Show (new CGRect (26, this.Frame.Height / 2 - 2, 2, 2), this, NSRectEdge.MinYEdge);
		}
	}

	internal class BrushEditorControl : PropertyEditorControl<BrushPropertyViewModel>
	{
		public BrushEditorControl ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.popover = new NSPopover ();
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = brushTabViewController = new BrushTabViewController {
				PreferredContentSize = new CGSize (430, 263)
			};

			this.popUpButton = new ColorPopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
			};

			popupButtonList = new NSMenu ();
			popUpButton.Menu = popupButtonList;

			this.DoConstraints (new [] {
				popUpButton.ConstraintTo (this, (pub, c) => pub.Width == c.Width - 34),
				popUpButton.ConstraintTo (this, (pub, c) => pub.Height == DefaultControlHeight + 1),
				popUpButton.ConstraintTo (this, (pub, c) => pub.Left == c.Left + 4),
				popUpButton.ConstraintTo (this, (pub, c) => pub.Top == c.Top + 0),
			});

			AddSubview (this.popUpButton);

			UpdateTheme ();
		}


		readonly ColorPopUpButton popUpButton;
		readonly NSPopover popover;
		readonly BrushTabViewController brushTabViewController;
		readonly NSMenu popupButtonList;
		readonly CommonBrushLayer previewLayer = new CommonBrushLayer {
			Frame = new CGRect (0, 0, 30, 10)
		};

		public override NSView FirstKeyView => this.popUpButton;
		public override NSView LastKeyView => this.popUpButton;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
		}

		protected override void SetEnabled ()
		{
			this.popUpButton.Enabled = this.ViewModel?.Property.CanWrite ?? false;
		}

		protected override void UpdateAccessibilityValues ()
		{
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
		}

		string GetTitle ()
		{
			var title = LocalizationResources.CommonBrushTitleUnknown;
			switch (ViewModel.Value) {
				case CommonSolidBrush solid:
					title = solid.Color.ToString ();
					break;
				case CommonGradientBrush gradient:
					title = LocalizationResources.CommonBrushTitleGradient;
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
			this.popUpButton.Popover = (ViewModel?.Property.CanWrite ?? false) ? popover : null;

			if (ViewModel.Solid != null) {
				var title = GetTitle ();

				if (popupButtonList.Count == 0)
					popupButtonList.AddItem (new NSMenuItem ());

				previewLayer.Brush = ViewModel.Value;
				var item = popupButtonList.ItemAt (0);
				if (item.Title != title) {
					item.Title = title;
					item.Image = previewLayer.RenderPreview ();
				}
			}
		}
	}
}
