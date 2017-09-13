﻿using System;

namespace Xamarin.PropertyEditing
{
	public class PropertyVariation
		: IEquatable<PropertyVariation>
	{
		public PropertyVariation (string category, string name)
		{
			if (category == null)
				throw new ArgumentNullException (nameof (category));
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Category = category;
			Name = name;
		}

		public string Category
		{
			get;
		}

		public string Name
		{
			get;
		}

		public virtual bool Equals (PropertyVariation other)
		{
			if (ReferenceEquals (null, other))return false;
			if (ReferenceEquals (this, other)) return true;
			return String.Equals (Category, other.Category) && String.Equals (Name, other.Name);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj)) return false;
			if (ReferenceEquals (this, obj)) return true;
			return Equals (obj as PropertyVariation);
		}

		public override int GetHashCode ()
		{
			unchecked {
				return ((Category != null ? Category.GetHashCode () : 0) * 397) ^ (Name != null ? Name.GetHashCode () : 0);
			}
		}

		public static bool operator == (PropertyVariation left, PropertyVariation right)
		{
			return Equals (left, right);
		}

		public static bool operator != (PropertyVariation left, PropertyVariation right)
		{
			return !Equals (left, right);
		}
	}
}