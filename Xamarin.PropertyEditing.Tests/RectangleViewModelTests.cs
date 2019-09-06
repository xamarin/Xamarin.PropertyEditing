using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class RectangleViewModelTests
		: PropertyViewModelTests<CommonRectangle, RectanglePropertyViewModel>
	{
		[Test]
		public void X ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRectangle (0, 0, 0, 0)));

			bool xChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RectanglePropertyViewModel.X))
					xChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.X = 5;
			Assert.That (vm.Value.X, Is.EqualTo (5));
			Assert.That (xChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Y ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRectangle (0, 0, 0, 0)));

			bool yChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Y))
					yChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Y = 5;
			Assert.That (vm.Value.Y, Is.EqualTo (5));
			Assert.That (yChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Width ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRectangle (0, 0, 0, 0)));

			bool widthChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Width))
					widthChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Width = 5;
			Assert.That (vm.Value.Width, Is.EqualTo (5));
			Assert.That (widthChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Height ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonRectangle (0, 0, 0, 0)));

			bool heightChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Height))
					heightChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Height = 5;
			Assert.That (vm.Value.Height, Is.EqualTo (5));
			Assert.That (heightChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void ValueChangesXYWidthHeight ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new[] { editor });
			Assume.That (vm.X, Is.EqualTo (0));
			Assume.That (vm.Y, Is.EqualTo (0));
			Assume.That (vm.Width, Is.EqualTo (0));
			Assume.That (vm.Height, Is.EqualTo (0));

			bool xChanged = false, yChanged = false, widthChanged = false, heightChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RectanglePropertyViewModel.X))
					xChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Y))
					yChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Width))
					widthChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Height))
					heightChanged = true;
				if (args.PropertyName == nameof (PointPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Value = new CommonRectangle (5, 10, 15, 20);

			Assert.That (vm.X, Is.EqualTo (5));
			Assert.That (vm.Y, Is.EqualTo (10));
			Assert.That (vm.Width, Is.EqualTo (15));
			Assert.That (vm.Height, Is.EqualTo (20));

			Assert.That (yChanged, Is.True);
			Assert.That (xChanged, Is.True);
			Assert.That (widthChanged, Is.True);
			Assert.That (heightChanged, Is.True);

			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void HasOrigin ()
		{
			var mockProperty = GetPropertyMock ();
			var origin = mockProperty.As<IOrigin> ();
			origin.Setup (n => n.HasOrigin).Returns (true);

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Properties).Returns (new[] { mockProperty.Object });
			SetupPropertySetAndGet (editor, mockProperty.Object);

			var vm = GetViewModel (mockProperty.Object, editor.Object);
			Assert.That (vm.HasOrigin, Is.True);
		}

		[Test]
		public void Origin ()
		{
			var mockProperty = GetPropertyMock ();
			var origin = mockProperty.As<IOrigin> ();
			origin.Setup (n => n.HasOrigin).Returns (true);

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Properties).Returns (new[] { mockProperty.Object });
			SetupPropertySetAndGet (editor, mockProperty.Object);

			var vm = GetViewModel (mockProperty.Object, editor.Object);

			Assume.That (vm.Origin, Is.Null);

			bool originChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Origin))
					originChanged = true;
				if (args.PropertyName == nameof (RectanglePropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Origin = CommonOrigin.Center;

			Assert.That (vm.Value.Origin, Is.EqualTo (CommonOrigin.Center));
			Assert.That (originChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		protected override CommonRectangle GetRandomTestValue (Random rand)
		{
			return new CommonRectangle (rand.Next (), rand.Next (), rand.Next (), rand.Next ());
		}

		protected override RectanglePropertyViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new RectanglePropertyViewModel (platform, property, editors);
		}
	}
}
