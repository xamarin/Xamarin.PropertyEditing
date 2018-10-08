using System;
using System.Linq;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class UnderlinedTabViewController<TViewModel>
		: NotifyingTabViewController<TViewModel>
		where TViewModel : NotifyingObject
	{
		public override void InsertTabViewItem (NSTabViewItem tabViewItem, nint index)
		{
			this.tabStack.InsertView (GetView (tabViewItem), (nuint)index, NSStackViewGravity.Leading);
			base.InsertTabViewItem (tabViewItem, index);
		}

		public override void RemoveTabViewItem (NSTabViewItem tabViewItem)
		{
			int index = (int)TabView.IndexOf (tabViewItem);
			NSView tabView = this.tabStack.Views[index];
			this.tabStack.RemoveView (tabView);
			tabView.Dispose ();

			base.RemoveTabViewItem (tabViewItem);
		}

		public NSEdgeInsets EdgeInsets {
			get => this.edgeInsets;
			set {
				this.edgeInsets = value;
				if (this.outerStack != null) {
					this.outerStack.EdgeInsets = value;
					this.innerStack.EdgeInsets = value;
				}
			}
		}

		public override void MouseDown (NSEvent theEvent)
		{
			NSView hit = View.HitTest (View.Superview.ConvertPointFromView (theEvent.LocationInWindow, null));
			if (!(hit is IUnderliningTabView))
				return;

			SelectedTabViewItemIndex = Array.IndexOf (this.tabStack.Views, hit);
		}

		public override void DidSelect (NSTabView tabView, NSTabViewItem item)
		{
			base.DidSelect (tabView, item);

			int i = (int)TabView.IndexOf (item);
			SetSelected (i);
		}

		public override nint SelectedTabViewItemIndex
		{
			get => base.SelectedTabViewItemIndex;
			set
			{
				base.SelectedTabViewItemIndex = value;
				SetSelected ((int)value);
			}
		}

		public override void LoadView ()
		{
			this.outerStack = new NSStackView () {
				Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
				Spacing = 0,
				EdgeInsets = new NSEdgeInsets (0, edgeInsets.Left, 0, edgeInsets.Right)
			};

			this.innerStack = new NSStackView () {
				Spacing = 0,
				Alignment = NSLayoutAttribute.Left,
				Orientation = NSUserInterfaceLayoutOrientation.Vertical,
				EdgeInsets = new NSEdgeInsets (edgeInsets.Top, 0, edgeInsets.Bottom, 0)
			};

			this.outerStack.AddView (this.innerStack, NSStackViewGravity.Leading);
			this.innerStack.AddView (this.tabStack, NSStackViewGravity.Leading);
			this.innerStack.AddView (TabView, NSStackViewGravity.Bottom);
			View = this.outerStack;
		}

		private IUnderliningTabView selected;
		private NSStackView outerStack;
		private NSStackView innerStack;
		private NSStackView tabStack = new NSStackView () {
			Spacing = 10f,
		};

		protected NSStackView TabStack => this.tabStack;

		private NSEdgeInsets edgeInsets = new NSEdgeInsets (0, 0, 0, 0);

		private void SetSelected (int index)
		{
			if (this.selected != null) {
				this.selected.Selected = false;
			}

			this.selected = this.tabStack.Views[index] as IUnderliningTabView;
			if (this.selected != null)
				this.selected.Selected = true;
		}

		private NSView GetView (NSTabViewItem item)
		{
			NSView tabView;
			if (item.Image != null) {
				tabView = new UnderlinedImageView (item.Image.Name) {
					Selected = this.tabStack.Views.Length == SelectedTabViewItemIndex,
					ToolTip = item.ToolTip
				};
			} else {
				tabView = new UnderlinedTextField {
					BackgroundColor = NSColor.Clear,
					Editable = false,
					Bezeled = false,
					StringValue = item.Label,
					Selected = this.tabStack.Views.Length == SelectedTabViewItemIndex,
					ToolTip = item.ToolTip
				};
			}

			return tabView;
		}
	}
}
