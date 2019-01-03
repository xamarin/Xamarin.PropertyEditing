using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ComponentSpinEditor
		: NumericSpinEditor
	{
		public ComponentSpinEditor (IHostResourceProvider hostResources, ChannelEditor component)
			: base (hostResources)
		{
			ComponentEditor = component;
			MinimumValue = component.MinimumValue;
			MaximumValue = component.MaximumValue;
			IncrementValue = component.IncrementValue;
			Digits = 3;
			FocusedFormat = component.FocusedFormat;
			DisplayFormat = component.DisplayFormat;
			if (Formatter is NSNumberFormatter numberFormatter && DisplayFormat != null) {
				numberFormatter.PositiveFormat = DisplayFormat;
			}
		}

		public ChannelEditor ComponentEditor { get; }
	}
}
