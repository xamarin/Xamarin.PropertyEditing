using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal class MaterialDesignColorViewModelTests : BrushPropertyViewModelTests
	{
		[Test]
		public void ValueChangesTriggerPropertyChanges ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			var colorChanged = false;
			var colorNameChanged = false;
			var alphaChanged = false;
			var accentChanged = false;
			var normalChanged = false;
			var accentScaleChanged = false;
			var normalScaleChanged = false;

			vm.MaterialDesign.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (MaterialDesignColorViewModel.Color):
					colorChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.ColorName):
					colorNameChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.Alpha):
					alphaChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.AccentColor):
					accentChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.NormalColor):
					normalChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.AccentColorScale):
					accentScaleChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.NormalColorScale):
					normalScaleChanged = true;
					break;
				}
			};

			CommonColor newColor = GetNewRandomColor (Random, vm.MaterialDesign.Color);
			vm.Value = new CommonSolidBrush (newColor);

			Assert.AreEqual (newColor, vm.MaterialDesign.Color);
			Assert.IsTrue (colorChanged);
			Assert.IsTrue (colorNameChanged);
			Assert.IsTrue (alphaChanged);
			Assert.IsTrue (accentChanged);
			Assert.IsTrue (normalChanged);
			Assert.IsTrue (accentScaleChanged);
			Assert.IsTrue (normalScaleChanged);
		}

		[Test]
		public void AccentAndNormalChangesTriggerPropertyChanges ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			var colorChanged = false;
			var colorNameChanged = false;
			var alphaChanged = false;
			var accentChanged = false;
			var normalChanged = false;
			var accentScaleChanged = false;
			var normalScaleChanged = false;

			CommonColor originalColor = GetNewRandomColor (Random, vm.MaterialDesign.Color);
			vm.Value = new CommonSolidBrush (originalColor);

			vm.MaterialDesign.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (MaterialDesignColorViewModel.Color):
					colorChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.ColorName):
					colorNameChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.Alpha):
					alphaChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.AccentColor):
					accentChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.NormalColor):
					normalChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.AccentColorScale):
					accentScaleChanged = true;
					break;
				case nameof (MaterialDesignColorViewModel.NormalColorScale):
					normalScaleChanged = true;
					break;
				}
			};

			// First, set a normal color
			MaterialColorScale scale = GetRandomScale (Random, false);
			CommonColor newColor = GetNewRandomScaledColor (Random, scale, originalColor);
			var newOpaqueColor = new CommonColor (newColor.R, newColor.G, newColor.B);
			var newTransparentColor = new CommonColor (newColor.R, newColor.G, newColor.B, originalColor.A);
			vm.MaterialDesign.NormalColor = newOpaqueColor;

			Assert.AreEqual (newTransparentColor, vm.MaterialDesign.Color);
			Assert.AreEqual (newOpaqueColor, vm.MaterialDesign.NormalColor);
			Assert.AreEqual (scale.Name, vm.MaterialDesign.ColorName);
			Assert.IsNull (vm.MaterialDesign.AccentColor);
			Assert.IsTrue (colorChanged);
			Assert.IsTrue (colorNameChanged);
			Assert.IsTrue (alphaChanged);
			Assert.IsTrue (accentChanged);
			Assert.IsTrue (normalChanged);
			Assert.IsTrue (accentScaleChanged);
			Assert.IsTrue (normalScaleChanged);

			colorChanged = false; colorNameChanged = false; alphaChanged = false; accentChanged = false;
			normalChanged = false; accentScaleChanged = false; normalScaleChanged = false;

			// Then set an accent color
			scale = GetRandomScale (Random, true);
			newColor = GetNewRandomScaledColor (Random, scale, originalColor);
			newOpaqueColor = new CommonColor (newColor.R, newColor.G, newColor.B);
			newTransparentColor = new CommonColor (newColor.R, newColor.G, newColor.B, originalColor.A);
			vm.MaterialDesign.AccentColor = newOpaqueColor;

			Assert.AreEqual (newTransparentColor, vm.MaterialDesign.Color);
			Assert.AreEqual (newOpaqueColor, vm.MaterialDesign.AccentColor);
			Assert.AreEqual (scale.Name, vm.MaterialDesign.ColorName);
			Assert.IsNull (vm.MaterialDesign.NormalColor);
			Assert.IsTrue (colorChanged);
			Assert.IsTrue (colorNameChanged);
			Assert.IsTrue (alphaChanged);
			Assert.IsTrue (accentChanged);
			Assert.IsTrue (normalChanged);
			Assert.IsTrue (accentScaleChanged);
			Assert.IsTrue (normalScaleChanged);
		}

		[Test]
		public void ColorNameChangesScalesAndKeepsAlphaAndAccent ()
		{
			// Set the color to some accent color
			BrushPropertyViewModel vm = PrepareMockViewModel ();
			MaterialColorScale scale = GetRandomScale (Random, true); // Accent color
			CommonColor scaledColor = GetNewRandomScaledColor (Random, scale, CommonColor.Black);
			var color = new CommonColor (scaledColor.R, scaledColor.G, scaledColor.B, Random.NextByte ());
			vm.Value = new CommonSolidBrush (color);

			Assert.AreEqual (scale.Colors, vm.MaterialDesign.AccentColorScale);

			MaterialColorScale expectedNormalScale = MaterialDesignColorViewModel.MaterialPalettes
				.First (p => !p.IsAccent && p.Name == scale.Name);
			Assert.AreEqual (expectedNormalScale.Colors, vm.MaterialDesign.NormalColorScale);
			Assert.AreEqual (color.A, vm.MaterialDesign.Alpha);

			var accentIndex = Array.IndexOf (scale.Colors.ToArray(), scaledColor);

			// Then change to another scale that has accents too
			MaterialColorScale newScale = GetRandomScale (Random, true);
			var newColorName = newScale.Name;
			while (newColorName == scale.Name) newColorName = GetRandomScale (Random, true).Name;
			vm.MaterialDesign.ColorName = newColorName;

			Assert.AreEqual (newColorName, vm.MaterialDesign.ColorName);
			Assert.AreEqual (newScale.Colors, vm.MaterialDesign.AccentColorScale);
			Assert.AreEqual (accentIndex, Array.IndexOf (newScale.Colors.ToArray (), vm.MaterialDesign.AccentColor));
			expectedNormalScale = MaterialDesignColorViewModel.MaterialPalettes
				.First (p => !p.IsAccent && p.Name == newScale.Name);
			Assert.AreEqual (expectedNormalScale.Colors, vm.MaterialDesign.NormalColorScale);
			Assert.AreEqual (color.A, vm.MaterialDesign.Alpha);

			// Finally, change to grey, which has only normal nuances, but no accents,
			// so color should snap to the closest normal.
			var grey = Properties.Resources.MaterialColorGrey;
			vm.MaterialDesign.ColorName = Properties.Resources.MaterialColorGrey;

			Assert.AreEqual (grey, vm.MaterialDesign.ColorName);
			Assert.AreEqual (MaterialDesignColorViewModel.EmptyColorScale, vm.MaterialDesign.AccentColorScale);
			Assert.IsNull (vm.MaterialDesign.AccentColor);
			Assert.AreEqual (MaterialDesignColorViewModel.MaterialPalettes.First(p => p.Name == grey).Colors,
				vm.MaterialDesign.NormalColorScale);
			Assert.IsNotNull (vm.MaterialDesign.NormalColor);
			Assert.AreEqual (color.A, vm.MaterialDesign.Alpha);
		}

		[Test]
		public void ColorsSnapBetweenBlackAndWhiteAndOtherPalettes ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			MaterialColorScale blackAndWhiteScale = MaterialDesignColorViewModel.MaterialPalettes.Last();
			MaterialColorScale normalScale = MaterialDesignColorViewModel.MaterialPalettes[0];
			MaterialColorScale accentScale = MaterialDesignColorViewModel.MaterialPalettes[1];

			CommonColor lightNormalColor = normalScale.Colors[0];
			CommonColor darkNormalColor = normalScale.Colors[9];

			CommonColor lightAccentColor = accentScale.Colors[0];
			CommonColor darkAccentColor = accentScale.Colors[3];

			vm.Value = new CommonSolidBrush (lightNormalColor);

			vm.MaterialDesign.ColorName = blackAndWhiteScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (CommonColor.White));

			vm.MaterialDesign.ColorName = normalScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (lightNormalColor));

			vm.Value = new CommonSolidBrush (darkNormalColor);

			vm.MaterialDesign.ColorName = blackAndWhiteScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (CommonColor.Black));

			vm.MaterialDesign.ColorName = normalScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (darkNormalColor));

			vm.Value = new CommonSolidBrush (lightAccentColor);

			vm.MaterialDesign.ColorName = blackAndWhiteScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (CommonColor.White));

			vm.MaterialDesign.ColorName = accentScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (lightNormalColor));

			vm.Value = new CommonSolidBrush (darkAccentColor);

			vm.MaterialDesign.ColorName = blackAndWhiteScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (CommonColor.Black));

			vm.MaterialDesign.ColorName = accentScale.Name;
			Assert.That (vm.Solid.Color, Is.EqualTo (darkNormalColor));
		}

		[Test]
		[Description ("If we have a resource brush value that matches material, we need to ensure it doesn't auto switch to material")]
		public async Task ResourceBrushMatchesMaterialStaysResource()
		{
			var platform = new TargetPlatform (new MockEditorProvider ()) {
				SupportsMaterialDesign = true
			};
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonSolidBrush));

			var mockEditor = new MockObjectEditor (mockProperty.Object);

			var provider = new MockResourceProvider ();
			var resources = await provider.GetResourcesAsync (mockEditor.Target, mockProperty.Object, CancellationToken.None);
			var resource = resources.OfType<Resource<CommonSolidBrush>> ().First (r => r.Value == new CommonSolidBrush (0, 0, 0));

			await mockEditor.SetValueAsync (mockProperty.Object, new ValueInfo<CommonSolidBrush> {
				Source = ValueSource.Resource,
				Value = resource.Value,
				SourceDescriptor = resource
			});

			var vm = new BrushPropertyViewModel (platform, mockProperty.Object, new[] { mockEditor });
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Resource));
			Assert.That (vm.SelectedBrushType, Is.EqualTo (CommonBrushType.Resource));
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
			while (newColor.Equals (oldColor, true)) newColor = rand.NextColor ();
			return newColor;
		}

		private static MaterialColorScale GetRandomScale (Random rand, bool isAccent)
		{
			MaterialColorScale[] accentedScales = MaterialDesignColorViewModel.MaterialPalettes
				.Where (s => s.IsAccent == isAccent).ToArray ();
			return accentedScales[rand.Next (accentedScales.Length)];
		}

		private static CommonColor GetNewRandomScaledColor (Random rand, MaterialColorScale scale, CommonColor oldColor)
		{
			CommonColor newAccent = scale.Colors[rand.Next (scale.Colors.Count)];
			while (newAccent.Equals (oldColor, true)) newAccent = scale.Colors[rand.Next (scale.Colors.Count)];
			return newAccent;
		}

		private static CommonColor GetNewRandomAccent (Random rand, CommonColor oldColor)
		{
			MaterialColorScale scale = GetRandomScale (rand, true);
			return GetNewRandomScaledColor (rand, scale, oldColor);
		}

		private static CommonColor GetNewRandomNormal (Random rand, CommonColor oldColor)
		{
			MaterialColorScale scale = GetRandomScale (rand, false);
			return GetNewRandomScaledColor (rand, scale, oldColor);
		}

		private static BrushPropertyViewModel PrepareMockViewModel ()
		{
			var platform = new TargetPlatform (new MockEditorProvider()) {
				SupportsMaterialDesign = true
			};
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonSolidBrush));
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			return new BrushPropertyViewModel (platform, mockProperty.Object, new[] { mockEditor });
		}
	}
}
