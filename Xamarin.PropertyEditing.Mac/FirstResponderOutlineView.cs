using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FirstResponderOutlineView : NSOutlineView
	{
		private bool tabbedIn;
		public override bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
		{
			return true;
		}

		public override bool BecomeFirstResponder ()
		{
			var willBecomeFirstResponder = base.BecomeFirstResponder ();
			if (willBecomeFirstResponder) {
				if (SelectedRows.Count == 0 && RowCount > 0) {
					SelectRow (0, false);
					this.tabbedIn = true;
					var row = GetRowView ((nint)SelectedRows.FirstIndex, false);
					return Window.MakeFirstResponder (row.NextValidKeyView);
				}
			}
			this.tabbedIn = false;
			return willBecomeFirstResponder;
		}

		public override bool ResignFirstResponder ()
		{
			var wilResignFirstResponder = base.ResignFirstResponder ();
			if (wilResignFirstResponder) {
				if (SelectedRows.Count > 0 && !this.tabbedIn) {
					DeselectRow ((nint)SelectedRows.FirstIndex);
				}
			}
			return wilResignFirstResponder;
		}
	}
}
