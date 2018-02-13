using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class BrushPropertyViewModel : PropertyViewModel<CommonBrush>
	{
		public BrushPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			if (property.Type.IsAssignableFrom (typeof (CommonSolidBrush))) {
				Solid = new SolidBrushViewModel (this,
					property is IColorSpaced colorSpacedPropertyInfo ? colorSpacedPropertyInfo.ColorSpaces :  null);
				if (platform.SupportsMaterialDesign) {
					MaterialDesign = new MaterialDesignColorViewModel (this);
				}
			}
		}

		public SolidBrushViewModel Solid { get; }
		public MaterialDesignColorViewModel MaterialDesign { get; }

		// TODO: make this its own property view model so we can edit bindings, set to resources, etc.
		public double Opacity {
			get => Value == null ? 1.0 : Value.Opacity;
			set {
				if (Value is null) return;
				if (Value is CommonSolidBrush solid) {
					Value = new CommonSolidBrush (solid.Color, solid.ColorSpace, value);
				} else if (Value is CommonImageBrush img) {
					Value = new CommonImageBrush (
						img.ImageSource, img.AlignmentX, img.AlignmentY, img.Stretch, img.TileMode,
						img.ViewBox, img.ViewBoxUnits, img.ViewPort, img.ViewPortUnits, value);
				} else if (Value is CommonLinearGradientBrush linear) {
					Value = new CommonLinearGradientBrush (
						linear.StartPoint, linear.EndPoint, linear.GradientStops,
						linear.ColorInterpolationMode, linear.MappingMode, linear.SpreadMethod, value);
				} else if (Value is CommonRadialGradientBrush radial) {
					Value = new CommonRadialGradientBrush (
						radial.Center, radial.GradientOrigin, radial.RadiusX, radial.RadiusY,
						radial.GradientStops, radial.ColorInterpolationMode, radial.MappingMode,
						radial.SpreadMethod, value);
				} else {
					throw new InvalidOperationException ("Value is an unsupported brush type.");
				}
				OnPropertyChanged ();
			}
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Opacity));
		}
	}
}
