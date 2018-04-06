using System;
using System.Collections;
using System.ComponentModel;
using AppKit;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorPopUpButton : NSPopUpButton
	{
		public NSPopover Popover { get; set; }

		public ColorPopUpButton () : base ()
		{
		}

		public ColorPopUpButton (CGRect frame) : base (frame, true)
		{
		}

		public ColorPopUpButton (IntPtr handle) : base (handle)
		{
		}

		public override void MouseDown (NSEvent theEvent) {
			if (Popover != null)
				Popover.Show (new CGRect (20, this.Frame.Height/2 - 2.5, 5, 5), this, NSRectEdge.MinYEdge);
		}

        public override void Layout()
        {
            base.Layout();

        }
    }

	internal class BrushEditorControl : PropertyEditorControl
	{
		public BrushEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;
			RowHeight = 600f;

			//this.colorEditor = new SolidColorBrushEditor (new CGRect (0, 30, 239, 239));
			this.colorEditor = new BrushTabViewController ();


			this.popover = new NSPopover ();
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = new SolidColorBrushEditorViewController ();
			popover.ContentViewController.PreferredContentSize = new CGSize (200, 200);

			this.popUpButton = new ColorPopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				Popover = this.popover
			};

			popupButtonList = new NSMenu ();
			popUpButton.Menu = popupButtonList;

			popUpButton.Activated += (o, e) => {
				//popover.Show (popUpButton.Frame, popUpButton, NSRectEdge.MinYEdge);
			};

			UpdateTheme ();
		}

		internal new BrushPropertyViewModel ViewModel
		{
			get { return (BrushPropertyViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		readonly NSPopUpButton popUpButton;
		readonly BrushTabViewController colorEditor;
		readonly NSPopover popover;
		NSMenu popupButtonList;

		bool dataPopulated;

		public override NSView FirstKeyView => this.popUpButton;

		public override NSView LastKeyView => this.popUpButton;

		protected override void HandleErrorsChanged (object sender, DataErrorsChangedEventArgs e)
		{
		}

		protected override void SetEnabled ()
		{
		}

		protected override void UpdateAccessibilityValues ()
		{
		}

		protected override void UpdateErrorsDisplayed (IEnumerable errors)
		{
		}

		protected override void UpdateValue ()
		{
			this.colorEditor.ViewModel = ViewModel;
			var controller = this.popover.ContentViewController as SolidColorBrushEditorViewController;
			controller.ViewModel = ViewModel;

			if (ViewModel.Solid != null) {
				var title = ViewModel.Solid.Color.ToString ();

				if (popupButtonList.Count == 0)
					popupButtonList.AddItem (new NSMenuItem ());
				
				var item = popupButtonList.ItemAt (0);
				if (item.Title != title) {
					item.Title = title;
					item.Image = ViewModel?.Solid?.Color.CreateSwatch (new CGSize (30, 10));
				}
			}
		}

		protected override void OnViewModelChanged (PropertyViewModel oldModel)
		{
			if (!dataPopulated) {
				this.DoConstraints (new[] {
						popUpButton.ConstraintTo (this, (pub, c) => pub.Width == c.Width - 34),
						popUpButton.ConstraintTo (this, (pub, c) => pub.Height == DefaultControlHeight + 1),
						popUpButton.ConstraintTo (this, (pub, c) => pub.Left == pub.Left + 4),
						popUpButton.ConstraintTo (this, (pub, c) => pub.Top == pub.Top + 0),
					});

				AddSubview (this.popUpButton);
				AddSubview (this.colorEditor.View);
			}
			UpdateValue ();
			dataPopulated = true;
		}
	}
}
