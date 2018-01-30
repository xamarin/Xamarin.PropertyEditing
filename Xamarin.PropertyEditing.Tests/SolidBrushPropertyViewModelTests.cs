using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal class SolidBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		private static readonly string[] SampleColorSpaces = new[] {
			"genericRGBLinear", "genericCMYK"
		};

		[Test]
		public void ColorSpaces ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			Assert.That (vm.Solid.ColorSpaces, new CollectionEquivalentConstraint (SampleColorSpaces));
		}

		[Test]
		public void ValueChangesTriggerSolidPropertyChanges()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			var colorChanged = false;
			var colorSpaceChanged = false;
			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof(SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.ColorSpace):
					colorSpaceChanged = true;
					break;
				}
			};
			vm.Value = new CommonSolidBrush (new CommonColor ());
			Assert.IsTrue (colorChanged);
			Assert.IsTrue (colorSpaceChanged);
		}

		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			CommonColor color = rand.NextColor ();
			var colorSpace = rand.NextString ();
			var opacity = rand.NextDouble ();

			return new CommonSolidBrush (color, colorSpace, opacity);
		}

		private static BrushPropertyViewModel PrepareMockViewModel ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonSolidBrush));
			mockProperty.As<IColorSpaced> ().SetupGet (pi => pi.ColorSpaces).Returns (SampleColorSpaces);
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			return new BrushPropertyViewModel (TargetPlatform.Default, mockProperty.Object, new[] { mockEditor });
		}
	}
}
