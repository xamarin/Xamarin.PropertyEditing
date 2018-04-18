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
		public void ValueChangesTriggerSolidColorAndColorSpacePropertyChangesOnly ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			var colorChanged = false;
			var colorSpaceChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;

			CommonColor initialColor = vm.Solid.InitialColor;
			CommonColor lastColor = vm.Solid.LastColor;
			CommonColor shade = vm.Solid.Shade;
			CommonColor hueColor = vm.Solid.HueColor;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.ColorSpace):
					colorSpaceChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				}
			};

			CommonColor newColor = GetNewRandomColor (Random, vm.Solid.Color);
			vm.Value = new CommonSolidBrush (newColor);

			Assert.IsTrue (colorChanged);
			Assert.AreEqual (newColor, vm.Solid.Color);
			Assert.IsTrue (colorSpaceChanged);
			Assert.IsFalse (hueColorChanged);
			Assert.AreEqual (hueColor, vm.Solid.HueColor);
			Assert.IsFalse (shadeChanged);
			Assert.AreEqual (shade, vm.Solid.Shade);
			Assert.AreEqual (initialColor, vm.Solid.InitialColor);
			Assert.AreEqual (lastColor, vm.Solid.LastColor);
		}

		[Test]
		public void UpdatingHueOnGreyDoesntCauseColorToChange ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			var grey = new CommonColor (20, 20, 20);
			vm.Value = new CommonSolidBrush (grey);

			var colorChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				}
			};

			vm.Solid.HueColor = new CommonColor (0, 0, 255);

			Assert.IsFalse (colorChanged);
			Assert.AreEqual (grey, vm.Solid.Color);
			Assert.IsTrue (hueColorChanged);
			Assert.IsFalse (shadeChanged);
		}

		[Test]
		public void UpdatingHueOnSaturatedColorsCausesColorToChange ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			var blue = new CommonColor (20, 20, 255);
			vm.Value = new CommonSolidBrush (blue);

			var colorChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				}
			};

			vm.Solid.HueColor = new CommonColor (255, 0, 0);

			Assert.IsTrue (colorChanged);
			Assert.IsTrue (hueColorChanged);
			Assert.IsFalse (shadeChanged);
		}

		[Test]
		public void UpdatingShadeCausesColorToChange ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			vm.Value = GetRandomTestValue (Random);

			var colorChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;
			var alpha = vm.Solid.Color.A;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				}
			};

			CommonColor newShade = GetNewRandomColor (Random, vm.Solid.Shade);
			vm.Solid.Shade = newShade;

			Assert.IsTrue (colorChanged);
			Assert.IsFalse (hueColorChanged);
			Assert.IsTrue (shadeChanged);
			Assert.AreEqual (alpha, vm.Solid.Color.A);
			Assert.AreEqual (newShade.R, vm.Solid.Color.R);
			Assert.AreEqual (newShade.G, vm.Solid.Color.G);
			Assert.AreEqual (newShade.B, vm.Solid.Color.B);
		}

		[Test]
		public void UpdatingColorCausesParentValueToChangeAndInitialColorToBeSetOnTheFirstTime ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			vm.Value = GetRandomTestValue (Random);

			var colorChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;
			var parentChanged = false;
			var alpha = vm.Solid.Color.A;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				}
			};
			vm.PropertyChanged += (s, e) => {
				parentChanged = true;
			};

			CommonColor newColor = GetNewRandomColor (Random, vm.Solid.Color);
			vm.Solid.Color = newColor;

			Assert.IsTrue (parentChanged);
			Assert.IsTrue (colorChanged);
			Assert.IsFalse (hueColorChanged);
			Assert.IsFalse (shadeChanged);
			Assert.AreEqual (newColor, vm.Solid.Color);
			Assert.AreEqual (newColor, vm.Solid.InitialColor);
		}

		[Test]
		public void InitialAndLastColorDontChangeOnceSet ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			vm.Value = GetRandomTestValue (Random);

			CommonColor initialColor = vm.Solid.InitialColor;
			CommonColor lastColor = vm.Solid.LastColor;

			CommonColor newColor = GetNewRandomColor (Random, vm.Solid.Color);
			vm.Solid.Color = newColor;

			Assert.AreEqual (initialColor, vm.Solid.InitialColor);
			Assert.AreEqual (lastColor, vm.Solid.InitialColor);
		}

		[Test]
		public void CommitLastColorChangesLastColorAndShade ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			vm.Value = GetRandomTestValue (Random);

			CommonColor lastColor = vm.Solid.LastColor;

			CommonColor newColor = GetNewRandomColor (Random, vm.Solid.Color);
			vm.Solid.Color = newColor;

			var colorChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;
			var lastColorChanged = false;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				case nameof (SolidBrushViewModel.LastColor):
					lastColorChanged = true;
					break;
				}
			};

			vm.Solid.CommitLastColor ();

			Assert.IsTrue (lastColorChanged);
			Assert.IsTrue (shadeChanged);
			Assert.IsFalse (colorChanged);
			Assert.IsFalse (hueColorChanged);
			Assert.AreEqual (newColor, vm.Solid.LastColor);
		}

		[Test]
		public void CommitHueChangesHueOnly ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			vm.Value = GetRandomTestValue (Random);

			CommonColor hueColor = vm.Solid.HueColor;

			CommonColor newColor = GetNewRandomColor (Random, vm.Solid.Color);
			vm.Solid.Color = newColor;

			var colorChanged = false;
			var hueColorChanged = false;
			var shadeChanged = false;
			var lastColorChanged = false;

			vm.Solid.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (SolidBrushViewModel.Color):
					colorChanged = true;
					break;
				case nameof (SolidBrushViewModel.HueColor):
					hueColorChanged = true;
					break;
				case nameof (SolidBrushViewModel.Shade):
					shadeChanged = true;
					break;
				case nameof (SolidBrushViewModel.LastColor):
					lastColorChanged = true;
					break;
				}
			};

			vm.Solid.CommitHue ();

			Assert.IsTrue (hueColorChanged);
			Assert.IsFalse (lastColorChanged);
			Assert.IsFalse (shadeChanged);
			Assert.IsFalse (colorChanged);
		}

		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			CommonColor color = rand.NextColor ();
			var colorSpace = rand.NextString ();
			var opacity = rand.NextDouble ();

			return new CommonSolidBrush (color, colorSpace, opacity);
		}

		private static CommonColor GetNewRandomColor (Random rand, CommonColor oldColor)
		{
			CommonColor newColor = rand.NextColor ();
			while (newColor == oldColor) newColor = rand.NextColor ();
			return newColor;
		}

		private static BrushPropertyViewModel PrepareMockViewModel ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonSolidBrush));
			mockProperty.As<IColorSpaced> ().SetupGet (pi => pi.ColorSpaces).Returns (SampleColorSpaces);
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			return new BrushPropertyViewModel (MockEditorProvider.MockPlatform, mockProperty.Object, new[] { mockEditor });
		}
	}
}
