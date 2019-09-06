using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class RectanglePropertyViewModel
		: PropertyViewModel<CommonRectangle>
	{
		public RectanglePropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
			this.origin = property as IOrigin;
		}

		public double X {
			get { return Value.X; }
			set {
				if (Value.X == value)
					return;

				Value = new CommonRectangle (value, Value.Y, Value.Width, Value.Height, Value.Origin);
			}
		}

		public double Y {
			get { return Value.Y; }
			set {
				if (Value.Y == value)
					return;

				Value = new CommonRectangle (Value.X, value, Value.Width, Value.Height, Value.Origin);
			}
		}

		public double Width {
			get { return Value.Width; }
			set {
				if (Value.Width == value)
					return;

				Value = new CommonRectangle (Value.X, Value.Y, value, Value.Height, Value.Origin);
			}
		}

		public double Height {
			get { return Value.Height; }
			set {
				if (Value.Height == value)
					return;

				Value = new CommonRectangle (Value.X, Value.Y, Value.Width, value, Value.Origin);
			}
		}

		public CommonOrigin? Origin {
			get { return Value.Origin; }
			set {

				if (Value.Origin.HasValue && Value.Origin.Value.Equals(value))
					return;

				Value = new CommonRectangle (Value.X, Value.Y, Value.Width, Value.Height, value);
			}
		}

		private readonly IOrigin origin;

		public bool HasOrigin => this.origin != null;

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (X));
			OnPropertyChanged (nameof (Y));
			OnPropertyChanged (nameof (Width));
			OnPropertyChanged (nameof (Height));

			if (HasOrigin)
				OnPropertyChanged (nameof (Origin));
		}
	}
}
