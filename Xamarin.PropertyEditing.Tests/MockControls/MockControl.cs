using System;
using System.Collections.Generic;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Tests.MockPropertyInfo;

namespace Xamarin.PropertyEditing.Tests.MockControls
{
	public class MockControl
	{
		public IReadOnlyDictionary<string, IPropertyInfo> Properties => this.properties;

		public IReadOnlyDictionary<string, IEventInfo> Events => this.events;

		public IPropertyInfo AddProperty<T> (string name, string category = null,
			bool canWrite = true, bool flag = false,
			IEnumerable<Type> converterTypes = null,
			string description = null, bool constrained = true, ValueSources valueSources = ValueSources.Local | ValueSources.Default | ValueSources.Binding,
			IReadOnlyList<InputMode> inputModes = null, PropertyVariationOption[] options = null, bool isUncommon = false, ITypeInfo realType = null, bool ignoreEnum = false)
		{
			IPropertyInfo propertyInfo;
			if (typeof(T).IsEnum && !ignoreEnum) {
				var underlyingType = typeof (T).GetEnumUnderlyingType ();
				var enumPropertyInfoType = typeof (MockEnumPropertyInfo<,>)
					.MakeGenericType (underlyingType, typeof (T));
				propertyInfo = (IPropertyInfo)Activator.CreateInstance (enumPropertyInfoType, name, description, category, canWrite, flag, converterTypes, constrained, options);
			} else if (inputModes != null) {
				propertyInfo = new MockPropertyInfoWithInputTypes<T> (name, inputModes, description, category, canWrite, converterTypes, valueSources, options);
			} else {
				propertyInfo = new MockPropertyInfo<T> (name, description, category, canWrite, converterTypes, valueSources, options, isUncommon, realType);
			}

			AddProperty<T> (propertyInfo);
			return propertyInfo;
		}

		public void AddProperty<T> (IPropertyInfo propertyInfo)
		{
			this.properties.Add (propertyInfo.Name, propertyInfo);
		}

		public void AddReadOnlyProperty<T> (string name, string category = null)
		{
			AddProperty<T> (name, category, false);
		}

		public void AddEvent (string name)
		{
			var eventInfo = new MockEventInfo (name);
			this.events.Add (name, eventInfo);
		}

		public void AddEvents (params string[] names)
		{
			foreach (var name in names) {
				AddEvent (name);
			}
		}

		public class NotImplemented { }

		private readonly OrderedDictionary<string, IEventInfo> events = new OrderedDictionary<string, IEventInfo> ();
		private readonly OrderedDictionary<string, IPropertyInfo> properties = new OrderedDictionary<string, IPropertyInfo> ();
	}
}
