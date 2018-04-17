using System;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal class ImageBrushPropertyViewModelTests : BrushPropertyViewModelTests
	{
		[Test]
		public void ValueChangesTriggerPropertyChanges ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			var imageSourceChanged = false;
			var stretchChanged = false;
			var tileModeChanged = false;

			vm.Image.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (ImageBrushViewModel.ImageSource):
					imageSourceChanged = true;
					break;
				case nameof (ImageBrushViewModel.Stretch):
					stretchChanged = true;
					break;
				case nameof (ImageBrushViewModel.TileMode):
					tileModeChanged = true;
					break;
				}
			};

			vm.Value = GetRandomTestValue();

			Assert.IsTrue (imageSourceChanged);
			Assert.IsTrue (stretchChanged);
			Assert.IsTrue (tileModeChanged);
		}

		[Test]
		public void ImageBrushPropertyChangesTriggerPropertyChanges ()
		{
			BrushPropertyViewModel vm = PrepareMockViewModel ();

			var valueChanged = false;
			var imageSourceChanged = false;
			var stretchChanged = false;
			var tileModeChanged = false;

			vm.Value = GetRandomTestValue ();

			vm.PropertyChanged += (s, e) => {
				if (e.PropertyName == nameof(PropertyViewModel<ImageBrushViewModel>.Value)) {
					valueChanged = true;
				}
			};

			vm.Image.PropertyChanged += (s, e) => {
				switch (e.PropertyName) {
				case nameof (ImageBrushViewModel.ImageSource):
					imageSourceChanged = true;
					break;
				case nameof (ImageBrushViewModel.Stretch):
					stretchChanged = true;
					break;
				case nameof (ImageBrushViewModel.TileMode):
					tileModeChanged = true;
					break;
				}
			};

			var newUri = new Uri(Random.NextFormattedString(uriFormat, differentFrom: vm.Image.ImageSource.UriSource.AbsoluteUri));
			vm.Image.ImageSource = new CommonImageSource { UriSource = newUri };

			Assert.AreEqual (newUri, vm.Image.ImageSource.UriSource);
			Assert.IsTrue (valueChanged);
			Assert.IsTrue (imageSourceChanged);
			Assert.IsTrue (stretchChanged);
			Assert.IsTrue (tileModeChanged);

			valueChanged = false; imageSourceChanged = false; stretchChanged = false; tileModeChanged = false;

			vm.Image.Stretch = Random.Next (differentFrom: vm.Image.Stretch);

			Assert.IsTrue (valueChanged);
			Assert.IsTrue (imageSourceChanged);
			Assert.IsTrue (stretchChanged);
			Assert.IsTrue (tileModeChanged);

			valueChanged = false; imageSourceChanged = false; stretchChanged = false; tileModeChanged = false;

			vm.Image.TileMode = Random.Next (differentFrom: vm.Image.TileMode);

			Assert.IsTrue (valueChanged);
			Assert.IsTrue (imageSourceChanged);
			Assert.IsTrue (stretchChanged);
			Assert.IsTrue (tileModeChanged);
		}

		protected override CommonBrush GetRandomTestValue (Random rand)
		{
			var imageSource = rand.NextFormattedString(uriFormat);
			CommonAlignmentX alignmentX = rand.Next<CommonAlignmentX> ();
			CommonAlignmentY alignmentY = rand.Next<CommonAlignmentY> ();
			CommonStretch stretch = rand.Next<CommonStretch> ();
			CommonTileMode tileMode = rand.Next<CommonTileMode> ();
			var viewBox = new CommonRectangle (
				rand.NextDouble(),
				rand.NextDouble(),
				rand.NextDouble(),
				rand.NextDouble()
			);
			CommonBrushMappingMode viewBoxUnit = rand.Next<CommonBrushMappingMode> ();
			var viewPort = new CommonRectangle (
				rand.NextDouble (),
				rand.NextDouble (),
				rand.NextDouble (),
				rand.NextDouble ()
			);
			CommonBrushMappingMode viewPortUnit = rand.Next<CommonBrushMappingMode> ();
			var opacity = rand.NextDouble ();

			return new CommonImageBrush (
				imageSource,
				alignmentX, alignmentY,
				stretch, tileMode,
				viewBox, viewBoxUnit,
				viewPort, viewPortUnit,
				opacity);
		}

		private const string uriFormat = "http://microsoft.com/{0}.gif";

		private static BrushPropertyViewModel PrepareMockViewModel ()
		{
			var platform = new TargetPlatform {
				SupportsImageBrush = true
			};
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonImageBrush));
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			return new BrushPropertyViewModel (platform, mockProperty.Object, new[] { mockEditor });
		}
	}
}
