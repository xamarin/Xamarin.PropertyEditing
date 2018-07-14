using System.Linq;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class SegmentStack : NSView
	{
		class SegmentItem
		{
			public NSImage Image { get; set; }
			public string Label { get; set; }
			public NSView View { get; set; }
		}


		private SegmentItem[] segments = new SegmentItem[0];
		private NSStackView stackView = new NSStackView () {
			Spacing = 3
		};

		public void SetImage (NSImage image, int segment)
		{
			var item = this.segments[segment];

			item.Image = image;
			if (item.Image != null)
				return;

			item.View = new UnderlinedImageView (image.Name) {
			//	Image = image
			};
			this.stackView.AddView (item.View, NSStackViewGravity.Leading);
		}

		public NSImage GetImage (int segment)
		{
			return this.segments[segment].Image;
		}

		public void SetLabel (string label, int segment)
		{
			var item = this.segments[segment];
			item.Label = label;
			if (item.Image != null)
				return;
			
			item.View = new UnderlinedTextField () {
				BackgroundColor = NSColor.Clear,
				Editable = false,
				Bezeled = false,
				StringValue = label
			};
			this.stackView.AddView (item.View, NSStackViewGravity.Leading);
		}

		public string GetLabel (int segment)
		{
			return this.segments[segment].Label;
		}

		public int SegmentCount
		{
			get => this.segments.Length;
			set
			{
				this.segments = new SegmentItem[value];
			}
		}
	}

	internal class UnderlinedTabViewController<TViewModel> : NotifyingTabViewController<TViewModel> where TViewModel : NotifyingObject
	{
		private NSStackView tabStack = new NSStackView () {
			Spacing = 4f,
		};

		public override void NumberOfItemsChanged (NSTabView tabView)
		{
			base.NumberOfItemsChanged (tabView);
			var items = this.tabStack.Views.ToList ();
			foreach (var view in items) {
				this.tabStack.RemoveView (view);
			}
			var i = 0;
			foreach (var item in TabViewItems) {
				if (item.Image != null) {
					this.tabStack.AddView (new UnderlinedImageView (item.Image.Name) {
						Selected = i == SelectedTabViewItemIndex
					}, NSStackViewGravity.Leading);
				} else {
					this.tabStack.AddView (new UnderlinedTextField () {
						BackgroundColor = NSColor.Clear,
						Editable = false,
						Bezeled = false,
						StringValue = item.Label,
						Selected = i == SelectedTabViewItemIndex,
					}, NSStackViewGravity.Leading);
				}
				i++;
			}
		}

		public override void MouseDown (NSEvent theEvent)
		{
			NSView hit = View.HitTest (View.Superview.ConvertPointFromView (theEvent.LocationInWindow, null));
			if (!(hit is ISelectable))
				return;

			int i = 0;
			foreach (var label in tabStack.Views) {
				if (hit == label) {
					SelectedTabViewItemIndex = i;
					break;
				}
				i++;
			}
		}

		public override void DidSelect (NSTabView tabView, NSTabViewItem item)
		{
			base.DidSelect (tabView, item);
			for (int i = 0; i < this.tabStack.Views.Length; i++) {
				var tabItem = this.tabStack.Views[i] as ISelectable;
				if (tabItem != null)
					tabItem.Selected = SelectedTabViewItemIndex == i;
			}
		}

		public override void LoadView ()
		{
			var stack = new NSStackView () {
				Spacing = 0,
				Alignment = NSLayoutAttribute.Left,
				Orientation = NSUserInterfaceLayoutOrientation.Vertical
			};

			stack.AddView (this.tabStack, NSStackViewGravity.Top);
			stack.AddView (TabView, NSStackViewGravity.Bottom);
			View = stack;
		}
	}
}
