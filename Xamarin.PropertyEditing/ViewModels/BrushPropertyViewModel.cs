using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Properties;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class BrushPropertyViewModel : PropertyViewModel<CommonBrush>
	{
		public BrushPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors,
		                               IEnumerable<CommonBrushType> allowedBrushTypes = null)
			: base (platform, property, editors)
		{
			if (property.Type.IsAssignableFrom (typeof (CommonSolidBrush))
				|| property.Type.IsAssignableFrom (typeof (CommonColor))) {
				Solid = new SolidBrushViewModel (this,
					property is IColorSpaced colorSpacedPropertyInfo ? colorSpacedPropertyInfo.ColorSpaces :  null);
				if (platform.SupportsMaterialDesign) {
					MaterialDesign = new MaterialDesignColorViewModel (this);
				}
			}

			allowedBrushTypes = allowedBrushTypes ?? new[] {
				CommonBrushType.NoBrush, CommonBrushType.Solid, CommonBrushType.MaterialDesign,
				CommonBrushType.Gradient, CommonBrushType.Tile, CommonBrushType.Resource
			};

			// TODO: we actually need to localize this for platforms really, "brush" doesn't make sense for some
			var types = new OrderedDictionary<string, CommonBrushType> ();

			if (allowedBrushTypes.Contains (CommonBrushType.NoBrush)) types.Add (Resources.NoBrush, CommonBrushType.NoBrush);
			if (allowedBrushTypes.Contains (CommonBrushType.Solid)) types.Add (Resources.SolidBrush, CommonBrushType.Solid);
			if (allowedBrushTypes.Contains (CommonBrushType.Resource)) types.Add (Resources.ResourceBrush, CommonBrushType.Resource);

			if (platform.SupportsMaterialDesign && allowedBrushTypes.Contains (CommonBrushType.MaterialDesign)) {
				types.Insert (2, Resources.MaterialDesignColorBrush, CommonBrushType.MaterialDesign);
			}

			BrushTypes = types;
			RequestCurrentValueUpdate ();
		}

		public IReadOnlyDictionary<string, CommonBrushType> BrushTypes
		{
			get;
		}

		public CommonBrushType SelectedBrushType
		{
			get { return this.selectedBrushType; }
			set {
				if (this.selectedBrushType == value)
					return;

				this.selectedBrushType = value;
				SetBrushType (value);
				OnPropertyChanged();
			}
		}

		public SolidBrushViewModel Solid { get; }
		public MaterialDesignColorViewModel MaterialDesign { get; }

		public ResourceSelectorViewModel ResourceSelector
		{
			get {
				if (this.resourceSelector != null)
					return this.resourceSelector;

				if (TargetPlatform.ResourceProvider == null || Editors == null)
					return null;

				return this.resourceSelector = new ResourceSelectorViewModel (TargetPlatform.ResourceProvider, Editors.Select (ed => ed.Target), Property);
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

		public void ResetResourceSelector ()
		{
			this.resourceSelector = null;
			OnPropertyChanged (nameof (ResourceSelector));
		}

		protected override async Task UpdateCurrentValueAsync ()
		{
			if (BrushTypes == null)
				return;

			await base.UpdateCurrentValueAsync ();

			if (ValueSource == ValueSource.Resource) {
				this.selectedBrushType = CommonBrushType.Resource;
			} else if (Value == null) {
				this.selectedBrushType = CommonBrushType.NoBrush;
			} else if (MaterialDesign != null && (MaterialDesign.NormalColor.HasValue || MaterialDesign.AccentColor.HasValue)) {
				if (this.selectedBrushType != CommonBrushType.Solid)
					this.selectedBrushType = CommonBrushType.MaterialDesign;
			} else {
				this.selectedBrushType = CommonBrushType.Solid;
			}

			OnPropertyChanged (nameof (SelectedBrushType));
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();
			OnPropertyChanged (nameof (Opacity));
		}

		protected override void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			base.OnEditorsChanged (sender, e);
			ResetResourceSelector ();
		}

		protected override void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged (propertyName);
			if (propertyName == nameof (TargetPlatform)) {
				ResetResourceSelector ();
			}
		}

		private ResourceSelectorViewModel resourceSelector;
		private CommonBrushType selectedBrushType;

		private void StorePreviousBrush ()
		{
			if (Value is CommonSolidBrush solid)
				Solid.PreviousSolidBrush = solid;
		}

		private void SetBrushType (CommonBrushType type)
		{
			StorePreviousBrush();

			switch (type) {
				case CommonBrushType.MaterialDesign:
					MaterialDesign.SetToClosest ();
					break;

				case CommonBrushType.NoBrush:
					Value = null;
					break;

				case CommonBrushType.Solid:
					Value = Solid?.PreviousSolidBrush ?? new CommonSolidBrush (CommonColor.Black);
					Solid?.CommitLastColor ();
					Solid?.CommitHue ();
					break;
			}
		}

		public static CommonBrush GetCommonBrushForResource (Resource resource)
		{
			switch (resource) {
				case Resource<CommonColor> colour:
					return new CommonSolidBrush (colour.Value);

				case Resource<CommonGradientBrush> gradient:
					return gradient.Value;

				case Resource<CommonSolidBrush> solid:
					return solid.Value;
				default:
					return null;
			}
		}
	}
}
