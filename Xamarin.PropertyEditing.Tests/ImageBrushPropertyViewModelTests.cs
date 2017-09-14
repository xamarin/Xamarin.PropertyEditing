using System;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	internal class ImageBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			var imageSource = rand.NextFilename (".gif");
			var alignmentX = rand.Next<CommonAlignmentX> ();
			var alignmentY = rand.Next<CommonAlignmentY> ();
			var stretch = rand.Next<CommonStretch> ();
			var tileMode = rand.Next<CommonTileMode> ();
			var viewBox = new CommonRectangle (
				rand.NextDouble(),
				rand.NextDouble(),
				rand.NextDouble(),
				rand.NextDouble()
			);
			var viewBoxUnit = rand.Next<CommonBrushMappingMode> ();
			var viewPort = new CommonRectangle (
				rand.NextDouble (),
				rand.NextDouble (),
				rand.NextDouble (),
				rand.NextDouble ()
			);
			var viewPortUnit = rand.Next<CommonBrushMappingMode> ();
			var opacity = rand.NextDouble ();

			return new CommonImageBrush (
				imageSource,
				alignmentX, alignmentY,
				stretch, tileMode,
				viewBox, viewBoxUnit,
				viewPort, viewPortUnit,
				opacity);
		}
	}
}
