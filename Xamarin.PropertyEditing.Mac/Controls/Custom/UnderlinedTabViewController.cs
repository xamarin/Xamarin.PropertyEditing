using System;

using AppKit;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class UnderlinedTabViewController<TViewModel>
		: NotifyingTabViewController<TViewModel>
		where TViewModel : NotifyingObject
	{
		public UnderlinedTabViewController (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			HostResources = hostResources;
		}

		public string TabBackgroundColor
		{
			get => this.tabBackground;
			set
			{
				if (this.tabContainer != null)
					this.tabContainer.FillColorName = value;

				this.tabBackground = value;
			}
		}

		public string TabBorderColor
		{
			get => this.tabBorder;
			set
			{
				if (this.border != null)
					this.border.FillColorName = value;

				this.tabBorder = value;
			}
		}

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

		/// <remarks>This doesn't actually support independent left/right padding, they'll be added and content centered.</remarks>
		public NSEdgeInsets ContentPadding {
			get => this.edgeInsets;
			set {
				this.edgeInsets = value;
				UpdatePadding ();
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
			this.innerStack = new NSStackView () {
				Spacing = 0,
				Alignment = NSLayoutAttribute.Left,
				Orientation = NSUserInterfaceLayoutOrientation.Vertical,
			};

			NSView tabs = this.tabStack;
			if (TabBackgroundColor != null) {
				this.tabContainer = new DynamicFillBox (HostResources, TabBackgroundColor) {
					ContentView = this.tabStack
				};

				tabs = this.tabContainer;
			}

			this.innerStack.AddView (tabs, NSStackViewGravity.Top);

			if (TabBorderColor != null) {
				this.border = new DynamicFillBox (HostResources, TabBorderColor)	{
					Frame = new CGRect (0, 0, 1, 1),
					AutoresizingMask = NSViewResizingMask.WidthSizable
				};

				this.innerStack.AddView (this.border, NSStackViewGravity.Top);
			}

			this.innerStack.AddView (TabView, NSStackViewGravity.Bottom);

			View = this.innerStack;

			if (TabBackgroundColor != null) {
				this.innerStack.AddConstraint (NSLayoutConstraint.Create (this.tabContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.innerStack, NSLayoutAttribute.Width, 1, 0));
			}

			this.innerStack.AddConstraint (NSLayoutConstraint.Create (TabView, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this.innerStack, NSLayoutAttribute.CenterX, 1, 0));

			UpdatePadding ();
		}

		protected IHostResourceProvider HostResources
		{
			get;
		}

		private NSLayoutConstraint sidePadding, topPadding, bottomPadding;

		private string tabBackground, tabBorder;
		private IUnderliningTabView selected;
		private DynamicFillBox tabContainer, border;
		private NSStackView innerStack;
		private NSStackView tabStack = new NSStackView () {
			Spacing = 10f,
		};

		protected NSStackView TabStack => this.tabStack;

		private NSEdgeInsets edgeInsets = new NSEdgeInsets (0, 0, 0, 0);

		private void UpdatePadding()
		{
			if (this.innerStack == null)
				return;

			if (this.sidePadding != null)
				this.innerStack.RemoveConstraints (new[] { this.sidePadding, this.topPadding, this.bottomPadding });

			this.sidePadding = NSLayoutConstraint.Create (TabView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.innerStack, NSLayoutAttribute.Width, 1, -(ContentPadding.Left + ContentPadding.Right));
			this.bottomPadding = NSLayoutConstraint.Create (TabView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.innerStack, NSLayoutAttribute.Bottom, 1, -(ContentPadding.Bottom));

			NSView bottomItem = this.border ?? this.tabContainer ?? (NSView)this.tabStack;
			this.topPadding = NSLayoutConstraint.Create (TabView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, bottomItem, NSLayoutAttribute.Bottom, 1, ContentPadding.Top);

			this.innerStack.AddConstraints (new[] { this.sidePadding, this.topPadding, this.bottomPadding });
		}

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
			var id = item.Identifier as NSObjectFacade;
			NSView tabView;
			if (id != null) {
				tabView = new UnderlinedImageView (HostResources, ((string)id.Target)) {
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
