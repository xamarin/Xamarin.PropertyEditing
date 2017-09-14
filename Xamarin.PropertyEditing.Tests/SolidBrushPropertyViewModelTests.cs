using System;
using System.Drawing;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	internal class SolidBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			var color = rand.NextColor();
			var opacity = rand.NextDouble ();

			return new CommonSolidBrush (color, opacity);
		}
	}
}
