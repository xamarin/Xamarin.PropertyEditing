using System;

namespace Xamarin.PropertyEditing
{
	public interface IResource
	{
		string Name { get; }
		Type RepresentationType { get; }
		ResourceSource Source { get; }
	}

	public interface IResource<out T>
	{
		T Value { get; }
	}

	public class Resource<T>
		: Resource, IResource<T>
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

		public override Type RepresentationType => typeof (T);

		public override string ToString () => $"{base.ToString ()}: {Value.ToString ()}";
	}

	public class Resource : IResource
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

		public override string ToString () => $"{Source.Name} - {Name}";
	}
}