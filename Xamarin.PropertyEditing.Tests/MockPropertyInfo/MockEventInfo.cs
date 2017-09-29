namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockEventInfo : IEventInfo
	{
		public MockEventInfo(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
