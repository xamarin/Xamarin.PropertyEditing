using System;
using System.Drawing;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal class SolidBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			CommonColor color = rand.NextColor();
			var colorSpace = rand.NextString ();
			var opacity = rand.NextDouble ();

			return new CommonSolidBrush (color, colorSpace, opacity);
		}

		static readonly string[] SampleColorSpaces = new[] {
			"genericRGBLinear", "genericCMYK"
		};

		[Test]
		public void ColorSpaces ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonSolidBrush));
			mockProperty.As<IColorSpaced>().SetupGet (pi => pi.ColorSpaces).Returns (SampleColorSpaces);
			var mockEditor = new Mock<IObjectEditor> ();

			var vm = new BrushPropertyViewModel(mockProperty.Object, new[] { mockEditor.Object });
			Assert.That (vm.Solid.ColorSpaces, new CollectionEquivalentConstraint (SampleColorSpaces));
		}
	}
}
