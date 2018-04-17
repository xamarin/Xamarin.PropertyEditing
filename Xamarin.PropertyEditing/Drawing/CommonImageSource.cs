using System;

namespace Xamarin.PropertyEditing.Drawing
{
	[Serializable]
	public class CommonImageSource : IEquatable<CommonImageSource>
	{
		public Uri UriSource { get; set; }

		public override bool Equals (object obj)
		{
			if (obj == null) return false;
			if (!(obj is CommonImageSource)) return false;
			return base.Equals ((CommonImageSource)obj);
		}

		public bool Equals (CommonImageSource other)
		{
			return UriSource == other.UriSource;
		}

		public override int GetHashCode ()
		{
			var hashCode = 466501756;
			unchecked {
				if (UriSource != null) hashCode = hashCode * -1521134295 + UriSource.GetHashCode ();
			}
			return hashCode;
		}
	}
}
