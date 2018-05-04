using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class PanelViewModelTests
		: PropertiesViewModelTests<PanelViewModel>
	{
		[SetUp]
		public void Setup ()
		{
			SynchronizationContext.SetSynchronizationContext (this.context = new TestContext ());
		}

		[Test]
		public async Task PropertiesAddedFromEditor ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClass ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			var vm = new PanelViewModel (new TargetPlatform (provider));
			vm.SelectedObjects.Add (obj);

			Assert.That (vm.Properties, Is.Not.Empty);
			Assert.That (((PropertyViewModel)vm.Properties[0]).Property, Is.EqualTo (editor.Properties.Single ()));
		}

		[Test]
		[Description ("When editors of two different types are selected, the properties that are common should be listed")]
		public void PropertiesFromCommonSubset ()
		{
			var obj1 = new TestClass ();
			var obj2 = new TestClassSub ();

			var sharedPropertyMock = new Mock<IPropertyInfo> ();
			sharedPropertyMock.SetupGet (pi => pi.Type).Returns (typeof (string));
			var subPropertyMock = new Mock<IPropertyInfo> ();
			subPropertyMock.SetupGet (pi => pi.Type).Returns (typeof (int));

			var editor1Mock = new Mock<IObjectEditor> ();
			editor1Mock.SetupGet (oe => oe.Properties).Returns (new[] { sharedPropertyMock.Object });
			var editor2Mock = new Mock<IObjectEditor> ();
			editor2Mock.SetupGet (oe => oe.Properties).Returns (new[] { sharedPropertyMock.Object, subPropertyMock.Object });

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj1)).ReturnsAsync (editor1Mock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2Mock.Object);

			var vm = new PanelViewModel (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.Add (obj1);

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (((PropertyViewModel)vm.Properties[0]).Property, Is.EqualTo (sharedPropertyMock.Object));

			// Reflection property info equate actually fails on the same property across class/subclass
			vm.SelectedObjects.Add (obj2);
			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (((PropertyViewModel)vm.Properties.Single()).Property, Is.EqualTo (sharedPropertyMock.Object));
		}

		[Test]
		[Description ("When editors of two different types are selected, the properties that are common should be listed")]
		public void PropertiesReducesToCommonSubset ()
		{
			var obj1 = new TestClass ();
			var obj2 = new TestClassSub ();

			var sharedPropertyMock = new Mock<IPropertyInfo> ();
			sharedPropertyMock.SetupGet (pi => pi.Type).Returns (typeof (string));
			var subPropertyMock = new Mock<IPropertyInfo> ();
			subPropertyMock.SetupGet (pi => pi.Type).Returns (typeof (int));

			var editor1Mock = new Mock<IObjectEditor> ();
			editor1Mock.SetupGet (oe => oe.Properties).Returns (new[] { sharedPropertyMock.Object });
			var editor2Mock = new Mock<IObjectEditor> ();
			editor2Mock.SetupGet (oe => oe.Properties).Returns (new[] { sharedPropertyMock.Object, subPropertyMock.Object });

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj1)).ReturnsAsync (editor1Mock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2Mock.Object);

			var vm = new PanelViewModel (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.Add (obj2);

			Assume.That (vm.Properties.Count, Is.EqualTo (2));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (sharedPropertyMock.Object));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (subPropertyMock.Object));

			// Reflection property info equate actually fails on the same property across class/subclass
			vm.SelectedObjects.Add (obj1);
			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (sharedPropertyMock.Object));
		}

		[Test]
		[Description ("Adding or removing editors shouldn't remake other editors or duplicate")]
		public void EditorsShouldBeConsistent ()
		{
			var provider = new ReflectionEditorProvider ();

			var obj1 = new TestClass ();
			var obj2 = new TestClass ();

			var vm = new PanelViewModel (new TargetPlatform (provider));
			vm.SelectedObjects.Add (obj1);

			var property = vm.Properties[0];
			Assume.That (property.Editors.Count, Is.EqualTo (1));

			var editor = property.Editors.Single ();

			vm.SelectedObjects.Add (obj2);

			Assume.That (property, Is.SameAs (vm.Properties[0]));
			Assert.That (property.Editors, Contains.Item (editor));
			Assert.That (property.Editors.Count, Is.EqualTo (2));
		}

		[Test]
		public void EditorRemovedWithSelectedObject ()
		{
			var provider = new ReflectionEditorProvider ();

			var obj1 = new TestClass ();
			var obj2 = new TestClass ();

			var vm = new PanelViewModel (new TargetPlatform (provider));
			vm.SelectedObjects.Add (obj1);
			vm.SelectedObjects.Add (obj2);

			var property = vm.Properties[0];
			var editor = property.Editors.Single (oe => oe.Target == obj1);
			Assume.That (property.Editors.Count, Is.EqualTo (2));
			Assume.That (vm.SelectedObjects.Remove (obj2));
			Assume.That (property, Is.SameAs (vm.Properties[0]));
			Assert.That (property.Editors, Contains.Item (editor));
			Assert.That (property.Editors.Count, Is.EqualTo (1));
		}


		[Test]
		public void PropertiesListItemRemoved ()
		{
			var mockProperty1 = new Mock<IPropertyInfo> ();
			mockProperty1.SetupGet (pi => pi.Type).Returns (typeof (string));

			var mockProperty2 = new Mock<IPropertyInfo> ();
			mockProperty2.SetupGet (pi => pi.Type).Returns (typeof (string));

			var properties = new ObservableCollection<IPropertyInfo> { mockProperty1.Object, mockProperty2.Object };
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Properties).Returns (properties);

			var obj = new object ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editorMock.Object);

			var vm = new PanelViewModel (new TargetPlatform (provider.Object));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.Properties.Count, Is.EqualTo (2));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty1.Object));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty2.Object));

			properties.Remove (mockProperty2.Object);
			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty1.Object));
		}

		[Test]
		public void PropertiesListItemAdded ()
		{
			var mockProperty1 = new Mock<IPropertyInfo> ();
			mockProperty1.SetupGet (pi => pi.Type).Returns (typeof (string));

			var mockProperty2 = new Mock<IPropertyInfo> ();
			mockProperty2.SetupGet (pi => pi.Type).Returns (typeof (string));

			var properties = new ObservableCollection<IPropertyInfo> { mockProperty1.Object };
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Properties).Returns (properties);

			var obj = new object ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editorMock.Object);

			var vm = new PanelViewModel (new TargetPlatform (provider.Object));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty1.Object));

			properties.Add (mockProperty2.Object);

			Assert.That (vm.Properties.Count, Is.EqualTo (2));
			Assert.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty1.Object));
			Assert.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty2.Object));
		}

		[Test]
		public void PropertiesListItemAddedWithReset ()
		{
			var mockProperty1 = new Mock<IPropertyInfo> ();
			mockProperty1.SetupGet (pi => pi.Type).Returns (typeof (string));

			var mockProperty2 = new Mock<IPropertyInfo> ();
			mockProperty2.SetupGet (pi => pi.Type).Returns (typeof (string));

			var properties = new ObservableCollection<IPropertyInfo> { mockProperty1.Object };
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Properties).Returns (properties);

			var obj = new object ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editorMock.Object);

			var vm = new PanelViewModel (new TargetPlatform (provider.Object));

			// We need access to the custom reset method here to ensure compliance
			// It's a bit hacky but this is unlikely to change. If it does, this test
			// will ensure the new notifier works as it should when resetting.
			Assume.That (vm.SelectedObjects, Is.TypeOf<ObservableCollectionEx<object>> ());
			((ObservableCollectionEx<object>)vm.SelectedObjects).Reset (new[] { obj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty1.Object));

			properties.Add (mockProperty2.Object);

			Assert.That (vm.Properties.Count, Is.EqualTo (2));
			Assert.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty1.Object));
			Assert.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (mockProperty2.Object));
		}

		[Test]
		public void PropertiesListItemRemovedJointList ()
		{
			var baseObj = new object ();
			var derivedObj = new object ();

			var baseProperty = new Mock<IPropertyInfo> ();
			baseProperty.SetupGet (pi => pi.Type).Returns (typeof (string));

			var baseProperties = new ObservableCollectionEx<IPropertyInfo> { baseProperty.Object };
			var derivedProperties = new ObservableCollectionEx<IPropertyInfo> { baseProperty.Object };

			var baseEditorMock = new Mock<IObjectEditor> ();
			baseEditorMock.SetupGet (e => e.Properties).Returns (baseProperties);
			baseEditorMock.SetupGet (e => e.Target).Returns (baseObj);

			var derivedEditorMock = new Mock<IObjectEditor> ();
			derivedEditorMock.SetupGet (e => e.Properties).Returns (derivedProperties);
			derivedEditorMock.SetupGet (e => e.Target).Returns (derivedObj);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (baseObj)).ReturnsAsync (baseEditorMock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (derivedObj)).ReturnsAsync (derivedEditorMock.Object);

			var vm = new PanelViewModel (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.AddItems (new[] { baseObj, derivedObj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (baseProperty.Object));

			derivedProperties.Remove (baseProperty.Object);
			Assert.That (vm.Properties, Is.Empty);
		}

		[Test]
		public void PropertiesListSelectedItemRemovedStopsListening ()
		{
			var baseObj = new object ();
			var derivedObj = new object ();

			var baseProperty = new Mock<IPropertyInfo> ();
			baseProperty.SetupGet (pi => pi.Type).Returns (typeof (string));

			var baseProperties = new ObservableCollectionEx<IPropertyInfo> { baseProperty.Object };
			var derivedProperties = new ObservableCollectionEx<IPropertyInfo> { baseProperty.Object };

			var baseEditorMock = new Mock<IObjectEditor> ();
			baseEditorMock.SetupGet (e => e.Properties).Returns (baseProperties);
			baseEditorMock.SetupGet (e => e.Target).Returns (baseObj);

			var derivedEditorMock = new Mock<IObjectEditor> ();
			derivedEditorMock.SetupGet (e => e.Properties).Returns (derivedProperties);
			derivedEditorMock.SetupGet (e => e.Target).Returns (derivedObj);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (baseObj)).ReturnsAsync (baseEditorMock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (derivedObj)).ReturnsAsync (derivedEditorMock.Object);

			var vm = new PanelViewModel (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.AddItems (new[] { baseObj, derivedObj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (baseProperty.Object));

			vm.SelectedObjects.Remove (derivedObj);
			Assume.That (vm.Properties, Is.Not.Empty);

			var changedField = typeof (ObservableCollection<IPropertyInfo>).GetField (nameof (INotifyCollectionChanged.CollectionChanged), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			MulticastDelegate d = (MulticastDelegate)changedField.GetValue (derivedProperties);
			Assert.That (d, Is.Null);
		}

		[Test]
		public void PropertiesListSelectedItemResetStopsListening ()
		{
			var baseObj = new object ();
			var derivedObj = new object ();

			var baseProperty = new Mock<IPropertyInfo> ();
			baseProperty.SetupGet (pi => pi.Type).Returns (typeof (string));

			var baseProperties = new ObservableCollectionEx<IPropertyInfo> { baseProperty.Object };
			var derivedProperties = new ObservableCollectionEx<IPropertyInfo> { baseProperty.Object };

			var baseEditorMock = new Mock<IObjectEditor> ();
			baseEditorMock.SetupGet (e => e.Properties).Returns (baseProperties);
			baseEditorMock.SetupGet (e => e.Target).Returns (baseObj);

			var derivedEditorMock = new Mock<IObjectEditor> ();
			derivedEditorMock.SetupGet (e => e.Properties).Returns (derivedProperties);
			derivedEditorMock.SetupGet (e => e.Target).Returns (derivedObj);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (baseObj)).ReturnsAsync (baseEditorMock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (derivedObj)).ReturnsAsync (derivedEditorMock.Object);

			var vm = new PanelViewModel (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.AddItems (new[] { baseObj, derivedObj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel>().Select (v => v.Property), Contains.Item (baseProperty.Object));

			Assume.That (vm.SelectedObjects, Is.TypeOf<ObservableCollectionEx<object>> ());
			((ObservableCollectionEx<object>)vm.SelectedObjects).Reset (new[] { baseObj });
			Assume.That (vm.Properties, Is.Not.Empty);

			var changedField = typeof (ObservableCollection<IPropertyInfo>).GetField (nameof (INotifyCollectionChanged.CollectionChanged), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			MulticastDelegate d = (MulticastDelegate)changedField.GetValue (derivedProperties);
			Assert.That (d, Is.Null);
		}

		[Test]
		[Description ("We must be sure that if the selected objects list changes while the provider is still retrieving that we end up with the right result")]
		public async Task InteruptedEditorRetrievalResolvesCorrectlyItemAdded ()
		{
			var obj1 = new object ();
			var obj2 = new object ();

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (pi => pi.Type).Returns (typeof (string));

			var editor1 = new Mock<IObjectEditor> ();
			editor1.SetupGet (oe => oe.Target).Returns (obj1);
			editor1.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (oe => oe.Target).Returns (obj2);
			editor2.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });

			Task<IObjectEditor> returnObject = null;

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj1)).Returns (() => {
				returnObject = Task.Delay (2000).ContinueWith (t => editor1.Object);
				return returnObject;
			});
			provider.Setup (ep => ep.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2.Object);

			var vm = new PanelViewModel (new TargetPlatform (provider.Object));
			vm.SelectedObjects.Add (obj1);

			Assume.That (returnObject, Is.Not.Null);
			Assume.That (returnObject.IsCompleted, Is.False);

			vm.SelectedObjects.Remove (obj1);

			await returnObject;
			await Task.Yield ();

			Assert.That (vm.Properties, Is.Empty);
			this.context.ThrowPendingExceptions ();
		}

		[Test]
		[Description ("When filtered Text exists then list is reduced.")]
		public async Task PropertyListFilteredTextReducesList ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Count, Is.EqualTo (2));

			vm.FilterText = "sub";
			Assert.That (vm.ArrangedEditors[0].Count, Is.EqualTo (1));
		}

		[Test]
		public async Task PropertyListIsFiltered ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Count, Is.EqualTo (2));

			Assume.That (vm.IsFiltering, Is.False);
			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PanelViewModel.IsFiltering)) {
					changed = true;
				}
			};

			vm.FilterText = "sub";
			Assume.That (vm.ArrangedEditors[0].Count, Is.EqualTo (1));
			Assert.That (vm.IsFiltering, Is.True);
			Assert.That (changed, Is.True);
			changed = false;

			vm.FilterText = null;
			Assert.That (vm.IsFiltering, Is.False);
			Assert.That (changed, Is.True);
		}

		[Test]
		[Description ("When filtered Text is cleared then list is restored back to its original.")]
		public async Task PropertyListFilteredTextClearedRestoresList ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));
			
			var vm = new PanelViewModel (new TargetPlatform (provider));
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors[0].Count, Is.EqualTo (2));

			vm.FilterText = "sub";
			Assume.That (vm.ArrangedEditors[0].Count, Is.EqualTo (1));

			vm.FilterText = String.Empty;
			Assert.That (vm.ArrangedEditors[0].Count, Is.EqualTo (2));
		}

		[Test]
		public async Task PropertyCategoryArrange ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider)) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assert.That (vm.ArrangedEditors.FirstOrDefault (g => g.Key == "Sub"), Is.Not.Null);
		}

		[Test]
		public void GroupedPropertiesArrange ()
		{
			var intProvider = new IntegerPropertyViewModelTests ();
			var stringProvider = new StringViewModelTests ();
			var brushProvider = new SolidBrushPropertyViewModelTests();

			var intProperty = intProvider.GetPropertyMock ("int", "A");
			var stringProperty1 = stringProvider.GetPropertyMock ("string1");
			var stringProperty2 = stringProvider.GetPropertyMock ("string2");
			var brushProperty = brushProvider.GetPropertyMock ("brush", "C");

			var editor = new MockObjectEditor (intProperty.Object, stringProperty1.Object, stringProperty2.Object, brushProperty.Object);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (editor.Target)).ReturnsAsync (editor);

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(string), "B" }
				}
			};

			var vm = new PanelViewModel (platform);
			Assume.That (vm.ArrangeMode, Is.EqualTo (PropertyArrangeMode.Name));

			vm.ArrangeMode = PropertyArrangeMode.Category;
			vm.SelectedObjects.Add (editor.Target);
			Assert.That (vm.ArrangedEditors[0].Key, Is.EqualTo ("A"));
			Assert.That (vm.ArrangedEditors[1].Key, Is.EqualTo ("B"));
			Assert.That (vm.ArrangedEditors[2].Key, Is.EqualTo ("C"));
		}

		[Test]
		public async Task PropertyListCategoryFiltered ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider)) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);

			vm.FilterText = "sub";
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors.FirstOrDefault (g => g.Key == "Sub");
			Assert.That (group, Is.Not.Null);
			Assert.That (group.Count, Is.EqualTo (1));
		}

		[Test]
		public async Task PropertyListCategoryGroupedWithNullCategory ()
		{
			// Purposefully a null catgory
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var mockPlatform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};

			var editor = await provider.Object.GetObjectEditorAsync (target);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (mockPlatform) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (target);

			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (2));
			Assert.That (vm.ArrangedEditors[0].Key, Is.EqualTo ("ints"), "Grouped group not found or out of order");
			Assert.That (vm.ArrangedEditors[1].Key, Is.Null);
		}

		[Test]
		public async Task PropertyListCategoryWithoutNameFiltered ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object ();
			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var mockPlatform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};			

			var editor = await provider.Object.GetObjectEditorAsync (target);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (mockPlatform) { ArrangeMode = PropertyArrangeMode.Category };
			vm.SelectedObjects.Add (target);

			Assume.That (vm.ArrangedEditors.Count, Is.EqualTo (2));

			vm.FilterText = "name";
			Assert.That (vm.ArrangedEditors.Count, Is.EqualTo (1));

			var group = vm.ArrangedEditors.FirstOrDefault (g => g.Key == "ints");
			Assert.That (group, Is.Null);
		}

		[Test]
		public void GroupedWithNullGroupedTypes ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var platform = new TargetPlatform (provider.Object);
			Assume.That (platform.GroupedTypes, Is.Null);

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category
			};

			Assert.That (() => vm.SelectedObjects.Add (target), Throws.Nothing);
		}

		[Test]
		[Description ("Bug coverage for grouped property failing to add if last")]
		public void AddGroupedAtEnd ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Category).Returns ("Category");
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};
			vm.SelectedObjects.Add (target);

			Assert.That (vm.ArrangedEditors.Any (g => g.Key == "ints"), Is.True, "Does not have grouped editors category");
		}

		[Test]
		public async Task AutoExpand ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (2));

			var vm = new PanelViewModel (new TargetPlatform (provider)) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assert.That (vm.GetIsExpanded (vm.ArrangedEditors[0].Key), Is.True);
		}

		[Test]
		public async Task AutoExpandGroupedProperties ()
		{
			var normalProp = new Mock<IPropertyInfo> ();
			normalProp.SetupGet (p => p.Type).Returns (typeof(string));
			normalProp.SetupGet (p => p.Category).Returns ("Category");
			normalProp.SetupGet (p => p.Name).Returns ("name");

			var groupProp = new Mock<IPropertyInfo> ();
			groupProp.SetupGet (p => p.Type).Returns (typeof(int));

			var target = new object();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target))
				.ReturnsAsync (new MockObjectEditor (normalProp.Object, groupProp.Object));

			var platform = new TargetPlatform (provider.Object) {
				GroupedTypes = new Dictionary<Type, string> {
					{ typeof(int), "ints" }
				}
			};

			var vm = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};
			vm.SelectedObjects.Add (target);

			Assume.That (vm.ArrangedEditors, Is.Not.Empty);
			Assume.That (vm.ArrangedEditors.Any (g => g.Key == "ints"), Is.True, "Does not have grouped editors category");
			Assert.That (vm.GetIsExpanded ("ints"), Is.True);
		}

		internal override PanelViewModel CreateVm (IEditorProvider provider)
		{
			return new PanelViewModel (new TargetPlatform (provider));
		}

		private TestContext context;
	}
}