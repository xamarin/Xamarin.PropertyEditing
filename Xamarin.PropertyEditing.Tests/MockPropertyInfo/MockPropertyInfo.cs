using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockPropertyInfo<T> : IPropertyInfo, IGetAndSet, IEquatable<MockPropertyInfo<T>>
	{
		public MockPropertyInfo (string name, string category = "", bool canWrite = true, IEnumerable<Type> converterTypes = null)
		{
			Name = name;
			Category = category;
			CanWrite = canWrite;
			if (converterTypes != null) {
				TypeConverters = converterTypes
					.Where (type => type != null && typeof (TypeConverter).IsAssignableFrom (type))
					.Select (type => (TypeConverter)Activator.CreateInstance (type))
					.ToArray();
			}
		}

		public string Name { get; }
		public virtual Type Type => typeof (T);
		public string Category { get; }
		public bool CanWrite { get; }
		public virtual ValueSources ValueSources => ValueSources.Local;
		static readonly PropertyVariation[] EmptyVariations = new PropertyVariation[0];
		public virtual IReadOnlyList<PropertyVariation> Variations => EmptyVariations;
		static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		public virtual IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;

		private IReadOnlyList<TypeConverter> TypeConverters;

		public TValue GetValue<TValue> (object target)
		{
			return GetValue<TValue> ((MockControl)target);
		}

		public void SetValue<TValue> (object target, TValue value)
		{
			SetValue ((MockControl)target, value);
		}

		public virtual TValue GetValue<TValue> (MockControl target)
		{
			object value = target.GetValue<T> (this);
			if (value is TValue)
				return (TValue)value;
			TValue converted;
			if (TryConvertToValue (value, out converted)) {
				return converted;
			}
			if (value == null)
				return default (TValue);
			return (TValue)(typeof (TValue) == typeof (string)
				? value.ToString ()
				: Convert.ChangeType (value, typeof (TValue)));
		}

		public virtual void SetValue<TValue> (MockControl target, TValue value)
		{
			object realValue = value;
			object converted;
			if (TryConvertFromValue (value, out converted)) {
				realValue = converted;
			}
			else if (realValue != null && !typeof (T).IsInstanceOfType (value)) {
				realValue = Convert.ChangeType (value, typeof (T));
			}

			target.SetValue (this, (T)realValue);
		}

		private bool TryConvertToValue<TValue> (object value, out TValue converted)
		{
			converted = default (TValue);

			foreach (var converter in TypeConverters) {
				if (converter.CanConvertTo (typeof (TValue))) {
					converted = (TValue)converter.ConvertTo (value, typeof (TValue));
					return true;
				}
			}

			return false;
		}

		private bool TryConvertFromValue<TValue> (TValue value, out object converted)
		{
			converted = null;

			if (TypeConverters == null)
				return false;

			foreach (var converter in TypeConverters) {
				if (converter.CanConvertFrom (typeof (TValue))) {
					converted = converter.ConvertFrom (value);
					return true;
				}
			}

			return false;
		}

		public bool Equals (MockPropertyInfo<T> other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return Name == other.Name
				&& Category == other.Category
				&& CanWrite == other.CanWrite;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (GetType() != obj.GetType ())
				return false;

			return Equals ((MockPropertyInfo<T>)obj);
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861411795;
			unchecked {
				hashCode = hashCode * -1521134295 + Name.GetHashCode ();
				hashCode = hashCode * -1521134295 + Category.GetHashCode ();
				hashCode = hashCode * -1521134295 + CanWrite.GetHashCode ();
			}
			return hashCode;
		}
	}
}
