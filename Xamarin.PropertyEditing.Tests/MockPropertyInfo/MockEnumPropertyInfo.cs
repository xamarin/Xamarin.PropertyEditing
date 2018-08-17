using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	/// <summary>
	/// Information about an enum or flag type property.
	/// </summary>
	/// <typeparam name="TUnderlying">The underlying type for the enum or flag.</typeparam>
	public class MockEnumPropertyInfo<TUnderlying, TEnum>
		: MockPropertyInfo<TEnum>, IHavePredefinedValues<TUnderlying>
		where TUnderlying : struct
		where TEnum : struct
	{
		public MockEnumPropertyInfo (string name, string description = null, string category = "", bool canWrite = true, bool flag = false, IEnumerable<Type> converterTypes = null, bool constrained = true, PropertyVariation[] variations = null)
			: base (name, description, category, canWrite, converterTypes, variations: variations)
		{
			var names = Enum.GetNames (typeof (TEnum));
			Values = Enum.GetValues (typeof (TEnum)).Cast<TUnderlying>().ToArray();

			LongValues = new ulong[names.Length];

			unchecked {
				var predefinedValues = new Dictionary<string, TUnderlying> (names.Length);
				for (var i = 0; i < names.Length; i++) {
					predefinedValues.Add (names[i], Values[i]);
					LongValues[i] = Convert.ToUInt64 (Values[i]);
				}

				PredefinedValues = predefinedValues;
			}

			IsConstrainedToPredefined = constrained;
			IsValueCombinable = flag;
		}

		public override Type Type => typeof (TEnum);

		public bool IsConstrainedToPredefined { get; }

		public bool IsValueCombinable { get; }

		public IReadOnlyDictionary<string, TUnderlying> PredefinedValues { get; }

		public override bool TryConvert<TFrom> (TFrom fromValue, Type toType, out object toValue)
		{
			var enumerationType = toType
				.GetInterfaces ()
				.FirstOrDefault (i => i.IsGenericType && i.GetGenericTypeDefinition () == typeof (IEnumerable<>));
			if (enumerationType != null) {
				// We're being asked for a list of values.
				var enumeratedType = enumerationType.GetGenericArguments ()[0];
				var listOfEnumeratedType = typeof (List<>).MakeGenericType (enumeratedType);
				// Build the list of values through a trip to uint64
				unchecked {
					var realValue = Convert.ToUInt64 (fromValue);
					var values = (IList)Activator.CreateInstance (listOfEnumeratedType);
					for (var i = 0; i < Values.Length; i++) {
						if ((LongValues[i] & realValue) != 0)
							values.Add (Convert.ChangeType(Values[i], enumeratedType));
					}

					toValue = values;
					return true;
				}
			} else if (fromValue is IEnumerable) {
				if (!IsValueCombinable)
					throw new ArgumentException ("Can't set a combined value on a non-combinable type", nameof(TFrom));

				unchecked {
					var realValue = Convert.ToUInt64 (default(TUnderlying));
					foreach (var val in (IEnumerable) fromValue) {
						realValue |= Convert.ToUInt64 (val);
					}

					if (toType.IsEnum)
						toValue = Enum.ToObject (toType, realValue);
					else
						toValue = realValue;
					return true;
				}
			} else if (toType.IsEnum) {
				toValue = Enum.ToObject (toType, fromValue);
				return true;
			}

			return base.TryConvert (fromValue, toType, out toValue);
		}

		private TUnderlying[] Values;
		private ulong[] LongValues;
	}
}
