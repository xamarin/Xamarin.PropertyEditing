using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class BrushPropertyViewModelTests : PropertyViewModelTests<CommonBrush, PropertyViewModel<CommonBrush>>
	{
		protected override PropertyViewModel<CommonBrush> GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new BrushPropertyViewModel (platform, property, editors);
		}

		[Test]
		public void SettingValueTriggersOpacityChange()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonBrush));
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			var vm = new BrushPropertyViewModel (MockEditorProvider.MockPlatform, mockProperty.Object, new[] { mockEditor });
			var changed = false;
			vm.PropertyChanged += (s, e) => {
				if (e.PropertyName == nameof(BrushPropertyViewModel.Opacity)) {
					changed = true;
				}
			};
			vm.Value = GetRandomTestValue();
			Assert.IsTrue (changed);
		}

		[Test]
		public void ChangingEditorsUpdatesResources ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonBrush));
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			var vm = new BrushPropertyViewModel (new TargetPlatform (new MockEditorProvider(), new MockResourceProvider()), mockProperty.Object, new [] { mockEditor });

			var changed = false;
			vm.PropertyChanged += (s, e) => {
				if (e.PropertyName == nameof (BrushPropertyViewModel.ResourceSelector)) {
					changed = true;
				}
			};
			var rs1 = vm.ResourceSelector;
			vm.Editors.Add (new MockObjectEditor ());
			var rs2 = vm.ResourceSelector;
			Assert.IsTrue (changed);
			Assert.AreNotEqual (rs1, rs2);
		}

		[Test]
		public void ResourcesChangedUpdatesResources ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (CommonBrush));
			var mockEditor = new MockObjectEditor (mockProperty.Object);

			var resource1 = new Resource ("first");
			var resources = new List<Resource> { resource1 };

			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (It.IsAny<object> ())).ReturnsAsync (new[] { MockResourceProvider.SystemResourcesSource });
			provider.Setup (p => p.GetResourcesAsync (It.IsAny<object> (), mockProperty.Object, CancellationToken.None)).ReturnsAsync (resources);

			var vm = new BrushPropertyViewModel (new TargetPlatform (new MockEditorProvider(), provider.Object), mockProperty.Object, new [] { mockEditor });
			
			Assume.That (vm.ResourceSelector.Resources, Contains.Item (resource1));

			var resource2 = new Resource ("second");
			resources.Add (resource2);

			provider.Raise (rp => rp.ResourcesChanged += null, new ResourcesChangedEventArgs());
			Assert.That (vm.ResourceSelector.Resources, Contains.Item (resource1));
			Assert.That (vm.ResourceSelector.Resources, Contains.Item (resource2));
		}
	}
}
