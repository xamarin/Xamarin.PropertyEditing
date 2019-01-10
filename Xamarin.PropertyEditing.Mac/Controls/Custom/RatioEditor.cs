using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Mac;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RatioEditor<T> : NumericSpinEditor<T>
	{
		private bool fullSelection;

		public RatioEditor ()
		{
			AllowNegativeValues = false;
			AllowRatios = true;
			BackgroundColor = NSColor.Clear;
			StringValue = string.Empty;
			TranslatesAutoresizingMaskIntoConstraints = false;
		}

		protected override void OnEditingEnded (object sender, EventArgs e)
		{
			if (!this.editing) {
				this.editing = true;
				NotifyingValueChanged (new RatioEventArgs (0, 0, 0));
				this.editing = false;
			}
		}

		protected override void SetIncrementOrDecrementValue (double incDevValue)
		{
			nint caretLocation = 0;
			nint selectionLength = 0;

			GetEditorCaretLocationAndLength (out caretLocation, out selectionLength);

			// Fire A Value change, so things are updated
			NotifyingValueChanged (new RatioEventArgs ((int)caretLocation, (int)selectionLength, incDevValue));

			// Resposition our caret so it doesn't jump around.
			SetEditorCaretLocationAndLength (caretLocation, selectionLength);
		}

		private void SetEditorCaretLocationAndLength (nint caretLocation, nint selectionLength)
		{
			if (NumericEditor.CurrentEditor != null) {
				if (fullSelection && (selectionLength != NumericEditor.StringValue.Length)) {
					selectionLength = NumericEditor.StringValue.Length;
				}
				NumericEditor.CurrentEditor.SelectedRange = new NSRange (caretLocation, selectionLength);
			}
		}

		private void GetEditorCaretLocationAndLength (out nint caretLocation, out nint selectionLength)
		{
			caretLocation = 0;
			selectionLength = 0;
			if (NumericEditor.CurrentEditor != null) {
				caretLocation = NumericEditor.CurrentEditor.SelectedRange.Location;
				selectionLength = NumericEditor.CurrentEditor.SelectedRange.Length;
				fullSelection = NumericEditor.StringValue.Length == selectionLength;
			}
		}

		public class RatioEventArgs : EventArgs
		{
			public RatioEventArgs (int caretPosition, int selectionLength, double incrementValue)
			{
				CaretPosition = caretPosition;
				SelectionLength = selectionLength;
				IncrementValue = incrementValue;
			}

			public int CaretPosition { get; }
			public int SelectionLength { get; }
			public double IncrementValue { get; }
		}
	}
}
