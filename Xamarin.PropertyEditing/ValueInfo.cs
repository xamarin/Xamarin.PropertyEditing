using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	/// <remarks>
	/// <para>The <see cref="Source"/> and <see cref="ValueDescriptor"/> for the value must match for two ValueInfos to be considered equal.</para>
	/// </remarks>
	public class ValueInfo<T> : IEquatable<ValueInfo<T>>
	{
		public T Value
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a descriptor of the value source, such as a resource reference or binding description.
		/// </summary>
		public object ValueDescriptor
		{
			get;
			set;
		}

		public ValueSource Source
		{
			get;
			set;
		}

		public bool Equals (ValueInfo<T> other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return EqualityComparer<T>.Default.Equals (Value, other.Value) && Equals (ValueDescriptor, other.ValueDescriptor) && Source == other.Source;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != GetType ())
				return false;
			return Equals ((ValueInfo<T>) obj);
		}

		public override int GetHashCode ()
		{
			unchecked {
				var hashCode = EqualityComparer<T>.Default.GetHashCode (Value);
				hashCode = (hashCode * 397) ^ (ValueDescriptor?.GetHashCode () ?? 0);
				hashCode = (hashCode * 397) ^ (int) Source;
				return hashCode;
			}
		}

		public static bool operator == (ValueInfo<T> left, ValueInfo<T> right)
		{
			return Equals (left, right);
		}

		public static bool operator != (ValueInfo<T> left, ValueInfo<T> right)
		{
			return !Equals (left, right);
		}
	}
}