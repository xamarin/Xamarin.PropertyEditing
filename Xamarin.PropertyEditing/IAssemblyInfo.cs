using System;

namespace Xamarin.PropertyEditing
{
	public interface IAssemblyInfo
	{
		string Name { get; }

		/// <summary>
		/// Gets whether the assembly is directly relevant to the current project.
		/// </summary>
		bool IsRelevant { get; }
	}

	public class AssemblyInfo
		: IAssemblyInfo, IEquatable<AssemblyInfo>
	{
		public AssemblyInfo (string name, bool isRelevant)
		{
			if (name == null)
				throw new ArgumentNullException (nameof(name));

			Name = name;
			IsRelevant = isRelevant;
		}

		public string Name
		{
			get;
		}

		public bool IsRelevant
		{
			get;
		}

		public bool Equals (AssemblyInfo other)
		{
			if (ReferenceEquals (null, other))
				return false;
			if (ReferenceEquals (this, other))
				return true;

			return String.Equals (Name, other.Name) && (IsRelevant == other.IsRelevant);
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
			unchecked {
				int hashCode = Name.GetHashCode ();
				hashCode = (hashCode * 397) ^ IsRelevant.GetHashCode ();
				return hashCode;
			}
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
