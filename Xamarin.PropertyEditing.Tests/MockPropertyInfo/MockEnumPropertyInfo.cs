using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockEnumPropertyInfo<T>
		: MockPropertyInfo<T>, IHavePredefinedValues<T>
	{
		public MockEnumPropertyInfo (string name, string category = "", bool canWrite = true, bool flag = false, IEnumerable<Type> converterTypes = null)
			: base (name, category, canWrite, converterTypes)
		{
			var names = Enum.GetNames (typeof (T));
			Values = (T[])Enum.GetValues (typeof (T));

			LongValues = new ulong[names.Length];

			var predefinedValues = new Dictionary<string, T> (names.Length);
			for (var i = 0; i < names.Length; i++) {
				predefinedValues.Add (names[i], Values[i]);
				LongValues[i] = Convert.ToUInt64(Values[i]);
			}

			PredefinedValues = predefinedValues;

			IsValueCombinable = flag;
		}

		public override Type Type => Enum.GetUnderlyingType (typeof (T));

		public bool IsConstrainedToPredefined => true;

		public bool IsValueCombinable { get; }

		public IReadOnlyDictionary<string, T> PredefinedValues { get; }

		private T[] Values;
		private ulong[] LongValues;

		public override TValue GetValue<TValue> (MockControl target)
		{
			if (typeof(IEnumerable).IsAssignableFrom(typeof (TValue))) {
				var realValue = Convert.ToUInt64 (target.GetValue<T> (this));

				var values = new List<T> ();
				for (var i = 0; i < Values.Length; i++) {
					if ((LongValues[i] & realValue) != 0)
						values.Add (Values[i]);
				}

				return (TValue)(object)(values.ToArray());
			}

			return target.GetValue<TValue> (this);
		}

		public override void SetValue<TValue> (MockControl target, TValue value)
		{
			var values = value as IEnumerable;
			if (values != null) {
				if (!IsValueCombinable)
					throw new ArgumentException ("Can't set a combined value on a non-combinable type", nameof (value));

				var realValue = Convert.ToUInt64 (default (T));
				foreach (var val in values) {
					realValue |= Convert.ToUInt64 (val);
				}

				target.SetValue (this, (T)Enum.ToObject (typeof (T), realValue));
			}
			else {
				target.SetValue (this, (T)Enum.ToObject (typeof (T), value));
			}
		}
	}
}
