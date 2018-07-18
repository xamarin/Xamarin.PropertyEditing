namespace Xamarin.PropertyEditing.Mac
{
	internal class ComponentSpinEditor : NumericSpinEditor
	{
		public ComponentSpinEditor (ChannelEditor component)
		{
			ComponentEditor = component;
			MinimumValue = component.MinimumValue;
			MaximumValue = component.MaximumValue;
			IncrementValue = component.IncrementValue;
			Digits = 3;
		}

		public ChannelEditor ComponentEditor { get; }
	}
}
