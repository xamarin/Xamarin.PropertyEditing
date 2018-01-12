using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockPropertyInfo<T> : IPropertyInfo, IPropertyConverter, IEquatable<MockPropertyInfo<T>>
	{
		public MockPropertyInfo (string name, string category = "",
			bool canWrite = true, IEnumerable<Type> converterTypes = null)
		{
			Name = name;
			Category = category;
			CanWrite = canWrite;
			if (converterTypes != null) {
				this.typeConverters = converterTypes
					.Where (type => type != null && typeof (TypeConverter).IsAssignableFrom (type))
					.Select (type => (TypeConverter)Activator.CreateInstance (type))
					.ToArray ();
			}
		}

		public string Name { get; }
		public virtual Type Type => typeof (T);
		public string Category { get; }
		public bool CanWrite { get; }
		public virtual ValueSources ValueSources => ValueSources.Local | ValueSources.Default;
		static readonly PropertyVariation[] EmptyVariations = new PropertyVariation[0];
		public virtual IReadOnlyList<PropertyVariation> Variations => EmptyVariations;
		static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		public virtual IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;

		public virtual bool TryConvert<TFrom> (TFrom fromValue, Type toType, out object toValue)
		{
			toValue = null;
			if (this.typeConverters != null) {
				foreach (TypeConverter converter in this.typeConverters) {
					if (converter.CanConvertTo (toType)) {
						toValue = converter.ConvertTo (fromValue, toType);
						return true;
					}
				}
			}

			if (toType == typeof (string)) {
				toValue = fromValue?.ToString ();
				return true;
			}

			try {
				toValue = Convert.ChangeType (fromValue, toType);
				return true;
			} catch {

			}

			return false;
		}

		public bool Equals (MockPropertyInfo<T> other)
		{
			if (other is null)
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return Name == other.Name
				&& Category == other.Category
				&& CanWrite == other.CanWrite;
		}

		public override bool Equals (object obj)
		{
			if (obj is null)
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (GetType () != obj.GetType ())
				return false;

			return Equals ((MockPropertyInfo<T>)obj);
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861411795;
			unchecked {
				if (Name != null)
					hashCode = hashCode * -1521134295 + Name.GetHashCode ();
				if (Category != null)
					hashCode = hashCode * -1521134295 + Category.GetHashCode ();
				hashCode = hashCode * -1521134295 + CanWrite.GetHashCode ();
			}
			return hashCode;
		}

		readonly IReadOnlyList<TypeConverter> typeConverters;
	}

	public class MockComplexPropertyInfo<T> : MockPropertyInfo<T>, IComplexPropertyInfo
	{
		public MockComplexPropertyInfo (string name, string category = "",
			bool canWrite = true, IEnumerable<Type> converterTypes = null) : base (name, category, canWrite, converterTypes)
		{
			this.subProperties = new List<ISubPropertyInfo> ();
		}

		public IReadOnlyCollection<ISubPropertyInfo> Properties => this.subProperties.ToArray ();

		public MockSubPropertyInfo<V> AddSubProperty<V> (string name, string category = "",
			bool canWrite = true, IEnumerable<Type> converterTypes = null)
		{
			var subProperty = new MockSubPropertyInfo<V> (this, name, category, canWrite, converterTypes);
			this.subProperties.Add (subProperty);
			return subProperty;
		}

		readonly List<ISubPropertyInfo> subProperties;
	}

	public class MockSubPropertyInfo<T> : MockPropertyInfo<T>, ISubPropertyInfo
	{
		public MockSubPropertyInfo (IComplexPropertyInfo parentProperty, string name, string category = "", bool canWrite = true, IEnumerable<Type> converterTypes = null) : base (name, category, canWrite, converterTypes)
		{
			ParentProperty = parentProperty;
		}

		public IComplexPropertyInfo ParentProperty { get; }
	}
}
