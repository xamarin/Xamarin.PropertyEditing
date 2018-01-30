using System;

namespace Xamarin.PropertyEditing
{
	public sealed class ResourceSource
		: IEquatable<ResourceSource>
	{
		public ResourceSource (string name, bool isLocal)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Name = name;
			IsLocal = isLocal;
		}

		public string Name
		{
			get;
		}

		/// <summary>
		/// Gets whether the source is local/relative to a target object.
		/// </summary>
		public bool IsLocal
		{
			get;
		}

		public bool Equals (ResourceSource other)
		{
			if (ReferenceEquals (other, null))
				return false;
			if (ReferenceEquals (other, this))
				return true;

			return IsLocal == other.IsLocal && Name == other.Name;
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
				hashCode = (hashCode * 397) ^ IsLocal.GetHashCode();
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
