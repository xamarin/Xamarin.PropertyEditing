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
    }

	internal class BrushEditorControl : PropertyEditorControl
	{
		public BrushEditorControl ()
		{
			base.TranslatesAutoresizingMaskIntoConstraints = false;
			RowHeight = 230f;

			this.colorEditor = new BrushTabViewController ();

			this.popover = new NSPopover ();
			popover.Behavior = NSPopoverBehavior.Transient;
			popover.ContentViewController = new BrushTabViewController ();

			this.popUpButton = new ColorPopUpButton {
				TranslatesAutoresizingMaskIntoConstraints = false,
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (DefaultFontName, DefaultFontSize),
				Popover = this.popover
			};

			popupButtonList = new NSMenu ();
			popUpButton.Menu = popupButtonList;

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
		readonly CommonBrushLayer previewLayer = new CommonBrushLayer {
			Frame = new CGRect (0, 0, 30, 10)
		};

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

		string GetTitle ()
		{
			var title = "Unknown";
			switch (ViewModel.Value) {
				case CommonSolidBrush solid:
					title = solid.Color.ToString ();
					break;
				case CommonGradientBrush gradient:
					title = "Gradient";
					break;
				default:
					if (ViewModel.Value == null)
					title = "null";
					break;
			}

			//return ViewModel.Resource == null ? title : $"{title} - (Resource: {ViewModel?.Resource?.Name})";
			return title;
		}

		protected override void UpdateValue ()
		{
			this.colorEditor.ViewModel = ViewModel;
			var controller = this.popover.ContentViewController as BrushTabViewController;
			controller.ViewModel = ViewModel;

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
