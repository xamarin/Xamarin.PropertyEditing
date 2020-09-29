using System;

namespace Xamarin.PropertyEditing.Common
{
	public class Date : IEquatable<Date>
	{
		private readonly DateTime dateTime;

		public Date (DateTime dateTime)
		{
			this.dateTime = dateTime;
		}

		public DateTime DateTime {
			get{
				return this.dateTime;
			}
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if ((obj is Date d))
				return Equals (d);
			else
				return false;
		}

		public bool Equals (Date other)
		{
			if (other == null)
				return false;
			return this.dateTime.Equals (other);
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
			return this.dateTime.ToShortDateString ();
		}

		public static Date Parse (string value)
		{
			try {
				return new Date (DateTime.Parse (value));
			} catch (Exception) {
				return null;
			}
		}
	}
}