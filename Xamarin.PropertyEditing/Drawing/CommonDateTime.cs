using System;

namespace Xamarin.PropertyEditing.Drawing
{
	[Serializable]
	public struct CommonDateTime : IEquatable<CommonDateTime>
	{
		private readonly DateTime dateTime;

		public int Year { get => this.dateTime.Year; }
		public int Month { get => this.dateTime.Month; }
		public int Day { get => this.dateTime.Day; }

		public int Hour { get => this.dateTime.Hour; }
		public int Minute { get => this.dateTime.Minute; }
		public int Second { get => this.dateTime.Second; }

		public long Ticks { get { return this.dateTime.Ticks; } }

		public CommonDateTime (int year = 0, int month = 0, int day = 0, int hour = 0, int minute = 0, int second = 0)
		{
			this.dateTime = new DateTime (year, month, day, hour, minute, second);
		}

		public CommonDateTime (long ticks)
		{
			this.dateTime = new DateTime (ticks);
		}

		public override bool Equals (object obj)
		{
			return obj is CommonPoint && Equals ((CommonPoint)obj);
		}

		public bool Equals (CommonDateTime other)
		{
			return this.Year == other.Year
				&& this.Month == other.Month
				&& this.Day == other.Day
				&& this.Hour == other.Hour
				&& this.Minute == other.Minute
				&& this.Second == other.Second;
		}

		public override int GetHashCode ()
		{
			var hashCode = 1861411796;
			unchecked {
				hashCode = hashCode * -1521134296 + Year.GetHashCode ();
				hashCode = hashCode * -1521134296 + Month.GetHashCode ();
				hashCode = hashCode * -1521134296 + Day.GetHashCode ();
				hashCode = hashCode * -1521134296 + Hour.GetHashCode ();
				hashCode = hashCode * -1521134296 + Minute.GetHashCode ();
				hashCode = hashCode * -1521134296 + Second.GetHashCode ();
			}
			return hashCode;
		}
	}
}
