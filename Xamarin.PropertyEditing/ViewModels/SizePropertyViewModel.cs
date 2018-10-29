using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class SizePropertyViewModel
		: PropertyViewModel<CommonSize>
	{
		public SizePropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
		}

		public double Width
		{
			get { return Value.Width; }
			set
			{
				if (Value.Width == value)
					return;

				Value = new CommonSize (value, Value.Height);
			}
		}

		public double Height
		{
			get { return Value.Height; }
			set
			{
				if (Value.Height == value)
					return;

				Value = new CommonSize (Value.Width, value);
			}
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof(Width));
			OnPropertyChanged (nameof(Height));
		}
	}
}