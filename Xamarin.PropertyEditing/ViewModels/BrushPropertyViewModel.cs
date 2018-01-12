using System;
using System.Collections.Generic;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class BrushPropertyViewModel : PropertyViewModel<CommonBrush>
	{
		public BrushPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			if (property.Type.IsAssignableFrom (typeof (CommonSolidBrush))) {
				Solid = new SolidBrushViewModel (this,
					property is IColorSpaced colorSpacedPropertyInfo ? colorSpacedPropertyInfo.ColorSpaces :  null);
			}
		}

		public SolidBrushViewModel Solid { get; }

		public ValueInfo<double> Opacity
		{
			get => this.opacity ?? (
				this.opacity = new ValueInfo<double> {
					Source = ValueSource.Local,
					Value = Value == null ? 1.0 : Value.Opacity
				});
			set {
				this.opacity = value;
				if (Value is null || value.Source != ValueSource.Local) return;
				var opacity = value.Value;
				if (Value is CommonSolidBrush solid) {
					Value = new CommonSolidBrush (solid.Color, solid.ColorSpace, opacity);
				} else if (Value is CommonImageBrush img) {
					Value = new CommonImageBrush (
						img.ImageSource, img.AlignmentX, img.AlignmentY, img.Stretch, img.TileMode,
						img.ViewBox, img.ViewBoxUnits, img.ViewPort, img.ViewPortUnits, opacity);
				} else if (Value is CommonLinearGradientBrush linear) {
					Value = new CommonLinearGradientBrush (
						linear.StartPoint, linear.EndPoint, linear.GradientStops,
						linear.ColorInterpolationMode, linear.MappingMode, linear.SpreadMethod, opacity);
				} else if (Value is CommonRadialGradientBrush radial) {
					Value = new CommonRadialGradientBrush (
						radial.Center, radial.GradientOrigin, radial.RadiusX, radial.RadiusY,
						radial.GradientStops, radial.ColorInterpolationMode, radial.MappingMode,
						radial.SpreadMethod, opacity);
				} else {
					throw new InvalidOperationException ("Value is an unsupported brush type.");
				}
				OnPropertyChanged ();
			}
		}

		protected override void UpdateCurrentValue ()
		{
			base.UpdateCurrentValue ();
			OnPropertyChanged (nameof (Opacity));
		}

		ValueInfo<double> opacity;
	}
}
