using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockEnumPropertyInfo<T>
		: MockPropertyInfo<T>, IHavePredefinedValues<T>
	{
		public MockEnumPropertyInfo (string name, string category = "", bool canWrite = true, bool flag = false)
			: base (name, category, canWrite)
		{
			var names = Enum.GetNames (typeof(T));
			var values = Enum.GetValues (typeof(T));

			var predefinedValues = new Dictionary<string, T> (names.Length);
			for (var i = 0; i < names.Length; i++) {
				predefinedValues.Add (names[i], (T)values.GetValue (i));
			}

			PredefinedValues = predefinedValues;

			IsValueCombinable = flag;
		}

		public bool IsConstrainedToPredefined => true;

		public bool IsValueCombinable { get; }

		public IReadOnlyDictionary<string, T> PredefinedValues { get; }
	}
}
