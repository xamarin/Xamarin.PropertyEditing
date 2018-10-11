using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class AddValueConverterViewModelTests
	{
		[SetUp]
		public void Setup ()
		{
			this.syncContext = new TestContext ();
			SynchronizationContext.SetSynchronizationContext (this.syncContext);
		}

		[TearDown]
		public void TearDown ()
		{
			SynchronizationContext.SetSynchronizationContext (null);
			this.syncContext.ThrowPendingExceptions ();
		}

		[Test]
		public void ConverterNameSuggestedWithProvider ()
		{
			object target = new object();

			var editorProvider = new Mock<IEditorProvider> ();

			var type = new TypeInfo (new AssemblyInfo ("Assembly", false), "Namespace", "Name");

			const string suggested = "SuggestedName";

			var resourcesProvider = new Mock<IResourceProvider> ();
			resourcesProvider.Setup (rp => rp.SuggestResourceNameAsync (It.IsAny<IReadOnlyCollection<object>> (), type))
				.ReturnsAsync (suggested);

			var types = new AssignableTypesResult (new [] { type });
			var vm = new AddValueConverterViewModel (
				new TargetPlatform (editorProvider.Object, resourcesProvider.Object), target,
				new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (
					Task.FromResult (types.GetTypeTree ())));

			Assume.That (vm.ConverterName, Is.Null);

			vm.SelectedType = type;

			Assert.That (vm.ConverterName, Is.EqualTo (suggested));
		}

		[Test]
		public void ConverterNameSuggestedWithoutProvider ()
		{
			object target = new object ();

			var editorProvider = new Mock<IEditorProvider> ();

			var type = new TypeInfo (new AssemblyInfo ("Assembly", false), "Namespace", "Name");

			var types = new AssignableTypesResult (new[] { type });
			var vm = new AddValueConverterViewModel (
				new TargetPlatform (editorProvider.Object), target,
				new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (
					Task.FromResult (types.GetTypeTree ())));

			Assume.That (vm.ConverterName, Is.Null);

			vm.SelectedType = type;

			Assert.That (vm.ConverterName, Is.EqualTo (type.Name));
		}

		private TestContext syncContext;
	}
}
