using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class FocusableBooleanButton
		: FocusableButton
	{
		public override bool CanBecomeKeyView { get { return Enabled; } }

		public FocusableBooleanButton (IHostResourceProvider hostResources)
			: base (hostResources)
		{
			SetButtonType (NSButtonType.Switch);
		}
	}
}
