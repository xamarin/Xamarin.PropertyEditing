using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class CreateResourceViewModelTests
	{
		[Test]
		public void ApplicationSources ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assert.That (vm.HasApplicationSources, Is.True, "HasApplicationSources is false");
			Assert.That (vm.ApplicationSources.Count, Is.EqualTo (2), "Incorrect number of items");
			Assert.That (vm.ApplicationSources, Contains.Item (Sources[1]));
			Assert.That (vm.ApplicationSources, Contains.Item (Sources[2]));
		}

		[Test]
		public void DocumentSources ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assert.That (vm.HasDocumentSources, Is.True, "HasDocumentSources is false");
			Assert.That (vm.ApplicationSources.Count, Is.EqualTo (2), "Incorrect number of items");
			Assert.That (vm.DocumentSources, Contains.Item (Sources[3]));
			Assert.That (vm.DocumentSources, Contains.Item (Sources[4]));
		}

		[Test]
		public void IsKeyed ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (new[] {
				new ResourceSource ("app", ResourceSourceType.Application)
			});

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assert.That (vm.IsKeyed, Is.True);
			Assert.That (vm.IsAppliedToAll, Is.False);

			bool namedChanged = false, allChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.IsAppliedToAll))
					allChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.IsKeyed))
					namedChanged = true;
			};

			vm.IsKeyed = false;
			Assert.That (vm.IsKeyed, Is.False);
			Assert.That (vm.IsAppliedToAll, Is.True);
			Assert.That (namedChanged, Is.True);
			Assert.That (allChanged, Is.True);
			namedChanged = allChanged = false;

			vm.IsKeyed = true;
			Assert.That (vm.IsKeyed, Is.True);
			Assert.That (vm.IsAppliedToAll, Is.False);
			Assert.That (namedChanged, Is.True);
			Assert.That (allChanged, Is.True);
		}

		[Test]
		public void IsAppliedToAll ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (new[] {
				new ResourceSource ("app", ResourceSourceType.Application)
			});

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assert.That (vm.IsKeyed, Is.True);
			Assert.That (vm.IsAppliedToAll, Is.False);

			bool namedChanged = false, allChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.IsAppliedToAll))
					allChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.IsKeyed))
					namedChanged = true;
			};

			vm.IsAppliedToAll = true;
			Assert.That (vm.IsKeyed, Is.False);
			Assert.That (vm.IsAppliedToAll, Is.True);
			Assert.That (namedChanged, Is.True);
			Assert.That (allChanged, Is.True);
			namedChanged = allChanged = false;

			vm.IsAppliedToAll = false;
			Assert.That (vm.IsKeyed, Is.True);
			Assert.That (vm.IsAppliedToAll, Is.False);
			Assert.That (namedChanged, Is.True);
			Assert.That (allChanged, Is.True);
		}

		[Test]
		public void IsLoading ()
		{
			var tcs = new TaskCompletionSource<IReadOnlyList<ResourceSource>> ();

			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).Returns (tcs.Task);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assert.That (vm.IsLoading, Is.True);

			bool isLoadingChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.IsLoading))
					isLoadingChanged = true;
			};

			tcs.SetResult (new[] { new ResourceSource ("app", ResourceSourceType.Application) });
			Assert.That (isLoadingChanged, Is.True);
			Assert.That (vm.IsLoading, Is.False);
		}

		[Test]
		public void FatalError ()
		{
			var tcs = new TaskCompletionSource<IReadOnlyList<ResourceSource>> ();

			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).Returns (tcs.Task);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.HasFatalError, Is.False);
			Assume.That (vm.FatalError, Is.Null);

			bool hasErrorChanged = false, errorChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.FatalError))
					errorChanged = true;
				if (e.PropertyName == nameof(CreateResourceViewModel.HasFatalError))
					hasErrorChanged = true;
			};

			const string errorMessage = "test";
			tcs.SetException (new Exception (errorMessage));
			Assert.That (hasErrorChanged, Is.True);
			Assert.That (errorChanged, Is.True);
			Assert.That (vm.HasFatalError, Is.True);
			Assert.That (vm.FatalError, Is.EqualTo (errorMessage));
		}

		[Test]
		public void DefineInApplication ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.DocumentSources, Is.Not.Empty);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assert.That (vm.DefineInApplication, Is.True);
			Assert.That (vm.DefineInDocument, Is.False);
			Assert.That (vm.DefineInApplicationSource, Is.False);

			bool inAppChanged = false, inDictChanged = false, inDocumentChanged = false, selectedSourceChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.DefineInApplicationSource))
					inDictChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.DefineInApplication))
					inAppChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.DefineInDocument))
					inDocumentChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.SelectedResourceSource))
					selectedSourceChanged = true;
			};

			vm.DefineInApplication = false;
			Assert.That (vm.DefineInApplication, Is.False);
			Assert.That (vm.DefineInDocument, Is.True);
			Assert.That (vm.DefineInApplicationSource, Is.False);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[3]));
			Assert.That (selectedSourceChanged, Is.True);
			Assert.That (inAppChanged, Is.True);
			Assert.That (inDocumentChanged, Is.True);
			inAppChanged = inDictChanged = inDocumentChanged = selectedSourceChanged = false;

			vm.DefineInApplication = true;
			Assert.That (vm.DefineInApplication, Is.True);
			Assert.That (vm.DefineInDocument, Is.False);
			Assert.That (vm.DefineInApplicationSource, Is.False);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assert.That (selectedSourceChanged, Is.True);
			Assert.That (inAppChanged, Is.True);
			Assert.That (inDocumentChanged, Is.True);
		}

		[Test]
		public void DefineInDocument ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.DocumentSources, Is.Not.Empty);
			Assume.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assume.That (vm.DefineInApplication, Is.True);
			Assume.That (vm.DefineInDocument, Is.False);
			Assume.That (vm.DefineInApplicationSource, Is.False);

			bool inAppChanged = false, inDictChanged = false, inDocumentChanged = false, selectedSourceChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.DefineInApplicationSource))
					inDictChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.DefineInApplication))
					inAppChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.DefineInDocument))
					inDocumentChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.SelectedResourceSource))
					selectedSourceChanged = true;
			};

			vm.DefineInDocument = true;
			Assert.That (vm.DefineInApplication, Is.False);
			Assert.That (vm.DefineInDocument, Is.True);
			Assert.That (vm.DefineInApplicationSource, Is.False);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[3]));
			Assert.That (selectedSourceChanged, Is.True);
			Assert.That (inAppChanged, Is.True);
			Assert.That (inDocumentChanged, Is.True);
			inAppChanged = inDictChanged = inDocumentChanged = selectedSourceChanged = false;

			vm.DefineInDocument = false;
			Assert.That (vm.DefineInApplication, Is.True);
			Assert.That (vm.DefineInDocument, Is.False);
			Assert.That (vm.DefineInApplicationSource, Is.False);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assert.That (selectedSourceChanged, Is.True);
			Assert.That (inAppChanged, Is.True);
			Assert.That (inDocumentChanged, Is.True);
		}

		[Test]
		public void DefineInApplicationSource ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.ApplicationSources, Is.Not.Empty);
			Assume.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assume.That (vm.DefineInApplication, Is.True);
			Assume.That (vm.DefineInDocument, Is.False);
			Assume.That (vm.DefineInApplicationSource, Is.False);

			bool inAppChanged = false, inDictChanged = false, inDocumentChanged = false, selectedSourceChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.DefineInApplicationSource))
					inDictChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.DefineInApplication))
					inAppChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.DefineInDocument))
					inDocumentChanged = true;
				else if (e.PropertyName == nameof(CreateResourceViewModel.SelectedResourceSource))
					selectedSourceChanged = true;
			};

			vm.DefineInApplicationSource = true;
			Assert.That (vm.DefineInApplication, Is.False);
			Assert.That (vm.DefineInDocument, Is.False);
			Assert.That (vm.DefineInApplicationSource, Is.True);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[1]));
			Assert.That (selectedSourceChanged, Is.True);
			Assert.That (inAppChanged, Is.True);
			Assert.That (inDictChanged, Is.True);
			inAppChanged = inDictChanged = inDocumentChanged = selectedSourceChanged = false;

			vm.DefineInApplicationSource = false;
			Assert.That (vm.DefineInApplication, Is.True);
			Assert.That (vm.DefineInDocument, Is.False);
			Assert.That (vm.DefineInApplicationSource, Is.False);
			Assert.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assert.That (selectedSourceChanged, Is.True);
			Assert.That (inAppChanged, Is.True);
			Assert.That (inDictChanged, Is.True);
		}

		[Test]
		public void CreateResourceCommand ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.IsKeyed, Is.True);
			Assume.That (vm.ResourceKey, Is.Null);
			Assume.That (vm.DefineInApplication, Is.True);

			Assert.That (vm.CreateResourceCommand.CanExecute (null), Is.False);

			bool changed = false;
			vm.CreateResourceCommand.CanExecuteChanged += (o, e) => {
				changed = true;
			};

			vm.ResourceKey = "name";

			Assert.That (changed, Is.True);
			Assert.That (vm.CreateResourceCommand.CanExecute (null), Is.True);

			bool create = false;
			vm.CreateResource += (o, e) => {
				create = true;
			};

			vm.CreateResourceCommand.Execute (null);
			Assert.That (create, Is.True);
		}

		[Test]
		public void NameError ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);
			provider.Setup (p => p.CheckNameErrorsAsync (target, It.IsAny<ResourceSource> (), It.IsAny<string> ()))
				.ReturnsAsync (new ResourceCreateError ("failed", false));

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			bool changed = false;
			vm.ErrorsChanged += (sender, args) => {
				if (args.PropertyName == nameof(CreateResourceViewModel.ResourceKey))
					changed = true;
			};

			bool hasErrorsChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(CreateResourceViewModel.HasErrors))
					hasErrorsChanged = true;
			};

			vm.ResourceKey = "Test";
			Assert.That (changed, Is.True, "ErrorsChanged did not fire");
			Assert.That (vm.HasErrors, Is.True);
			Assert.That (hasErrorsChanged, Is.True, "HasErrors did not change");
			Assert.That (vm.GetErrors (nameof(CreateResourceViewModel.ResourceKey)), Contains.Item ("failed"));
		}

		[Test]
		public void ErrorCheckOnSourceChange ()
		{
			const string error = "failed";
			const string invalidName = "test";
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);
			provider.Setup (p => p.CheckNameErrorsAsync (target, It.Is<ResourceSource> (r => r == Sources[0]), invalidName))
				.ReturnsAsync (new ResourceCreateError (error, false));

			provider.Setup (p => p.CheckNameErrorsAsync (target, It.Is<ResourceSource> (r => r == Sources[1] || r == Sources[2]), invalidName))
				.ReturnsAsync ((ResourceCreateError)null);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.SelectedResourceSource, Is.EqualTo (Sources[0]));
			Assume.That (vm.HasErrors, Is.False);

			bool errorsChanged = false;
			vm.ErrorsChanged += (sender, args) => {
				if (args.PropertyName == nameof(CreateResourceViewModel.ResourceKey))
					errorsChanged = true;
			};

			bool hasErrorsChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(CreateResourceViewModel.HasErrors))
					hasErrorsChanged = true;
			};

			vm.ResourceKey = invalidName;
			Assume.That (vm.HasErrors, Is.True);
			Assume.That (hasErrorsChanged, Is.True, "HasErrors did not change");
			Assume.That (errorsChanged, Is.True, "ErrorsChanged did not fire");
			hasErrorsChanged = errorsChanged = false;

			vm.DefineInApplicationSource = true;
			provider.Verify (r => r.CheckNameErrorsAsync (target, It.IsAny<ResourceSource> (), invalidName), Times.AtLeast (2));
			Assert.That (vm.HasErrors, Is.False);
			Assert.That (hasErrorsChanged, Is.True, "HasErrors did not change");
			Assert.That (errorsChanged, Is.True, "ErrorsChanged did not fire");
		}

		[Test]
		public void DoesntErrorCheckBeforeSourceSet ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);
			provider.Setup (p =>
					p.SuggestResourceNameAsync (It.IsAny<IReadOnlyCollection<object>> (), It.IsAny<IPropertyInfo> ()))
				.ReturnsAsync ("suggested");

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			provider.Verify (r => r.CheckNameErrorsAsync (target, null, It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void NameErrorCleared ()
		{
			const string error = "failed";
			const string invalidName = "test";
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);
			provider.Setup (p => p.CheckNameErrorsAsync (target, It.IsAny<ResourceSource> (), invalidName))
				.ReturnsAsync (new ResourceCreateError (error, false));
			provider.Setup (p => p.CheckNameErrorsAsync (target, It.IsAny<ResourceSource> (), It.IsNotIn (invalidName)))
				.ReturnsAsync ((ResourceCreateError)null);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			bool errorsChanged = false;
			vm.ErrorsChanged += (sender, args) => {
				if (args.PropertyName == nameof(CreateResourceViewModel.ResourceKey))
					errorsChanged = true;
			};

			bool hasErrorsChanged = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(CreateResourceViewModel.HasErrors))
					hasErrorsChanged = true;
			};

			vm.ResourceKey = invalidName;
			Assume.That (vm.HasErrors, Is.True);
			Assume.That (hasErrorsChanged, Is.True, "HasErrors did not change");
			Assume.That (errorsChanged, Is.True, "ErrorsChanged did not fire");
			Assume.That (vm.GetErrors (nameof(CreateResourceViewModel.ResourceKey)), Contains.Item (error));
			errorsChanged = false;
			hasErrorsChanged = false;

			vm.ResourceKey = "validName";
			Assert.That (vm.HasErrors, Is.False);
			Assert.That (hasErrorsChanged, Is.True, "HasErrors did not change");
			Assert.That (errorsChanged, Is.True, "ErrorsChanged did not fire");
			Assert.That (vm.GetErrors (nameof(CreateResourceViewModel.ResourceKey)), Does.Not.Contain (error));
		}

		[Test]
		public void SuggestedName ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var tcs = new TaskCompletionSource<string> ();
			provider.Setup (p => p.SuggestResourceNameAsync (It.IsAny<IReadOnlyList<object>> (), property.Object))
				.Returns (tcs.Task);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.ResourceKey, Is.Null);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.ResourceKey))
					changed = true;
			};

			const string suggested = "suggested";
			tcs.SetResult (suggested);
			Assert.That (changed, Is.True, "ResourceKey did not change");
			Assert.That (vm.ResourceKey, Is.EqualTo (suggested));
		}

		[Test]
		public void SuggestedNameAfterChanged ()
		{
			var target = new object();
			var property = new Mock<IPropertyInfo>();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var provider = new Mock<IResourceProvider>();
			provider.Setup (p => p.GetResourceSourcesAsync (target, property.Object)).ReturnsAsync (Sources);

			var tcs = new TaskCompletionSource<string> ();
			provider.Setup (p => p.SuggestResourceNameAsync (It.IsAny<IReadOnlyList<object>> (), property.Object))
				.Returns (tcs.Task);

			var vm = new CreateResourceViewModel (provider.Object, new[] { target }, property.Object);
			Assume.That (vm.ResourceKey, Is.Null);

			const string userKey = "Test";
			vm.ResourceKey = userKey;

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateResourceViewModel.ResourceKey))
					changed = true;
			};

			tcs.SetResult ("suggested");
			Assert.That (changed, Is.False, "ResourceKey changed when suggested completed");
			Assert.That (vm.ResourceKey, Is.EqualTo (userKey), "ResourceKey changed from user set key after suggested");
		}

		private static readonly ResourceSource[] Sources = new[] {
			new ResourceSource ("app", ResourceSourceType.Application),
			new ResourceSource ("Resources.xaml", ResourceSourceType.ResourceDictionary),
			new ResourceSource ("MyStyles.xaml", ResourceSourceType.ResourceDictionary), 
			new ResourceSource ("Page", ResourceSourceType.Document),
			new ResourceSource ("Grid", ResourceSourceType.Document),
			new ResourceSource ("system", ResourceSourceType.System),
		};
	}
}
