using System;

namespace Xamarin.PropertyEditing
{
	public interface IAssemblyInfo
	{
		string Name { get; }
	}

	public class AssemblyInfo
		: IAssemblyInfo, IEquatable<AssemblyInfo>
	{
		public AssemblyInfo (string name)
		{
			if (name == null)
				throw new ArgumentNullException (nameof(name));

			Name = name;
		}

		public string Name
		{
			get;
		}

		public bool Equals (AssemblyInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return string.Equals (Name, other.Name);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != GetType ())
				return false;

			return Equals ((AssemblyInfo) obj);
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public static bool operator == (AssemblyInfo left, AssemblyInfo right)
		{
			return Equals (left, right);
		}

		public static bool operator != (AssemblyInfo left, AssemblyInfo right)
		{
			return !Equals (left, right);
		}
	}
}
