using System;

namespace Xamarin.PropertyEditing.Common
{
	public class FilePath : IEquatable<FilePath>
	{
		public string Source { get; }

		public FilePath (string source)
		{
			if (source == null) {
				throw new ArgumentNullException (nameof (source));
			}
			Source = source;
		}

		public override string ToString ()
		{
			return Source;
		}

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is FilePath)) return false;
			return Equals ((FilePath)obj);
		}

		public bool Equals (FilePath other)
		{
			if (other == null) return false;
			return Source.Equals (other.Source, StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861433795;
			unchecked {
				hashCode = hashCode * -1521134295 + Source.GetHashCode ();
			}
			return hashCode;
		}
	}
}