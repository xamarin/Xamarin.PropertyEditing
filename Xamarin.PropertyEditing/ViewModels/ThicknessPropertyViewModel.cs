using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ThicknessPropertyViewModel
		:PropertyViewModel<CommonThickness>
	{
		public ThicknessPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
		}

		public double Top
		{
			get { return Value.Top; }
			set {
				if (Value.Top == value)
					return;

				Value = new CommonThickness (Value.Bottom, Value.Left, Value.Right, value);
			}
		}

		public double Left
		{
			get { return Value.Left; }
			set {
				if (Value.Left == value)
					return;

				Value = new CommonThickness (Value.Bottom, value, Value.Right, Value.Top);
			}
		}

		public double Bottom
		{
			get { return Value.Bottom; }
			set {
				if (Value.Bottom == value)
					return;

				Value = new CommonThickness (value, Value.Left, Value.Right, Value.Top);
			}
		}

		public double Right
		{
			get { return Value.Right; }
			set {
				if (Value.Right == value)
					return;

				Value = new CommonThickness (Value.Bottom, Value.Left, value, Value.Top);
			}
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Top));
			OnPropertyChanged (nameof (Left));
			OnPropertyChanged (nameof (Bottom));
			OnPropertyChanged (nameof (Right));
		}
	}
}
