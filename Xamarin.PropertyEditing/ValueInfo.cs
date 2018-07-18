using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing
{
	public class ValueInfo<T> : IEquatable<ValueInfo<T>>
	{
		public T Value
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a descriptor of the value, such as a string value name or <see cref="ITypeInfo"/>.
		/// </summary>
		/// <seealso cref="IObjectEditor.SetValueAsync{T}"/>
		/// <seealso cref="IObjectEditor.GetValueAsync{T}"/>
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

		/// <summary>
		/// Gets or sets a descriptor for the source, such as a <see cref="Resource"/> or binding.
		/// </summary>
		/// <seealso cref="IObjectEditor.SetValueAsync{T}"/>
		/// <seealso cref="IObjectEditor.GetValueAsync{T}"/>
		public object SourceDescriptor
		{
			get;
			set;
		}

		public string CustomExpression
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

			return EqualityComparer<T>.Default.Equals (Value, other.Value) &&
					Equals (ValueDescriptor, other.ValueDescriptor) &&
					Source == other.Source &&
					Equals (SourceDescriptor, other.SourceDescriptor) &&
					CustomExpression == other.CustomExpression;
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
				hashCode = (hashCode * 397) ^ (SourceDescriptor?.GetHashCode () ?? 0);
				hashCode = (hashCode * 397) ^ (CustomExpression?.GetHashCode () ?? 0);
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