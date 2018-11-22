using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class DateTimePropertyViewModel
		: PropertyViewModel<CommonDateTime>
	{
		public int Year {
			get { return Value.Year; }
			set {
				if (Value.Year == value)
					return;

				Value = new CommonDateTime (value, Value.Month, Value.Day, Value.Hour, Value.Minute, Value.Second);
			}
		}

		public int Month {
			get { return Value.Month; }
			set {
				if (Value.Month == value)
					return;

				Value = new CommonDateTime (Value.Year, value, Value.Day, Value.Hour, Value.Minute, Value.Second);
			}
		}

		public int Day {
			get { return Value.Day; }
			set {
				if (Value.Day == value)
					return;

				Value = new CommonDateTime (Value.Year, Value.Month, value, Value.Hour, Value.Minute, Value.Second);
			}
		}

		public int Hour {
			get { return Value.Hour; }
			set {
				if (Value.Hour == value)
					return;

				Value = new CommonDateTime (Value.Year, Value.Month, Value.Day, value, Value.Minute, Value.Second);
			}
		}

		public int Minute {
			get { return Value.Minute; }
			set {
				if (Value.Minute == value)
					return;

				Value = new CommonDateTime (Value.Year, Value.Month, Value.Day, Value.Hour, value, Value.Second);
			}
		}

		public int Second {
			get { return Value.Minute; }
			set {
				if (Value.Minute == value)
					return;

				Value = new CommonDateTime (Value.Year, Value.Month, Value.Day, Value.Hour, Value.Minute, value);
			}
		}

		public DateTimePropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Year));
			OnPropertyChanged (nameof (Month));
			OnPropertyChanged (nameof (Day));
			OnPropertyChanged (nameof (Hour));
			OnPropertyChanged (nameof (Minute));
			OnPropertyChanged (nameof (Second));
		}
	}
}
