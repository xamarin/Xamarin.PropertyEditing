using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class CreateBindingViewModelTests
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
		public void PropertyDisplayType ()
		{
			var target = new object();

			const string propName = "propertyName";
			var property = GetBasicProperty (propName);

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new[] { property.Object });
			editor.SetupGet (e => e.Target).Returns (target);
			editor.SetupGet (e => e.TargetType).Returns (typeof(object).ToTypeInfo ());
			
			var editorProvider = new MockEditorProvider (editor.Object);

			var bpmock = new Mock<IBindingProvider> ();

			var vm = new CreateBindingViewModel (new TargetPlatform (editorProvider, bpmock.Object), editor.Object, property.Object);
			Assert.That (vm.PropertyDisplay, Contains.Substring ("Object"));
			Assert.That (vm.PropertyDisplay, Contains.Substring (propName));
		}

		[Test]
		public void PropertyDisplayNameable ()
		{
			var target = new object ();
			const string propName = "propertyName";
			var property = GetBasicProperty (propName);
			var editor = GetBasicEditor (target, property.Object);

			const string objName = "objName";
			var nameable = editor.As<INameableObject> ();
			nameable.Setup (n => n.GetNameAsync ()).ReturnsAsync (objName);

			var editorProvider = new MockEditorProvider (editor.Object);

			var bpmock = new Mock<IBindingProvider> ();

			var vm = new CreateBindingViewModel (new TargetPlatform (editorProvider, bpmock.Object), editor.Object, property.Object);
			Assert.That (vm.PropertyDisplay, Does.Not.Contains ("Object"));
			Assert.That (vm.PropertyDisplay, Contains.Substring (objName));
			Assert.That (vm.PropertyDisplay, Contains.Substring (propName));
		}

		[Test]
		public async Task BindingSources ()
		{
			var target = new object ();
			var property = GetBasicProperty ();
			var editor = GetBasicEditor (target, property.Object);

			var editorProvider = new MockEditorProvider (editor.Object);

			var sources = new[] {
				new BindingSource ("Short Description", BindingSourceType.Object, "Short Description"),
				new BindingSource ("Long Description", BindingSourceType.Object, "Long Description"),
			};

			var bpmock = new Mock<IBindingProvider> ();
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (sources);
			bpmock.Setup (bp => bp.GetRootElementsAsync (sources[0], target)).ReturnsAsync (new[] { new object (), new object () });
			bpmock.Setup (bp => bp.GetRootElementsAsync (sources[1], target)).ReturnsAsync (new[] { new object () });
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new Resource[0]);

			var vm = new CreateBindingViewModel (new TargetPlatform (editorProvider, bpmock.Object), editor.Object, property.Object);

			Assert.That (vm.BindingSources, Is.Not.Null);

			var requested = await vm.BindingSources.Task;
			CollectionAssert.AreEqual (sources, requested);

			Assert.That (vm.SelectedBindingSource, Is.EqualTo (sources[0]));
		}

		[Test]
		public async Task BindingSourceObjectRoots ()
		{
			var target = new object ();
			var property = GetBasicProperty ();
			var editor = GetBasicEditor (target, property.Object);

			var editorProvider = new MockEditorProvider (editor.Object);

			var sources = new[] {
				new BindingSource ("Short Description", BindingSourceType.Object, "Short Description"),
				new BindingSource ("Long Description", BindingSourceType.Object, "Long Description"),
			};

			var shortRoots = new[] { new object (), new object () };

			var bpmock = new Mock<IBindingProvider> ();
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (sources);
			bpmock.Setup (bp => bp.GetRootElementsAsync (sources[0], target)).ReturnsAsync (shortRoots);
			bpmock.Setup (bp => bp.GetRootElementsAsync (sources[1], target)).ReturnsAsync (new[] { new object () });
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new Resource[0]);

			var vm = new CreateBindingViewModel (new TargetPlatform (editorProvider, bpmock.Object), editor.Object, property.Object);
			await vm.BindingSources.Task;
			Assume.That (vm.SelectedBindingSource, Is.EqualTo (sources[0]));

			bpmock.Verify (bp => bp.GetRootElementsAsync (sources[0], target));
			IReadOnlyList<ObjectTreeElement> roots = await vm.ObjectElementRoots.Task;
			Assert.That (roots.Count, Is.EqualTo (2), "Unexpected number of roots");
			CollectionAssert.AreEqual (roots.Select (r => r.Editor.Target), shortRoots);
		}

		[Test]
		[Description ("If the source has a description, we're on object type source and it's the only one, use a long description")]
		public async Task ShowLongDescription ()
		{
			var sources = new[] {
				new BindingSource ("Short Description", BindingSourceType.Object, "Short Description"),
				new BindingSource ("Long Description", BindingSourceType.SingleObject, "Long Description"),
			};
			var vm = CreateBasicViewModel (sources: sources);
			await vm.BindingSources.Task;

			Assume.That (vm.SelectedBindingSource, Is.EqualTo (sources[0]));
			Assume.That (vm.ShowObjectSelector, Is.True, "Object selector should be showing");
			Assert.That (vm.ShowLongDescription, Is.False);

			vm.SelectedBindingSource = sources[1];
			await vm.ObjectElementRoots.Task;
			Assert.That (vm.ShowLongDescription, Is.True);
			Assert.That (vm.ShowObjectSelector, Is.False);
		}

		[Test]
		public async Task ValueConverters ()
		{
			var target = new object ();

			var property = GetBasicProperty ();
			var editor = GetBasicEditor (target, property.Object);

			const string objName = "objName";
			var nameable = editor.As<INameableObject> ();
			nameable.Setup (n => n.GetNameAsync ()).ReturnsAsync (objName);

			var editorProvider = new MockEditorProvider (editor.Object);

			var sources = new[] {
				new BindingSource ("Short Description", BindingSourceType.Object, "Short Description"),
				new BindingSource ("Long Description", BindingSourceType.Object, "Long Description"),
			};

			var visi = new Resource ("BooleanToVisibilityConverter");

			var bpmock = new Mock<IBindingProvider> ();
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (sources);
			bpmock.Setup (bp => bp.GetRootElementsAsync (sources[0], target)).ReturnsAsync (new[] { new object (), new object () });
			bpmock.Setup (bp => bp.GetRootElementsAsync (sources[1], target)).ReturnsAsync (new[] { new object () });
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new [] { visi });

			var vm = new CreateBindingViewModel (new TargetPlatform (editorProvider, bpmock.Object), editor.Object, property.Object);
			Assert.That (vm.ValueConverters, Is.Not.Null);

			await vm.ValueConverters.Task;
			Assert.That (vm.ValueConverters.Value, Contains.Item (visi));
			Assert.That (vm.ValueConverters.Value.Count, Is.EqualTo (3)); // visi, No Converter, Request Converter
		}

		[Test]
		public async Task RequestValueConverter ()
		{
			var target = new object();

			var vm = CreateBasicViewModel (target: target);
			Assume.That (vm.ValueConverters, Is.Not.Null);

			await vm.ValueConverters.Task;

			Assume.That (vm.SelectedValueConverter, Is.EqualTo (vm.ValueConverters.Value[0]));
			Assume.That (vm.ValueConverters.Value.Count, Is.EqualTo (2));

			const string name = "NewConverter";
			bool requested = false;
			vm.CreateValueConverterRequested += (o, e) => {
				e.ConverterType = typeof(object).ToTypeInfo ();
				e.Name = name;
				e.Source = MockResourceProvider.ApplicationResourcesSource;
				requested = true;
			};

			Assume.That (vm.ValueConverters.Value, Is.InstanceOf (typeof(INotifyCollectionChanged)));

			object newItem = null;
			((INotifyCollectionChanged) vm.ValueConverters.Value).CollectionChanged += (o, e) => {
				if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count == 1) {
					newItem = e.NewItems[0];
					Assert.That (e.NewStartingIndex, Is.EqualTo (1), "New converter was not added below none and above request");
				}
			};

			int selectedChanged = 0;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateBindingViewModel.SelectedValueConverter))
					selectedChanged++;
			};

			vm.SelectedValueConverter = vm.ValueConverters.Value[1];

			Assert.That (requested, Is.True);
			Assert.That (selectedChanged, Is.EqualTo (2), "SelectedValueConverter did not fire INPC for request and result changes");
			Assert.That (newItem, Is.Not.Null);
			Assert.That (vm.SelectedValueConverter, Is.EqualTo (newItem));
		}

		[Test]
		public async Task RequestValueConverterCanceled ()
		{
			var vm = CreateBasicViewModel ();
			Assume.That (vm.ValueConverters, Is.Not.Null);

			await vm.ValueConverters.Task;

			Assume.That (vm.SelectedValueConverter, Is.EqualTo (vm.ValueConverters.Value[0]));
			Assume.That (vm.ValueConverters.Value.Count, Is.EqualTo (2));

			bool requested = false;
			vm.CreateValueConverterRequested += (o, e) => {
				requested = true;
			};

			vm.SelectedValueConverter = vm.ValueConverters.Value[1];

			Assert.That (requested, Is.True);
			Assert.That (vm.SelectedValueConverter, Is.EqualTo (vm.ValueConverters.Value[0]),
				"SelectedValueConverter wasn't set back to No Converter after canceled request");
		}

		[Test]
		public async Task PropertyRoot ()
		{
			var target = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Local | ValueSources.Binding);
			property.SetupGet (p => p.Type).Returns (typeof (object));
			property.SetupGet (p => p.Name).Returns ("name");
			property.SetupGet (p => p.RealType).Returns (typeof (object).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);

			var editor = GetBasicEditor (target, property.Object);

			var controlTarget = new MockWpfControl ();
			var controlEditor = new MockObjectEditor (controlTarget);
			var provider = new MockEditorProvider (controlEditor);

			var source = new BindingSource ("Control", BindingSourceType.Object);
			var bpmock = new Mock<IBindingProvider> ();
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (new[] { source });
			bpmock.Setup (bp => bp.GetRootElementsAsync (source, target)).ReturnsAsync (new[] { controlTarget });
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new Resource[0]);

			var vm = new CreateBindingViewModel (new TargetPlatform (provider, bpmock.Object), editor.Object, property.Object);
			Assume.That (vm.SelectedBindingSource, Is.EqualTo (source));

			Assert.That (vm.PropertyRoot, Is.Not.Null);
			await vm.PropertyRoot.Task;

			var targetType = typeof(MockWpfControl).ToTypeInfo ();
			Assert.That (vm.PropertyRoot.Value.TargetType, Is.EqualTo (targetType));
			CollectionAssert.AreEqual (controlEditor.Properties, vm.PropertyRoot.Value.Children.Select (te => te.Property));
		}

		[Test]
		public async Task PropertyRootChildren ()
		{
			var target = new object();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Local | ValueSources.Binding);
			property.SetupGet (p => p.Type).Returns (typeof (object));
			property.SetupGet (p => p.Name).Returns ("name");
			property.SetupGet (p => p.RealType).Returns (typeof (object).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);

			var editor = GetBasicEditor (target, property.Object);

			var controlTarget = new MockWpfControl();
			var controlEditor = new MockObjectEditor (controlTarget);
			var provider = new MockEditorProvider (controlEditor);

			var source = new BindingSource ("Control", BindingSourceType.Object);
			var bpmock = new Mock<IBindingProvider> ();
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (new[] { source });
			bpmock.Setup (bp => bp.GetRootElementsAsync (source, target)).ReturnsAsync (new[] { controlTarget });
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new Resource[0]);

			var vm = new CreateBindingViewModel (new TargetPlatform (provider, bpmock.Object), editor.Object, property.Object);
			Assume.That (vm.SelectedBindingSource, Is.EqualTo (source));
			Assume.That (vm.PropertyRoot, Is.Not.Null);
			await vm.PropertyRoot.Task;

			var childrenProperty = controlEditor.Properties.First (p => p.Type == typeof(CommonThickness));
			var element = vm.PropertyRoot.Value.Children.First (p => Equals (p.Property, childrenProperty));

			Assert.That (element.Children, Is.Not.Null);

			await element.Children.Task;
			var expected = await provider.GetPropertiesForTypeAsync (typeof(CommonThickness).ToTypeInfo ());
			CollectionAssert.AreEqual (expected, element.Children.Value.Select (te => te.Property));
		}

		[Test]
		public async Task Path ()
		{
			var target = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Local | ValueSources.Binding);
			property.SetupGet (p => p.Type).Returns (typeof (object));
			property.SetupGet (p => p.Name).Returns ("name");
			property.SetupGet (p => p.RealType).Returns (typeof (object).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);

			var editor = GetBasicEditor (target, property.Object);

			var controlTarget = new MockWpfControl ();
			var controlEditor = new MockObjectEditor (controlTarget);
			var provider = new MockEditorProvider (controlEditor);

			var source = new BindingSource ("Control", BindingSourceType.Object);
			var bpmock = new Mock<IBindingProvider> ();
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (new[] { source });
			bpmock.Setup (bp => bp.GetRootElementsAsync (source, target)).ReturnsAsync (new[] { controlTarget });
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new Resource[0]);

			var vm = new CreateBindingViewModel (new TargetPlatform (provider, bpmock.Object), editor.Object, property.Object);
			Assume.That (vm.SelectedBindingSource, Is.EqualTo (source));
			Assume.That (vm.PropertyRoot, Is.Not.Null);
			await vm.PropertyRoot.Task;

			var element = vm.PropertyRoot.Value.Children.First (p => p.Property.Type == typeof(CommonThickness));
			await element.Children.Task;
			var sub = element.Children.Value.First ();
			vm.SelectedPropertyElement = sub;

			Assert.That (vm.Path, Is.EqualTo ($"{element.Property.Name}.{sub.Property.Name}"));
		}

		[Test]
		public async Task SelectedBindingSource ()
		{
			BindingSource[] sources = new[] {
				new BindingSource ("First", BindingSourceType.Object),
				new BindingSource ("Second", BindingSourceType.Resource),
				new BindingSource ("Third", BindingSourceType.Type),
			};

			var vm = CreateBasicViewModel (sources);

			while (vm.SelectedObjects.Count == 0) {
				await Task.Delay (1);
			}

			var binding = (MockBinding)vm.SelectedObjects.First ();

			Assert.That (vm.SelectedBindingSource, Is.EqualTo (sources[0]));
			Assert.That (binding.Source, Is.EqualTo (sources[0]), "Backing binding object property didn't update");

			bool propertyChanged = false, objectRootsChanged = false, resourceRootsChanged = false, typeRootsChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateBindingViewModel.SelectedBindingSource))
					propertyChanged = true;
				else if (e.PropertyName == nameof(CreateBindingViewModel.ObjectElementRoots))
					objectRootsChanged = true;
				else if (e.PropertyName == nameof(CreateBindingViewModel.SourceResources))
					resourceRootsChanged = true;
				else if (e.PropertyName == nameof(CreateBindingViewModel.TypeSelector))
					typeRootsChanged = true;
			};

			vm.SelectedBindingSource = sources[1];
			Assert.That (propertyChanged, Is.True, "INPC did not fire for SelectedBindingSource");
			Assert.That (binding.Source, Is.EqualTo (sources[1]), "Backing binding object property didn't update");
			Assert.That (resourceRootsChanged, Is.True, "SourceResources did not update when selected");
			Assert.That (vm.SourceResources, Is.Not.Null);
			Assert.That (objectRootsChanged, Is.False);
			Assert.That (typeRootsChanged, Is.False);

			propertyChanged = objectRootsChanged = resourceRootsChanged = typeRootsChanged = false;
			vm.SelectedBindingSource = sources[2];
			Assert.That (propertyChanged, Is.True, "INPC did not fire for SelectedBindingSource");
			Assert.That (binding.Source, Is.EqualTo (sources[2]), "Backing binding object property didn't update");
			Assert.That (resourceRootsChanged, Is.False);
			Assert.That (objectRootsChanged, Is.False);
			Assert.That (typeRootsChanged, Is.True, "TypeSelector didn't update when selected");
			Assert.That (vm.TypeSelector, Is.Not.Null);

			propertyChanged = objectRootsChanged = resourceRootsChanged = typeRootsChanged = false;
			vm.SelectedBindingSource = sources[0];
			Assert.That (propertyChanged, Is.True, "INPC did not fire for SelectedBindingSource");
			Assert.That (binding.Source, Is.EqualTo (sources[0]), "Backing binding object property didn't update");
			Assert.That (resourceRootsChanged, Is.False);
			Assert.That (objectRootsChanged, Is.True, "ObjectElementRoots didn't update when selected");
			Assert.That (vm.ObjectElementRoots, Is.Not.Null);
			Assert.That (typeRootsChanged, Is.False);
		}

		[Test]
		public async Task ShowSourceParameterSelectors ()
		{
			BindingSource[] sources = new[] {
				new BindingSource ("First", BindingSourceType.Object),
				new BindingSource ("Second", BindingSourceType.Resource),
				new BindingSource ("Third", BindingSourceType.Type),
			};

			var vm = CreateBasicViewModel (sources);

			while (vm.SelectedObjects.Count == 0) {
				await Task.Delay (1);
			}

			Assume.That (vm.SelectedBindingSource, Is.EqualTo (sources[0]));

			bool objectChanged = false, resourceChanged = false, typeChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (CreateBindingViewModel.ShowObjectSelector))
					objectChanged = true;
				else if (e.PropertyName == nameof (CreateBindingViewModel.ShowResourceSelector))
					resourceChanged = true;
				else if (e.PropertyName == nameof (CreateBindingViewModel.ShowTypeSelector))
					typeChanged = true;
			};

			vm.SelectedBindingSource = sources[1];
			Assert.That (resourceChanged, Is.True);
			Assert.That (vm.ShowResourceSelector, Is.True);
			Assert.That (objectChanged, Is.True, "Did not signal old selector changed");
			Assert.That (vm.ShowObjectSelector, Is.False);
			Assert.That (vm.ShowTypeSelector, Is.False);

			objectChanged = resourceChanged = typeChanged = false;
			vm.SelectedBindingSource = sources[2];
			Assert.That (resourceChanged, Is.True);
			Assert.That (vm.ShowResourceSelector, Is.False);
			Assert.That (objectChanged, Is.True, "Did not signal old selector changed");
			Assert.That (vm.ShowObjectSelector, Is.False);
			Assert.That (vm.ShowTypeSelector, Is.True);

			objectChanged = resourceChanged = typeChanged = false;
			vm.SelectedBindingSource = sources[0];
			Assert.That (vm.ShowResourceSelector, Is.False);
			Assert.That (objectChanged, Is.True);
			Assert.That (vm.ShowObjectSelector, Is.True);
			Assert.That (typeChanged, Is.True, "Did not signal old selector changed");
			Assert.That (vm.ShowTypeSelector, Is.False);
		}

		[Test]
		public async Task ResourceRoots ()
		{
			object target = new object();
			var property = GetBasicProperty ();
			var editor = GetBasicEditor (target, property.Object);
			var resources = new MockResourceProvider ();
			var bindings = GetBasicBindingProvider (target, property.Object);
			var source = new BindingSource ("Resources", BindingSourceType.Resource);
			bindings.Setup (bp => bp.GetBindingSourcesAsync (target, property.Object)).ReturnsAsync (new[] { source });
			bindings.Setup (bp => bp.GetResourcesAsync (source, target))
				.Returns<BindingSource,object> (async (bs, t) => {
					var rs = await resources.GetResourcesAsync (target, CancellationToken.None);
					return rs.ToLookup (r => r.Source);
				});

			var vm = new CreateBindingViewModel (
				new TargetPlatform (new MockEditorProvider (editor.Object), resources, bindings.Object), editor.Object,
				property.Object);

			Assume.That (vm.SelectedBindingSource, Is.EqualTo (source));

			Assert.That (vm.SourceResources, Is.Not.Null);
			await vm.SourceResources.Task;
			Assert.That (vm.SourceResources.Value.First().Key, Is.EqualTo (DefaultResourceSources[0]));
		}

		[Test]
		public async Task ResourceProperties ()
		{
			object target = new object ();
			var property = GetBasicProperty ();
			var editor = GetBasicEditor (target, property.Object);
			var resources = new MockResourceProvider ();
			var source = new BindingSource ("Resources", BindingSourceType.Resource);
			var bindings = GetBasicBindingProvider (target, property.Object, sources: new [] { source });
			bindings.Setup (bp => bp.GetResourcesAsync (source, target))
				.Returns<BindingSource, object> (async (bs, t) => {
					var rs = await resources.GetResourcesAsync (target, CancellationToken.None);
					return rs.ToLookup (r => r.Source);
				});

			var vm = new CreateBindingViewModel (
				new TargetPlatform (new MockEditorProvider (editor.Object), resources, bindings.Object), editor.Object,
				property.Object);

			Assume.That (vm.SelectedBindingSource, Is.EqualTo (source));
			Assume.That (vm.SourceResources, Is.Not.Null);
			await vm.SourceResources.Task;

			while (vm.SelectedObjects.Count == 0) {
				await Task.Delay (1);
			}

			var binding = (MockBinding)vm.SelectedObjects.First();
			vm.SelectedResource = vm.SourceResources.Value.First ().OfType<Resource<CommonSolidBrush>>().First ();
			Assert.That (binding.SourceParameter, Is.EqualTo (vm.SelectedResource));
			Assume.That (vm.PropertyRoot, Is.Not.Null);
			await vm.PropertyRoot.Task;

			Assert.That (vm.PropertyRoot.Value.TargetType, Is.EqualTo (typeof(CommonSolidBrush).ToTypeInfo ()));
			CollectionAssert.AreEqual (ReflectionEditorProvider.GetPropertiesForType (typeof(CommonSolidBrush)),
				vm.PropertyRoot.Value.Children.Select (pe => pe.Property));
		}

		[Test]
		public async Task Types ()
		{
			object target = new object ();
			var property = GetBasicProperty ();
			var editor = GetBasicEditor (target, property.Object);
			var resources = new MockResourceProvider ();
			var source = new BindingSource ("Resources", BindingSourceType.Type);

			var type = typeof(MockSampleControl).ToTypeInfo ();
			var bindings = GetBasicBindingProvider (target, property.Object, sources: new[] { source });
			bindings.Setup (bp => bp.GetSourceTypesAsync (source, target))
				.ReturnsAsync (new AssignableTypesResult (new[] { type }));

			var provider = new MockEditorProvider (editor.Object);
			var vm = new CreateBindingViewModel (new TargetPlatform (provider, resources, bindings.Object), editor.Object, property.Object);

			Assume.That (vm.SelectedBindingSource, Is.EqualTo (source));
			Assume.That (vm.TypeSelector, Is.Not.Null);

			while (vm.SelectedObjects.Count == 0 && vm.TypeSelector.IsLoading) {
				await Task.Delay (1);
			}

			var binding = (MockBinding) vm.SelectedObjects.First ();

			Assert.That (vm.TypeSelector.SelectedType, Is.Null);

			bool propertyChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(CreateBindingViewModel.PropertyRoot))
					propertyChanged = true;
			};

			vm.TypeSelector.SelectedType = type;
			Assert.That (propertyChanged, Is.True, "INPC didn't change for PropertyRoot on selected source param");
			Assert.That (vm.PropertyRoot, Is.Not.Null);
			Assert.That (binding.SourceParameter, Is.EqualTo (type));

			await vm.PropertyRoot.Task;
			CollectionAssert.AreEqual (provider.GetPropertiesForTypeAsync (type).Result,
				vm.PropertyRoot.Value.Children.Select (pe => pe.Property));
		}

		[Test]
		public async Task ExtraProperties ()
		{
			var provider = new MockEditorProvider();
			var vm = CreateBasicViewModel ();

			var editors = (IList<IObjectEditor>)typeof(CreateBindingViewModel).GetProperty ("ObjectEditors", BindingFlags.NonPublic | BindingFlags.Instance)
				.GetValue (vm); // Shortcut
			var editor = editors[0];

			IEnumerable<IPropertyInfo> properties = (await provider.GetPropertiesForTypeAsync (typeof(MockBinding).ToTypeInfo ()));
			CollectionAssert.AreEqual (properties, vm.Properties.Cast<PropertyViewModel> ().Select (pvm => pvm.Property));

			properties = properties.Where (p => !editor.KnownProperties.ContainsKey (p));
			CollectionAssert.AreEqual (properties.Where (p => p.Type == typeof(bool)),
				vm.FlagsProperties.Cast<PropertyViewModel> ().Select (pvm => pvm.Property));
			CollectionAssert.AreEqual (properties.Where (p => p.Type != typeof (bool)),
				vm.BindingProperties.Cast<PropertyViewModel> ().Select (pvm => pvm.Property));
		}

		private TestContext syncContext;
		private static readonly ResourceSource[] DefaultResourceSources = new[] { MockResourceProvider.SystemResourcesSource, MockResourceProvider.ApplicationResourcesSource };

		private Mock<IPropertyInfo> GetBasicProperty (string name = "propertyName")
		{
			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Local | ValueSources.Binding);
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.Name).Returns (name);
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);

			return property;
		}

		private Mock<IObjectEditor> GetBasicEditor (object target, IPropertyInfo property)
		{
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new[] { property });
			editor.SetupGet (e => e.Target).Returns (target);
			editor.SetupGet (e => e.TargetType).Returns (typeof (object).ToTypeInfo ());

			return editor;
		}

		private Mock<IResourceProvider> GetBasicResourceProvider (object target, ResourceSource[] sources = null)
		{
			sources = sources ?? DefaultResourceSources;

			var resources = new Mock<IResourceProvider> ();
			resources.Setup (r => r.GetResourceSourcesAsync (target)).ReturnsAsync (sources);
			resources.Setup (r => r.CreateResourceAsync (It.IsAny<ResourceSource> (), It.IsAny<string> (), It.IsAny<object> ()))
				.ReturnsAsync ((Func<ResourceSource, string, object, Resource>) ((s, n, v) => new Resource (s, n)));
			return resources;
		}

		private Mock<IBindingProvider> GetBasicBindingProvider (object target, IPropertyInfo property, BindingSource[] sources = null)
		{
			var bpmock = new Mock<IBindingProvider> ();

			if (sources == null) {
				sources = new[] {
					new BindingSource ("Short Description", BindingSourceType.Object, "Short Description"),
					new BindingSource ("Long Description", BindingSourceType.SingleObject, "Long Description"),
				};

				bpmock.Setup (bp => bp.GetRootElementsAsync (sources[0], target)).ReturnsAsync (new[] { new object (), new object () });
				bpmock.Setup (bp => bp.GetRootElementsAsync (sources[1], target)).ReturnsAsync (new[] { new object () });
			} else {
				for (int i = 0; i < sources.Length; i++) {
					int index = i;
					if (sources[i].Type == BindingSourceType.SingleObject)
						bpmock.Setup (bp => bp.GetRootElementsAsync (sources[index], target)).ReturnsAsync (new[] { new object () });
					else
						bpmock.Setup (bp => bp.GetRootElementsAsync (sources[index], target)).ReturnsAsync (new[] { new object (), new object() });
				}
			}
			
			bpmock.Setup (bp => bp.GetBindingSourcesAsync (target, property)).ReturnsAsync (sources);
			bpmock.Setup (bp => bp.GetValueConverterResourcesAsync (It.IsAny<object> ())).ReturnsAsync (new Resource[0]);
			return bpmock;
		}

		private CreateBindingViewModel CreateBasicViewModel (BindingSource[] sources = null, object target = null)
		{
			target = target ?? new object ();
			Mock<IPropertyInfo> property = GetBasicProperty ();
			Mock<IObjectEditor> editor = GetBasicEditor (target, property.Object);

			var editorProvider = new MockEditorProvider (editor.Object);
			Mock<IResourceProvider> resourceProvider = GetBasicResourceProvider (target);
			Mock<IBindingProvider> bpmock = GetBasicBindingProvider (target, property.Object, sources);

			return new CreateBindingViewModel (new TargetPlatform (editorProvider, resourceProvider.Object, bpmock.Object), editor.Object, property.Object);
		}
	}
}