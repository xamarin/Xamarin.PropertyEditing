using System.Collections.Generic;
using System.Linq;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Tests.MockPropertyInfo;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockControl
	{
		private OrderedDictionary<string, IPropertyInfo> PropertyInfos { get; }
			= new OrderedDictionary<string, IPropertyInfo> { };
		private OrderedDictionary<string, IEventInfo> EventInfos { get; }
			= new OrderedDictionary<string, IEventInfo> { };
		internal IDictionary<IPropertyInfo, object> Values { get; }
			= new Dictionary<IPropertyInfo, object> { };
		internal IDictionary<IEventInfo, string> EventHandlers { get; }
			= new Dictionary<IEventInfo, string> { };

		public ICollection<IPropertyInfo> Properties => PropertyInfos.Values;
		public ICollection<IEventInfo> Events => EventInfos.Values;

		public void AddProperty<T>(string name, string category = "", bool canWrite = true, bool flag = false)
		{
			var propertyInfo = typeof (T).IsEnum
				? new MockEnumPropertyInfo<T> (name, category, canWrite, flag)
				: new MockPropertyInfo<T> (name, category, canWrite);
			PropertyInfos.Add(name, propertyInfo);
			Values.Add (propertyInfo, default (T));
		}

		public void AddReadOnlyProperty<T> (string name)
		{
			AddProperty<T> (name, "", false);
		}

		public void AddEvent(string name)
		{
			var eventInfo = new MockEventInfo (name);
			EventInfos.Add (name, eventInfo);
			EventHandlers.Add (eventInfo, "");
		}

		public void AddEvents(params string[] names)
		{
			foreach (var name in names) {
				AddEvent (name);
			}
		}

		public IPropertyInfo GetPropertyInfo (string name)
			=> PropertyInfos[name];

		public class NotImplemented { }
	}
}
