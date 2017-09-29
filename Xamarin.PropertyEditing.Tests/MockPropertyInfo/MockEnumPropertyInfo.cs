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
		public MockEnumPropertyInfo (string name, string category = "", bool canWrite = true, bool flag = false, IEnumerable<Type> converterTypes = null)
			: base (name, category, canWrite, converterTypes)
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

			IsValueCombinable = flag;
		}

		public override Type Type => typeof (TEnum);

		public bool IsConstrainedToPredefined => true;

		public bool IsValueCombinable { get; }

		public IReadOnlyDictionary<string, TUnderlying> PredefinedValues { get; }

		private TUnderlying[] Values;
		private ulong[] LongValues;

		/// <summary>
		/// Gets the value or list of values of the described property on the target, as an `TUnderlying`
		/// or as a `IEnumerable` of `TUnderlying`.
		/// </summary>
		/// <typeparam name="TValue">The result type desired, should be `TUnderlying` or an `IEnumerable` of `TUnderlying`.</typeparam>
		/// <param name="target">The control from which to get the value.</param>
		/// <returns>The value or list of values.</returns>
		public override TValue GetValue<TValue> (MockControl target)
		{
			var enumerationType = typeof (TValue)
				.GetInterfaces ()
				.FirstOrDefault (i => i.IsGenericType && i.GetGenericTypeDefinition () == typeof (IEnumerable<>));
			if (enumerationType != null) {
				// We're being asked for a list of values.
				var enumeratedType = enumerationType.GetGenericArguments ()[0];
				var listOfEnumeratedType = typeof (List<>).MakeGenericType (enumeratedType);
				// Build the list of values through a trip to uint64
				unchecked {
					var realValue = Convert.ToUInt64 (target.GetValue<TEnum> (this));
					var values = (IList)Activator.CreateInstance (listOfEnumeratedType);
					for (var i = 0; i < Values.Length; i++) {
						if ((LongValues[i] & realValue) != 0)
							values.Add (Convert.ChangeType(Values[i], enumeratedType));
					}

					return (TValue)values;
				}
			}
			else {
				// Get a single value.
				var value = target.GetValue<TEnum> (this);
				return (TValue)Convert.ChangeType (value, typeof (TValue));
			}
		}

		/// <summary>
		/// Sets a value or a list of values for the represented property on the target control.
		/// </summary>
		/// <typeparam name="TValue">An `IEnumerable` of the values to set, or the value to set.</typeparam>
		/// <param name="target">The object on which the value must be set.</param>
		/// <param name="value">The value to set.</param>
		public override void SetValue<TValue> (MockControl target, TValue value)
		{
			var values = value as IEnumerable;
			if (values != null) {
				if (!IsValueCombinable)
					throw new ArgumentException ("Can't set a combined value on a non-combinable type", nameof (value));

				unchecked {
					var realValue = Convert.ToUInt64 (default (TUnderlying));
					foreach (var val in values) {
						realValue |= Convert.ToUInt64 (val);
					}

					target.SetValue (this, (TEnum)Enum.ToObject (typeof (TEnum), realValue));
				}
			}
			else {
				target.SetValue (this, (TEnum)Enum.ToObject (typeof (TEnum), value));
			}
		}
	}
}
