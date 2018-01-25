using System;

namespace Xamarin.PropertyEditing
{
	public interface ITypeInfo
	{
		IAssemblyInfo Assembly { get; }
		string NameSpace { get; }
		string Name { get; }
	}

	public class TypeInfo
		: ITypeInfo, IEquatable<TypeInfo>
	{
		public TypeInfo (IAssemblyInfo assembly, string nameSpace, string name)
		{
			if (assembly == null)
				throw new ArgumentNullException (nameof(assembly));
			if (nameSpace == null)
				throw new ArgumentNullException (nameof(nameSpace));
			if (name == null)
				throw new ArgumentNullException (nameof(name));

			Assembly = assembly;
			NameSpace = nameSpace;
			Name = name;
		}

		public IAssemblyInfo Assembly
		{
			get;
		}

		public string NameSpace
		{
			get;
		}

		public string Name
		{
			get;
		}

		public bool Equals (TypeInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return Assembly.Equals (other.Assembly) && String.Equals (NameSpace, other.NameSpace) && String.Equals (Name, other.Name);
		}

		public override bool Equals (object obj)
		{
			if (ReferenceEquals (null, obj))
				return false;
			if (ReferenceEquals (this, obj))
				return true;
			if (obj.GetType () != GetType ())
				return false;

			return Equals ((TypeInfo) obj);
		}

		public override int GetHashCode ()
		{
			unchecked {
				var hashCode = Assembly.GetHashCode ();
				hashCode = (hashCode * 397) ^ NameSpace.GetHashCode ();
				hashCode = (hashCode * 397) ^ Name.GetHashCode ();
				return hashCode;
			}
		}

		public static bool operator == (TypeInfo left, TypeInfo right)
		{
			return Equals (left, right);
		}

		public static bool operator != (TypeInfo left, TypeInfo right)
		{
			return !Equals (left, right);
		}
	}
}
