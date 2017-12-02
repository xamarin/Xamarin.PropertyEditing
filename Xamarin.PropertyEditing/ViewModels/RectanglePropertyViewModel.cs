using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class RectanglePropertyViewModel
		: PropertyViewModel<CommonRectangle>
	{
		public RectanglePropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
		}

		public double X {
			get { return Value.X; }
			set {
				if (Value.X == value)
					return;

				Value = new CommonRectangle (value, Value.Y, Value.Width, Value.Height);
			}
		}

		public double Y {
			get { return Value.Y; }
			set {
				if (Value.Y == value)
					return;

				Value = new CommonRectangle (Value.X, value, Value.Width, Value.Height);
			}
		}

		public double Width {
			get { return Value.Width; }
			set {
				if (Value.Width == value)
					return;

				Value = new CommonRectangle (Value.X, Value.Y, value, Value.Height);
			}
		}

		public double Height {
			get { return Value.Height; }
			set {
				if (Value.Height == value)
					return;

				Value = new CommonRectangle (Value.X, Value.Y, Value.Width, value);
			}
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (X));
			OnPropertyChanged (nameof (Y));
			OnPropertyChanged (nameof (Width));
			OnPropertyChanged (nameof (Height));
		}
	}
}
