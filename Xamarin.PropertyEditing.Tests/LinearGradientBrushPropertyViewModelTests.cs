using System;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	internal class LinearGradientBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			var startPoint = new CommonPoint (
				rand.NextDouble(),
				rand.NextDouble()
			);
			var endPoint = new CommonPoint (
				rand.NextDouble (),
				rand.NextDouble ()
			);
			var stops = new[] {
				new CommonGradientStop(rand.NextColor(), rand.NextDouble()),
				new CommonGradientStop(rand.NextColor(), rand.NextDouble()),
				new CommonGradientStop(rand.NextColor(), rand.NextDouble())
			};
			var colorInterpolationMode = rand.Next<CommonColorInterpolationMode> ();
			var mappingMode = rand.Next<CommonBrushMappingMode> ();
			var spreadMethod = rand.Next<CommonGradientSpreadMethod> ();
			var opacity = rand.NextDouble ();

			return new CommonLinearGradientBrush (
				startPoint, endPoint,
				stops,
				colorInterpolationMode,
				mappingMode,
				spreadMethod,
				opacity);
		}
	}
}
