using System;
using System.ComponentModel;
using System.Globalization;

namespace Xamarin.PropertyEditing.Drawing
{
	internal class CommonColorToCommonBrushConverter : TypeConverter
	{
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			=> typeof(CommonBrush) == destinationType ? true : base.CanConvertTo (context, destinationType);

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (typeof (CommonBrush).IsAssignableFrom (destinationType) && value is CommonColor color) {
				return new CommonSolidBrush (color);
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof (CommonColor) ? true : base.CanConvertFrom (context, sourceType);

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			=> value is CommonColor color ? new CommonSolidBrush(color) : base.ConvertFrom (context, culture, value);
	}
}