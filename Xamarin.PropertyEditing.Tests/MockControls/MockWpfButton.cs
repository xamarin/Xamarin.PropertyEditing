using System.Windows.Input;
using static Xamarin.PropertyEditing.Tests.MockPropertyInfo.MockPropertyCategories;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockWpfButton : MockWpfControl
	{
		public MockWpfButton() : base()
		{
			AddProperty<ClickMode> ("ClickMode", Behavior);
			AddProperty<NotImplemented> ("Command", Action);
			AddProperty<object> ("CommandParameter", Action);
			AddProperty<NotImplemented> ("CommandTarget", Action);
			AddProperty<object> ("Content", Content);
			AddProperty<string> ("ContentStringFormat", Content);
			AddProperty<NotImplemented> ("ContentTemplate", Content);
			AddProperty<NotImplemented> ("ContentTemplateSelector", Content);
			AddProperty<bool> ("HasContent", None, false);
			AddProperty<bool> ("IsCancel");
			AddProperty<bool> ("IsDefault");
			AddProperty<bool> ("IsDefaulted", None, false);
			AddProperty<bool> ("IsPressed", Appearance);

			AddEvent ("Click");
		}

		public enum ClickMode
		{
			Hover,
			Press,
			Release
		}
	}
}
