using Foundation;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	class FocusableComboBox : NSComboBox
	{
		public override bool ShouldBeginEditing (NSText textObject)
		{
			textObject.Delegate = new FocusableComboBoxDelegate ();
			return false;
		}

		public override void DidBeginEditing (NSNotification notification)
		{
			base.DidBeginEditing (notification);
		}

		class FocusableComboBoxDelegate : NSTextDelegate
		{
			public override bool TextShouldEndEditing (NSText textObject)
			{
				return false;
			}

			public override bool TextShouldBeginEditing (NSText textObject)
			{
				return false;
			}
		}
	}
}