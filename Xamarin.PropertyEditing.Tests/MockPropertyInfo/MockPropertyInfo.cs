using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests.MockPropertyInfo
{
	public class MockPropertyInfoWithInputTypes<T>
		: MockPropertyInfo<T>, IHaveInputModes
	{
		public MockPropertyInfoWithInputTypes (string name, IReadOnlyList<InputMode> inputModes, string description = null, string category = null, bool canWrite = true, IEnumerable<Type> converterTypes = null, ValueSources valueSources = ValueSources.Default | ValueSources.Local, PropertyVariation[] variations = null)
			: base (name, description, category, canWrite, converterTypes, valueSources, variations)
		{
			InputModes = inputModes.ToArray ();
		}

		public IReadOnlyList<InputMode> InputModes
		{
			get;
		}
	}

	public class MockPropertyInfo<T> : IPropertyInfo, IPropertyConverter, IEquatable<MockPropertyInfo<T>>
	{
		public MockPropertyInfo (string name, string description = null, string category = null, bool canWrite = true, IEnumerable<Type> converterTypes = null, ValueSources valueSources = ValueSources.Local | ValueSources.Default, PropertyVariation[] variations = null)
		{
			Name = name;
			Description = description;
			Category = category;
			CanWrite = canWrite;
			ValueSources = valueSources;
			if (converterTypes != null) {
				this.typeConverters = converterTypes
					.Where (type => type != null && typeof (TypeConverter).IsAssignableFrom (type))
					.Select (type => (TypeConverter)Activator.CreateInstance (type))
					.ToArray();
			}

			if (typeof(T).IsValueType) {
				this.nullConverter = new NullableConverter (typeof(Nullable<>).MakeGenericType (typeof(T)));
			}

			Variations = variations ?? EmptyVariations;
		}

		public string Name { get; }
		public string Description { get; }
		public virtual Type Type => typeof (T);

		public virtual ITypeInfo RealType => typeof(T).ToTypeInfo ();

		public string Category { get; }
		public bool CanWrite { get; }
		public ValueSources ValueSources { get; }
		static readonly PropertyVariation[] EmptyVariations = new PropertyVariation[0];

		public virtual IReadOnlyList<PropertyVariation> Variations { get; }
		static readonly IAvailabilityConstraint[] EmptyConstraints = new IAvailabilityConstraint[0];
		public virtual IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => EmptyConstraints;

		public virtual bool TryConvert<TFrom> (TFrom fromValue, Type toType, out object toValue)
		{
			toValue = null;
			if (this.typeConverters != null) {
				foreach (var converter in this.typeConverters) {
					if (converter.CanConvertTo (toType)) {
						toValue = converter.ConvertTo (fromValue, toType);
						return true;
					}
				}
			}

			if (this.nullConverter != null && (fromValue != null  && this.nullConverter.CanConvertFrom (fromValue.GetType()) || this.nullConverter.CanConvertFrom (typeof(TFrom)))) {
				toValue = this.nullConverter.ConvertFrom (fromValue);
				return true;
			}

			if (toType == typeof(string)) {
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
				if (Name != null)
					hashCode = hashCode * -1521134295 + Name.GetHashCode ();
				if (Category != null)
					hashCode = hashCode * -1521134295 + Category.GetHashCode ();
				hashCode = hashCode * -1521134295 + CanWrite.GetHashCode ();
			}
			return hashCode;
		}

		private readonly IReadOnlyList<TypeConverter> typeConverters;
		private readonly NullableConverter nullConverter;
	}
}
