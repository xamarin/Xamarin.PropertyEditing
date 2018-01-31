using System;

namespace Xamarin.PropertyEditing
{
	public class Resource<T>
		: Resource
	{
		public Resource (ResourceSource source, string name, T value)
			: base (source, name)
		{
			Value = value;
		}

		public T Value
		{
			get;
		}

		public override Type RepresentationType => typeof(T);
	}

	public class Resource
		: IEquatable<Resource>
	{
		public Resource (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			Name = name;
		}

		public Resource (ResourceSource source, string name)
			: this (name)
		{			
			if (source == null)
				throw new ArgumentNullException (nameof (source));

			Source = source;
		}

		/// <summary>
		/// Gets the source for this resource.
		/// </summary>
		/// <remarks>This may be <c>null</c> when the resource is a dynamic reference that is unlocatable.</remarks>
		public ResourceSource Source
		{
			get;
		}

		/// <remarks>This may be <c>null</c> when the resource is a dynamic reference that is unlocatable.</remarks>
		public virtual Type RepresentationType => null;

		public string Name
		{
			get;
		}

		public override bool Equals (object other)
		{
			// This does mean that a Resource<T> will match a Resource as long as its source and name are
			// the same, but for all intents and purposes this is correct. The identification of resources
			// is its source and name, the value is simply a helper for previews.
			var r = other as Resource;
			if (ReferenceEquals (null, r))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return r.Source == Source && r.Name == Name;
		}

		public bool Equals (Resource other)
		{
			return Equals ((object)other);
		}

		public override int GetHashCode ()
		{
			unchecked {
				int hashCode = Source.GetHashCode();
				hashCode = (hashCode * 397) ^ Name.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator == (Resource left, Resource right)
		{
			return Equals (left, right);
		}

		public static bool operator != (Resource left, Resource right)
		{
			return !Equals  (left, right);
		}
	}
}
