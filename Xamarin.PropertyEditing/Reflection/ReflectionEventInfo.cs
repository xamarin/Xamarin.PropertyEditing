using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Xamarin.PropertyEditing.Reflection
{
	public class ReflectionEventInfo
		: IEventInfo
	{
		public ReflectionEventInfo (EventInfo info)
		{
			if (info == null)
				throw new ArgumentNullException (nameof (info));

			this.info = info;
		}

		public string Name => this.info.Name;

		public IReadOnlyList<string> GetHandlers (object target)
		{
			if (target == null)
				return new string[0];

			Type targetType = target.GetType ();
			FieldInfo field = targetType.GetField ($"Event{Name}", BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
			Delegate d = field?.GetValue (target) as Delegate;
			if (d == null)
				return new string[0];

			return d.GetInvocationList ().Select (i => i.Method.Name).ToList ();
		}

		private EventInfo info;
	}
}
