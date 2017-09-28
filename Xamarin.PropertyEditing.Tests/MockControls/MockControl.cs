using System;
using System.Collections.Generic;
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

		public void AddProperty<T> (string name, string category = "",
			bool canWrite = true, bool flag = false,
			IEnumerable<Type> converterTypes = null)
		{
			var propertyInfo = typeof (T).IsEnum
				? new MockEnumPropertyInfo<T> (name, category, canWrite, flag, converterTypes)
				: new MockPropertyInfo<T> (name, category, canWrite, converterTypes);
			PropertyInfos.Add (name, propertyInfo);
			Values.Add (propertyInfo, new ValueInfo<T> {
				Value = default (T),
				Source = ValueSource.Local
			});
		}

		public void AddReadOnlyProperty<T> (string name, string category = "")
		{
			AddProperty<T> (name, category, false);
		}

		public void AddEvent (string name)
		{
			var eventInfo = new MockEventInfo (name);
			EventInfos.Add (name, eventInfo);
			EventHandlers.Add (eventInfo, "");
		}

		public void AddEvents (params string[] names)
		{
			foreach (var name in names) {
				AddEvent (name);
			}
		}

		public IPropertyInfo GetPropertyInfo (string name)
			=> PropertyInfos[name];

		public T GetValue<T> (string name) => GetValue<T> (PropertyInfos[name]);

		public T GetValue<T> (IPropertyInfo info)
		{
			var valueInfo = Values[info] as ValueInfo<T>;
			if (valueInfo != null)
				return valueInfo.Value;
			return (T)Convert.ChangeType (((IHasValue)Values[info]).Value, typeof (T));
		}

		public void SetValue<T> (string name, T value)
		{
			SetValue(PropertyInfos[name], value);
		}

		public void SetValue<T> (IPropertyInfo info, T value)
		{
			Values[info] = new ValueInfo<T> {
				Value = value,
				Source = ValueSource.Local
			};
		}

		public class NotImplemented { }
	}
}
