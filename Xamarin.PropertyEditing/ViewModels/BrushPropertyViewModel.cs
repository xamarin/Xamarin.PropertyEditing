using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

		private ResourceSelectorViewModel resourceSelector;
		public ResourceSelectorViewModel ResourceSelector
		{
			get => resourceSelector;
			private set {
				resourceSelector = value;
				OnPropertyChanged ();
			}
		}

		// TODO: make this its own property view model so we can edit bindings, set to resources, etc.
		public double Opacity {
			get => Value == null ? 1.0 : Value.Opacity;
			set {
				switch (Value) {
				case CommonBrush brush when brush == null:
					return;
				case CommonSolidBrush solid:
					Value = new CommonSolidBrush (solid.Color, solid.ColorSpace, value);
					break;
				case CommonImageBrush img:
					Value = new CommonImageBrush (
						img.ImageSource, img.AlignmentX, img.AlignmentY, img.Stretch, img.TileMode,
						img.ViewBox, img.ViewBoxUnits, img.ViewPort, img.ViewPortUnits, value);
					break;
				case CommonLinearGradientBrush linear:
					Value = new CommonLinearGradientBrush (
						linear.StartPoint, linear.EndPoint, linear.GradientStops,
						linear.ColorInterpolationMode, linear.MappingMode, linear.SpreadMethod, value);
					break;
				case CommonRadialGradientBrush radial:
					Value = new CommonRadialGradientBrush (
						radial.Center, radial.GradientOrigin, radial.RadiusX, radial.RadiusY,
						radial.GradientStops, radial.ColorInterpolationMode, radial.MappingMode,
						radial.SpreadMethod, value);
					break;
				default:
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

		protected override void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged (propertyName);
			if (propertyName == nameof (PropertyViewModel.ResourceProvider)) {
				ResourceSelector = (ResourceProvider != null) ? new ResourceSelectorViewModel (ResourceProvider, Editors.Select (ed => ed.Target), Property) : null;
			}
		}
	}
}
