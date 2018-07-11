using System.Linq;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class UnderlinedTabViewController<TViewModel> : NotifyingTabViewController<TViewModel> where TViewModel : NotifyingObject
	{
		private NSStackView labelStack = new NSStackView () {
			Spacing = 4f,
		};

		public override void NumberOfItemsChanged (NSTabView tabView)
		{
			base.NumberOfItemsChanged (tabView);
			var items = this.labelStack.Views.ToList ();
			foreach (var view in items) {
				this.labelStack.RemoveView (view);
			}
			var i = 0;
			foreach (var item in TabViewItems) {
				this.labelStack.AddView (new UnderlinedTextField () {
					BackgroundColor = NSColor.Clear,
					Editable = false,
					Bezeled = false,
					StringValue = item.Label,
					Selected = i == SelectedTabViewItemIndex,
				}, NSStackViewGravity.Leading);
				i++;
			}
		}

		public override void MouseDown (NSEvent theEvent)
		{
			var hit = View.HitTest (View.Superview.ConvertPointFromView (theEvent.LocationInWindow, null));
			if (!(hit is UnderlinedTextField))
				return;

			int i = 0;
			foreach (var label in labelStack.Views) {
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
			for (int i = 0; i < this.labelStack.Views.Length; i++) {
				var underlined = this.labelStack.Views[i] as UnderlinedTextField;
				if (underlined != null)
					underlined.Selected = SelectedTabViewItemIndex == i;
			}
		}

		public override void LoadView ()
		{
			var stack = new NSStackView () {
				Spacing = 0,
				Alignment = NSLayoutAttribute.Width,
				Orientation = NSUserInterfaceLayoutOrientation.Vertical
			};

			stack.AddView (this.labelStack, NSStackViewGravity.Leading);
			stack.AddView (TabView, NSStackViewGravity.Bottom);
			View = stack;
		}
	}
}
