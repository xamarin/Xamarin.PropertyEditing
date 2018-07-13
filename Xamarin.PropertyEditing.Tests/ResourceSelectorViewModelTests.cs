using System.Threading;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class ResourceSelectorViewModelTests
	{
		[Test]
		public void Resources()
		{
			var tests = new StringViewModelTests();
			var mproperty = tests.GetPropertyMock ("Property");

			object target = new object();

			var resource = new Resource<string> (MockResourceProvider.SystemResourcesSource, "Resource", "value");
			var mprovider = new Mock<IResourceProvider>();

			mprovider.Setup (r => r.GetResourcesAsync (target, mproperty.Object, CancellationToken.None)).ReturnsAsync (new[] { resource });

			var vm = new ResourceSelectorViewModel (mprovider.Object, new[] { target }, mproperty.Object);
			Assert.That (vm.Resources, Contains.Item (resource));
		}

		[Test]
		public void MultipleTargetResources()
		{
			var tests = new StringViewModelTests();
			var mproperty = tests.GetPropertyMock ("Property");

			object target = new object();
			object target2 = new object();

			var resource = new Resource<string> (MockResourceProvider.SystemResourcesSource, "Resource", "value");
			var resource2 = new Resource<string> (MockResourceProvider.SystemResourcesSource, "Resource2", "value2");
			var resource3 = new Resource<string> (MockResourceProvider.SystemResourcesSource, "Resource3", "value3");

			var mprovider = new Mock<IResourceProvider>();
			mprovider.Setup (r => r.GetResourcesAsync (target, mproperty.Object, CancellationToken.None)).ReturnsAsync (new[] { resource, resource3 });
			mprovider.Setup (r => r.GetResourcesAsync (target2, mproperty.Object, CancellationToken.None)).ReturnsAsync (new[] { resource2, resource3 });

			var vm = new ResourceSelectorViewModel (mprovider.Object, new[] { target, target2 }, mproperty.Object);
			Assert.That (vm.Resources, Does.Not.Contain (resource));
			Assert.That (vm.Resources, Does.Not.Contain (resource2));
			Assert.That (vm.Resources, Contains.Item (resource3));
		}

		[Test]
		public void FuzzyFilter ()
		{
			var tests = new StringViewModelTests();
			var mproperty = tests.GetPropertyMock ("Property");

			var target = new object();
			var resource = new Resource<string> (MockResourceProvider.SystemResourcesSource, "@android:string/foo_bar", "value");
			var resource2 = new Resource<string> (MockResourceProvider.SystemResourcesSource, "@android:string/foo_baz", "value");

			var mprovider = new Mock<IResourceProvider>();
			mprovider.Setup (r => r.GetResourcesAsync (target, mproperty.Object, CancellationToken.None)).ReturnsAsync (new[] { resource, resource2 });

			var vm = new ResourceSelectorViewModel (mprovider.Object, new[] { target }, mproperty.Object);
			Assume.That (vm.Resources, Contains.Item (resource));
			Assume.That (vm.Resources, Contains.Item (resource2));

			vm.FilterText = "baz";
			Assert.That (vm.Resources, Does.Not.Contain (resource));
			Assert.That (vm.Resources, Contains.Item (resource2));

			vm.FilterText = "string";
			Assert.That (vm.Resources, Contains.Item (resource));
			Assert.That (vm.Resources, Contains.Item (resource2));
		}

		[Test]
		public void OnlyLocal ()
		{
			var tests = new StringViewModelTests ();
			var mproperty = tests.GetPropertyMock ("Property");

			var target = new object ();
			var resource = new Resource<string> (MockResourceProvider.SystemResourcesSource, "@android:string/foo_bar", "value");
			var resource2 = new Resource<string> (MockResourceProvider.ApplicationResourcesSource, "@android:string/foo_baz", "value");

			var mprovider = new Mock<IResourceProvider> ();
			mprovider.Setup (r => r.GetResourcesAsync (target, mproperty.Object, CancellationToken.None)).ReturnsAsync (new[] { resource, resource2 });

			var vm = new ResourceSelectorViewModel (mprovider.Object, new[] { target }, mproperty.Object);
			Assume.That (vm.Resources, Contains.Item (resource));
			Assume.That (vm.Resources, Contains.Item (resource2));

			vm.ShowOnlyLocalResources = true;
			Assert.That (vm.Resources, Does.Not.Contain (resource));
			Assert.That (vm.Resources, Contains.Item (resource2));
		}

		[Test]
		public void OnlyShared ()
		{
			var tests = new StringViewModelTests ();
			var mproperty = tests.GetPropertyMock ("Property");

			var target = new object ();
			var resource = new Resource<string> (MockResourceProvider.SystemResourcesSource, "@android:string/foo_bar", "value");
			var resource2 = new Resource<string> (MockResourceProvider.ApplicationResourcesSource, "@android:string/foo_baz", "value");

			var mprovider = new Mock<IResourceProvider> ();
			mprovider.Setup (r => r.GetResourcesAsync (target, mproperty.Object, CancellationToken.None)).ReturnsAsync (new[] { resource, resource2 });

			var vm = new ResourceSelectorViewModel (mprovider.Object, new[] { target }, mproperty.Object);
			Assume.That (vm.Resources, Contains.Item (resource));
			Assume.That (vm.Resources, Contains.Item (resource2));

			vm.ShowOnlySystemResources = true;
			Assert.That (vm.Resources, Does.Not.Contain (resource2));
			Assert.That (vm.Resources, Contains.Item (resource));
		}
	}
}
