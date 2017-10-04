using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public IPropertyInfo AddProperty<T> (string name, string category = "",
			bool canWrite = true, bool flag = false,
			IEnumerable<Type> converterTypes = null)
		{
			IPropertyInfo propertyInfo;
			if (typeof(T).IsEnum) {
				var underlyingType = typeof (T).GetEnumUnderlyingType ();
				var enumPropertyInfoType = typeof (MockEnumPropertyInfo<,>)
					.MakeGenericType (underlyingType, typeof (T));
				propertyInfo = (IPropertyInfo)Activator.CreateInstance (enumPropertyInfoType, name, category, canWrite, flag, converterTypes);
			}
			else {
				propertyInfo = new MockPropertyInfo<T> (name, category, canWrite, converterTypes);
			}
			return AddProperty<T>(propertyInfo);
		}

		public IPropertyInfo AddProperty<T>(IPropertyInfo propertyInfo)
		{
			PropertyInfos.Add (propertyInfo.Name, propertyInfo);
			Values.Add (propertyInfo, new ValueInfo<T> {
				Value = default (T),
				Source = ValueSource.Local
			});
			return propertyInfo;
		}

		public IPropertyInfo AddReadOnlyProperty<T> (string name, string category = "")
		{
			return AddProperty<T> (name, category, false);
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
			var infoObject = Values[info];
			var valueInfo = infoObject as ValueInfo<T>;
			if (valueInfo != null)
				return valueInfo.Value;
			return default(T);
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
