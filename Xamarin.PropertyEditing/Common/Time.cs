using System;

namespace Xamarin.PropertyEditing.Common
{
	public class Time : IEquatable<Time>
	{
		private readonly DateTime dateTime;

		public DateTime DateTime {
			get {
				return this.dateTime;
			}
		}

		public Time (DateTime dateTime)
		{
			this.dateTime = dateTime;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if ((obj is Time d))
				return Equals (d);
			else
				return false;
		}

		public bool Equals (Time other)
		{
			if (other == null)
				return false;
			return this.dateTime.Equals (other.DateTime);
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861433795;
			unchecked {
				hashCode = hashCode * -1521134295 + this.dateTime.GetHashCode ();
			}
			return hashCode;
		}
		public override string ToString ()
		{
			return this.dateTime.ToLongTimeString ();
		}

		public static Time Parse(string value)
		{
			try {
				return new Time (DateTime.Parse (value));
			} catch (Exception) {
				return null;
			}
		}
	}
}