using System;

namespace Xamarin.PropertyEditing.Mac
{
	internal enum ProxyRowType
	{
		SingleView,
		FirstView,
		LastView
	}

	internal class RowProxyResponder 
	{
		protected WeakReference<PropertyEditorControl> editorControl;

		readonly ProxyRowType rowType;

		public RowProxyResponder (PropertyEditorControl editorControl, ProxyRowType rowType)
		{
			this.rowType = rowType;
			this.editorControl = new WeakReference<PropertyEditorControl> (editorControl);
		}

		public bool NextResponder()
		{
			if (this.editorControl.TryGetTarget (out var editor))
			{
				if (this.rowType == ProxyRowType.LastView || this.rowType == ProxyRowType.SingleView) {
					editor.OnNextResponderRequested ();
					return true;
				}
			}
			return false;
		}

		public bool PreviousResponder ()
		{
			if (this.editorControl.TryGetTarget (out var editor))
			{
				if (this.rowType == ProxyRowType.FirstView || this.rowType == ProxyRowType.SingleView) {
					editor.OnPreviousResponderRequested ();
					return true;
				}
			}
			return false;
		}
	}
}
