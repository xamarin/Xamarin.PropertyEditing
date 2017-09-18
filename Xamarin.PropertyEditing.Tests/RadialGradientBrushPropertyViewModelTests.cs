using System;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	internal class RadialGradientBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			var center = new CommonPoint (
				rand.NextDouble(),
				rand.NextDouble()
			);
			var gradientOrigin = new CommonPoint (
				rand.NextDouble (),
				rand.NextDouble ()
			);
			var radiusX = rand.NextDouble ();
			var radiusY = rand.NextDouble ();
			var stops = new[] {
				new CommonGradientStop(rand.NextColor(), rand.NextDouble()),
				new CommonGradientStop(rand.NextColor(), rand.NextDouble()),
				new CommonGradientStop(rand.NextColor(), rand.NextDouble())
			};
			var colorInterpolationMode = rand.Next<CommonColorInterpolationMode> ();
			var mappingMode = rand.Next<CommonBrushMappingMode> ();
			var spreadMethod = rand.Next<CommonGradientSpreadMethod> ();
			var opacity = rand.NextDouble ();

			return new CommonRadialGradientBrush (
				center, gradientOrigin,
				radiusX, radiusY,
				stops,
				colorInterpolationMode,
				mappingMode,
				spreadMethod,
				opacity);
		}
	}
}
