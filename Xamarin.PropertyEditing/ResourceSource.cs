using System;

namespace Xamarin.PropertyEditing
{
	public enum ResourceSourceType
	{
		System = 0,
		Application = 1,
		ResourceDictionary = 2,
		Document = 3
	}

	public class ResourceSource
		: IEquatable<ResourceSource>
	{
		public ResourceSource (string name, ResourceSourceType type)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Name = name;
			Type = type;
		}

		public string Name
		{
			get;
		}

		/// <summary>
		/// Gets the type of resource source.
		/// </summary>
		public ResourceSourceType Type
		{
			get;
		}

		public virtual bool Equals (ResourceSource other)
		{
			if (ReferenceEquals (other, null))
				return false;
			if (ReferenceEquals (other, this))
				return true;

			return Type == other.Type && Name == other.Name;
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (obj, null))
				return false;
			if (ReferenceEquals (obj, this))
				return true;
			if (obj.GetType() != GetType())
				return false;

			return Equals ((ResourceSource)obj);
		}

		public override int GetHashCode ()
		{
			unchecked {
				int hashCode = Name.GetHashCode();
				hashCode = (hashCode * 397) ^ Type.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator == (ResourceSource left, ResourceSource right)
		{
			return Equals (left, right);
		}

		public static bool operator != (ResourceSource left, ResourceSource right)
		{
			return !Equals (left, right);
		}
	}
}
