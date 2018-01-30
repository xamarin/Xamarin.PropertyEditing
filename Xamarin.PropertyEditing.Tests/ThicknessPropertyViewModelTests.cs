using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class ThicknessPropertyViewModelTests
		:PropertyViewModelTests<CommonThickness, ThicknessPropertyViewModel>
	{
		[Test]
		public void Top ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new [] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonThickness (0, 0, 0, 0)));

			bool topChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Top))
					topChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Top = 5;
			Assert.That (vm.Value.Top, Is.EqualTo (5));
			Assert.That (topChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Left ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new [] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonThickness (0, 0, 0, 0)));

			bool leftChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Left))
					leftChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Left = 5;
			Assert.That (vm.Value.Left, Is.EqualTo (5));
			Assert.That (leftChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Right ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new [] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonThickness (0, 0, 0, 0)));

			bool rightChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Right))
					rightChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Right = 5;
			Assert.That (vm.Value.Right, Is.EqualTo (5));
			Assert.That (rightChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void Bottom ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new [] { editor });
			Assume.That (vm.Value, Is.EqualTo (new CommonThickness (0, 0, 0, 0)));

			bool bottomChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Bottom))
					bottomChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Bottom = 5;
			Assert.That (vm.Value.Bottom, Is.EqualTo (5));
			Assert.That (bottomChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		[Test]
		public void ValueChangesProps ()
		{
			var property = GetPropertyMock ();
			var editor = GetBasicEditor (property.Object);
			var vm = GetViewModel (property.Object, new [] { editor });
			Assume.That (vm.Left, Is.EqualTo (0));
			Assume.That (vm.Top, Is.EqualTo (0));
			Assume.That (vm.Bottom, Is.EqualTo (0));
			Assume.That (vm.Right, Is.EqualTo (0));

			bool leftChanged = false, topChanged = false, bottomChanged = false, rightChanged = false, valueChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Left))
					leftChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Top))
					topChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Bottom))
					bottomChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Right))
					rightChanged = true;
				if (args.PropertyName == nameof (ThicknessPropertyViewModel.Value))
					valueChanged = true;
			};

			vm.Value = new CommonThickness (top:5, left: 10, bottom: 15, right: 20);

			Assert.That (vm.Left, Is.EqualTo (10));
			Assert.That (vm.Top, Is.EqualTo (5));
			Assert.That (vm.Bottom, Is.EqualTo (15));
			Assert.That (vm.Right, Is.EqualTo (20));
			Assert.That (topChanged, Is.True);
			Assert.That (leftChanged, Is.True);
			Assert.That (rightChanged, Is.True);
			Assert.That (bottomChanged, Is.True);
			Assert.That (valueChanged, Is.True);
		}

		protected override CommonThickness GetRandomTestValue (Random rand)
		{
			return new CommonThickness (rand.Next (), rand.Next (), rand.Next (), rand.Next ());
		}

		protected override ThicknessPropertyViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new ThicknessPropertyViewModel (platform, property, editors);
		}
	}
}
