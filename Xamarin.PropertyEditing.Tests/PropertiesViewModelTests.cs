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
	internal abstract class PropertiesViewModelTests<TViewModel>
		where TViewModel : PropertiesViewModel
	{
		protected class TestClass
		{
			public string Property
			{
				get;
				set;
			}
		}

		protected class TestClassSub
			: TestClass
		{
			[System.ComponentModel.Category ("Sub")]
			public int SubProperty
			{
				get;
				set;
			}
		}

		private TestContext syncContext;
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
		public void TypeName ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();

			var vm = CreateVm (provider);
			Assume.That (vm.TypeName, Is.Null);

			vm.SelectedObjects.Add (obj);

			Assert.That (vm.TypeName, Is.EqualTo (nameof (TestClassSub)));
		}

		[Test]
		public void TypeNameNoneSelected ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();

			var vm = CreateVm (provider);
			Assume.That (vm.TypeName, Is.Null);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.TypeName, Is.EqualTo (nameof (TestClassSub)));

			vm.SelectedObjects.Remove (obj);
			Assert.That (vm.TypeName, Is.Null);
		}

		[Test]
		public void TypeNameMultipleSameSelected ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var obj2 = new TestClassSub ();

			var vm = CreateVm (provider);
			Assume.That (vm.TypeName, Is.Null);

			vm.SelectedObjects.Add (obj);
			vm.SelectedObjects.Add (obj2);

			Assert.That (vm.TypeName, Is.EqualTo (nameof (TestClassSub)));
		}

		[Test]
		public void TypeNameMultipleNotSameSelected ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClassSub ();
			var obj2 = new TestClass ();

			var vm = CreateVm (provider);
			Assume.That (vm.TypeName, Is.Null);

			vm.SelectedObjects.Add (obj);
			vm.SelectedObjects.Add (obj2);

			Assert.That (vm.TypeName, Is.Not.EqualTo (nameof (TestClassSub)));
			Assert.That (vm.TypeName, Is.Not.EqualTo (nameof (TestClass)));
		}

		[Test]
		public void ObjectName ()
		{
			var obj = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.ObjectName, Is.Null);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (PropertiesViewModel.ObjectName))
					changed = true;
			};

			vm.SelectedObjects.Add (obj);

			Assert.That (vm.ObjectName, Is.EqualTo (name));
			Assert.That (changed, Is.True);
		}

		[Test]
		public void IsObjectNameable ()
		{
			var obj = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.IsObjectNameable, Is.False);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (PropertiesViewModel.IsObjectNameable))
					changed = true;
			};

			vm.SelectedObjects.Add (obj);

			Assert.That (vm.IsObjectNameable, Is.True);
			Assert.That (changed, Is.True);
		}

		[Test]
		public void ObjectNameNoneSelected ()
		{
			var obj = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.ObjectName, Is.Null);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.ObjectName, Is.EqualTo (name));

			vm.SelectedObjects.Remove (obj);
			Assert.That (vm.ObjectName, Is.Null);
		}

		[Test]
		public void IsObjectNameableNoneSelected ()
		{
			var obj = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.IsObjectNameable, Is.False);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.IsObjectNameable, Is.True);

			vm.SelectedObjects.Remove (obj);
			Assert.That (vm.IsObjectNameable, Is.False);
		}

		[Test]
		public void IsObjectNameableNotNameable ()
		{
			var obj = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.IsObjectNameable, Is.False);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.IsObjectNameable, Is.False);

			vm.SelectedObjects.Remove (obj);
			Assert.That (vm.IsObjectNameable, Is.False);
		}

		[Test]
		public void ObjectNameNoName ()
		{
			var obj = new object ();

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetTarget (obj);
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync ((string)null);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.ObjectName, Is.Null);

			vm.SelectedObjects.Add (obj);

			Assert.That (vm.ObjectName, Is.EqualTo (Properties.Resources.NoName));
		}

		[Test]
		public void ObjectNameMultipleSelected ()
		{
			var obj = new object ();
			var obj2 = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Target).Returns (obj2);
			editor2.SetupGet (oe => oe.TargetType).Returns (obj2.GetType ().ToTypeInfo ());
			editor2.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			
			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (p => p.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.ObjectName, Is.Null);

			vm.SelectedObjects.Add (obj);
			vm.SelectedObjects.Add (obj2);

			// Properties.Resources.MultipleObjectsSelected
			Assert.That (vm.ObjectName, Is.Not.Null.And.Not.EqualTo (name));

			vm.SelectedObjects.Remove (obj2);

			Assert.That (vm.ObjectName, Is.EqualTo (name));
		}

		[Test]
		public void IsObjectNameableMultipleSelected ()
		{
			var obj = new object ();
			var obj2 = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor2.SetupGet (e => e.Target).Returns (obj2);
			editor2.SetupGet (oe => oe.TargetType).Returns (obj2.GetType ().ToTypeInfo ());
			editor2.As<INameableObject> ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (p => p.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.IsObjectNameable, Is.False);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.IsObjectNameable, Is.True);

			vm.SelectedObjects.Add (obj2);
			Assert.That (vm.IsObjectNameable, Is.True);

			vm.SelectedObjects.Remove (obj2);
			Assert.That (vm.IsObjectNameable, Is.True);
		}

		[Test]
		public void IsObjectReadonlyMultipleSelected ()
		{
			var obj = new object ();
			var obj2 = new object ();

			const string name = "name";
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (e => e.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor2.SetupGet (e => e.Target).Returns (obj2);
			editor2.SetupGet (oe => oe.TargetType).Returns (obj2.GetType ().ToTypeInfo ());
			editor2.As<INameableObject> ();

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (p => p.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.IsObjectNameReadOnly, Is.False);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.IsObjectNameReadOnly, Is.False);

			vm.SelectedObjects.Add (obj2);
			Assert.That (vm.IsObjectNameReadOnly, Is.True);

			vm.SelectedObjects.Remove (obj2);
			Assert.That (vm.IsObjectNameReadOnly, Is.False);
		}

		[Test]
		public void SetObjectName()
		{
			var obj = new object ();

			const string name = "name";

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetTarget (obj);
			var nameable = editor.As<INameableObject> ();
			nameable.Setup (n => n.GetNameAsync ()).ReturnsAsync ((string)null);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.IsObjectNameable, Is.False);

			vm.SelectedObjects.Add (obj);
			Assume.That (vm.IsObjectNameable, Is.True);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (PropertiesViewModel.ObjectName))
					changed = true;
			};

			vm.ObjectName = name;
			nameable.Verify (n => n.SetNameAsync (name));
			Assert.That (changed, Is.True);
			Assert.That (vm.ObjectName, Is.EqualTo (name));
		}

		[Test]
		public void EventsNotEnabled()
		{
			var obj = new object ();

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetTarget (obj);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assert.That (vm.EventsEnabled, Is.False);

			vm.SelectedObjects.Add (obj);
			Assert.That (vm.EventsEnabled, Is.False);
		}

		[Test]
		public void Events()
		{
			var obj = new object ();

			var ev = new Mock<IEventInfo> ();
			ev.SetupGet (e => e.Name).Returns ("name");

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor.SetupGet (oe => oe.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.SetupGet (e => e.Events).Returns (new[] { ev.Object });

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			Assume.That (vm.EventsEnabled, Is.False);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof (PropertiesViewModel.EventsEnabled))
					changed = true;
			};

			vm.SelectedObjects.Add (obj);

			Assert.That (vm.EventsEnabled, Is.True);
			Assert.That (changed, Is.True);
			Assert.That (vm.Events.Count, Is.EqualTo (1));
			Assert.That (vm.Events[0].Event, Is.SameAs (ev.Object));
		}

		[Test]
		[Description ("Currently we just clear the events list if multiple objects are selected regardless of typ")]
		public void MultipleObjectEvents ()
		{
			var obj = new object();
			var obj2 = new object();

			var ev = new Mock<IEventInfo> ();
			ev.SetupGet (e => e.Name).Returns ("name");

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Target).Returns (obj);
			editor.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.SetupGet (e => e.Events).Returns (new[] { ev.Object });

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (oe => oe.Target).Returns (obj2);
			editor2.SetupGet (oe => oe.TargetType).Returns (obj2.GetType ().ToTypeInfo ());
			editor2.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			var eeditor2 = editor.As<IObjectEventEditor> ();
			eeditor2.SetupGet (e => e.Events).Returns (new[] { ev.Object });

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (obj)).ReturnsAsync (editor.Object);
			provider.Setup (p => p.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (obj);
			Assume.That (vm.EventsEnabled, Is.True);
			Assume.That (vm.Events.Count, Is.EqualTo (1));

			vm.SelectedObjects.Add (obj2);
			Assert.That (vm.Events.Count, Is.EqualTo (0));
		}

		[Test]
		[Description ("The IEditorProvider should be able to return null editors")]
		public void NullEditorAdded ()
		{
			var providerMock = new Mock<IEditorProvider> ();

			var vm = CreateVm (providerMock.Object);
			vm.SelectedObjects.Add (null);
			vm.SelectedObjects.Add (null);
		}

		[Test]
		public void BasicPredefinedGetsPredefined ()
		{
			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (pi => pi.Type).Returns (typeof(string));
			var p = property.As<IHavePredefinedValues<string>> ();
			p.SetupGet (pv => pv.PredefinedValues).Returns (new Dictionary<string, string> { { "key", "value" } });

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.TargetType).Returns (target.GetType ().ToTypeInfo ());
			editor.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);

			Assume.That (vm.Properties, Has.Count.EqualTo (1));
			Assert.That (vm.Properties.First(), Is.TypeOf<PredefinedValuesViewModel<string>> ());
		}

		[Test]
		public async Task PropertiesAddedFromEditor ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClass ();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			var vm = CreateVm (new TargetPlatform (provider));
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
			editor1Mock.SetupGet (oe => oe.Target).Returns (obj1);
			editor1Mock.SetupGet (oe => oe.TargetType).Returns (obj1.GetType ().ToTypeInfo ());
			var editor2Mock = new Mock<IObjectEditor> ();
			editor2Mock.SetupGet (oe => oe.Properties).Returns (new[] { sharedPropertyMock.Object, subPropertyMock.Object });
			editor2Mock.SetupGet (oe => oe.Target).Returns (obj2);
			editor2Mock.SetupGet (oe => oe.TargetType).Returns (obj2.GetType ().ToTypeInfo ());

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj1)).ReturnsAsync (editor1Mock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2Mock.Object);

			var vm = CreateVm (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.Add (obj1);

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (((PropertyViewModel)vm.Properties[0]).Property, Is.EqualTo (sharedPropertyMock.Object));

			// Reflection property info equate actually fails on the same property across class/subclass
			vm.SelectedObjects.Add (obj2);
			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (((PropertyViewModel)vm.Properties.Single ()).Property, Is.EqualTo (sharedPropertyMock.Object));
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
			editor1Mock.SetTarget (obj1);
			var editor2Mock = new Mock<IObjectEditor> ();
			editor2Mock.SetupGet (oe => oe.Properties).Returns (new[] { sharedPropertyMock.Object, subPropertyMock.Object });
			editor2Mock.SetTarget (obj2);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj1)).ReturnsAsync (editor1Mock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (obj2)).ReturnsAsync (editor2Mock.Object);

			var vm = CreateVm (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.Add (obj2);

			Assume.That (vm.Properties.Count, Is.EqualTo (2));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (sharedPropertyMock.Object));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (subPropertyMock.Object));

			// Reflection property info equate actually fails on the same property across class/subclass
			vm.SelectedObjects.Add (obj1);
			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (sharedPropertyMock.Object));
		}

		[Test]
		[Description ("Adding or removing editors shouldn't remake other editors or duplicate")]
		public void EditorsShouldBeConsistent ()
		{
			var provider = new ReflectionEditorProvider ();

			var obj1 = new TestClass ();
			var obj2 = new TestClass ();

			var vm = CreateVm (new TargetPlatform (provider));
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

			var vm = CreateVm (new TargetPlatform (provider));
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

			var obj = new object ();

			var properties = new ObservableCollection<IPropertyInfo> { mockProperty1.Object, mockProperty2.Object };
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (obj);
			editorMock.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editorMock.SetupGet (e => e.Properties).Returns (properties);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editorMock.Object);

			var vm = CreateVm (new TargetPlatform (provider.Object));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.Properties.Count, Is.EqualTo (2));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty1.Object));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty2.Object));

			properties.Remove (mockProperty2.Object);
			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty1.Object));
		}

		[Test]
		public void PropertiesListItemAdded ()
		{
			var mockProperty1 = new Mock<IPropertyInfo> ();
			mockProperty1.SetupGet (pi => pi.Type).Returns (typeof (string));

			var mockProperty2 = new Mock<IPropertyInfo> ();
			mockProperty2.SetupGet (pi => pi.Type).Returns (typeof (string));

			var obj = new object ();

			var properties = new ObservableCollection<IPropertyInfo> { mockProperty1.Object };
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (oe => oe.Target).Returns (obj);
			editorMock.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editorMock.SetupGet (e => e.Properties).Returns (properties);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editorMock.Object);

			var vm = CreateVm (new TargetPlatform (provider.Object));
			vm.SelectedObjects.Add (obj);

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty1.Object));

			properties.Add (mockProperty2.Object);

			Assert.That (vm.Properties.Count, Is.EqualTo (2));
			Assert.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty1.Object));
			Assert.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty2.Object));
		}

		[Test]
		public void PropertiesListItemAddedWithReset ()
		{
			var mockProperty1 = new Mock<IPropertyInfo> ();
			mockProperty1.SetupGet (pi => pi.Type).Returns (typeof (string));

			var mockProperty2 = new Mock<IPropertyInfo> ();
			mockProperty2.SetupGet (pi => pi.Type).Returns (typeof (string));

			var obj = new object ();

			var properties = new ObservableCollection<IPropertyInfo> { mockProperty1.Object };
			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (e => e.Target).Returns (obj);
			editorMock.SetupGet (oe => oe.TargetType).Returns (obj.GetType ().ToTypeInfo ());
			editorMock.SetupGet (e => e.Properties).Returns (properties);

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (ep => ep.GetObjectEditorAsync (obj)).ReturnsAsync (editorMock.Object);

			var vm = CreateVm (new TargetPlatform (provider.Object));

			// We need access to the custom reset method here to ensure compliance
			// It's a bit hacky but this is unlikely to change. If it does, this test
			// will ensure the new notifier works as it should when resetting.
			Assume.That (vm.SelectedObjects, Is.TypeOf<ObservableCollectionEx<object>> ());
			((ObservableCollectionEx<object>)vm.SelectedObjects).Reset (new[] { obj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty1.Object));

			properties.Add (mockProperty2.Object);

			Assert.That (vm.Properties.Count, Is.EqualTo (2));
			Assert.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty1.Object));
			Assert.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (mockProperty2.Object));
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
			baseEditorMock.SetTarget (baseObj);

			var derivedEditorMock = new Mock<IObjectEditor> ();
			derivedEditorMock.SetupGet (e => e.Properties).Returns (derivedProperties);
			derivedEditorMock.SetTarget (derivedObj);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (baseObj)).ReturnsAsync (baseEditorMock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (derivedObj)).ReturnsAsync (derivedEditorMock.Object);

			var vm = CreateVm (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.AddItems (new[] { baseObj, derivedObj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (baseProperty.Object));

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
			baseEditorMock.SetTarget (baseObj);

			var derivedEditorMock = new Mock<IObjectEditor> ();
			derivedEditorMock.SetupGet (e => e.Properties).Returns (derivedProperties);
			derivedEditorMock.SetTarget (derivedObj);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (baseObj)).ReturnsAsync (baseEditorMock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (derivedObj)).ReturnsAsync (derivedEditorMock.Object);

			var vm = CreateVm (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.AddItems (new[] { baseObj, derivedObj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (baseProperty.Object));

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
			baseEditorMock.SetTarget (baseObj);

			var derivedEditorMock = new Mock<IObjectEditor> ();
			derivedEditorMock.SetupGet (e => e.Properties).Returns (derivedProperties);
			derivedEditorMock.SetTarget (derivedObj);

			var providerMock = new Mock<IEditorProvider> ();
			providerMock.Setup (ep => ep.GetObjectEditorAsync (baseObj)).ReturnsAsync (baseEditorMock.Object);
			providerMock.Setup (ep => ep.GetObjectEditorAsync (derivedObj)).ReturnsAsync (derivedEditorMock.Object);

			var vm = CreateVm (new TargetPlatform (providerMock.Object));
			vm.SelectedObjects.AddItems (new[] { baseObj, derivedObj });

			Assume.That (vm.Properties.Count, Is.EqualTo (1));
			Assume.That (vm.Properties.Cast<PropertyViewModel> ().Select (v => v.Property), Contains.Item (baseProperty.Object));

			Assume.That (vm.SelectedObjects, Is.TypeOf<ObservableCollectionEx<object>> ());
			((ObservableCollectionEx<object>)vm.SelectedObjects).Reset (new[] { baseObj });
			Assume.That (vm.Properties, Is.Not.Empty);

			var changedField = typeof (ObservableCollection<IPropertyInfo>).GetField (nameof (INotifyCollectionChanged.CollectionChanged), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
			MulticastDelegate d = (MulticastDelegate)changedField.GetValue (derivedProperties);
			Assert.That (d, Is.Null);
		}

		[Test]
		public void SetVariantsAreIncluded ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof(string));
			property.SetupGet (p => p.RealType).Returns (typeof(string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);

			var variants = new[] {
				new PropertyVariation (variations[0]),
				new PropertyVariation (variations[0], variations[2])
			};

			var target = new object();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof(object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[1])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});
			
			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);

			var stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList();
			Assert.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.First (svm => svm.Variation == null).HasVariantChildren, Is.True, "Did not report variant children");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");
		}

		[Test]
		public void SetVariantsAreIncludedMultiSelect ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);

			var variants = new[] {
				new PropertyVariation (variations[0]),
				new PropertyVariation (variations[0], variations[2])
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (new[] { variants[0], variants[1] });
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[1])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var target2 = new object ();
			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor2.SetupGet (oe => oe.Target).Returns (target2);
			editor2.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });
			editor2.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (new[] { variants[0] });
			editor2.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor2.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);
			provider.Setup (p => p.GetObjectEditorAsync (target2)).ReturnsAsync (editor2.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);

			var stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assert.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.First (svm => svm.Variation == null).HasVariantChildren, Is.True, "Did not report variant children");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");

			vm.SelectedObjects.Add (target2);

			stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assert.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			var neutralVm = stringVms.FirstOrDefault (svm => svm.Variation == null);
			Assert.That (neutralVm, Is.Not.Null, "Did not include neutral property");
			Assert.That (neutralVm.Editors.Count, Is.EqualTo (2), "Did not add second editor to VM");

			var variant1Vm = stringVms.FirstOrDefault (svm => svm.Variation == variants[0]);
			Assert.That (variant1Vm, Is.Not.Null, "Missing variant property");
			Assert.That (variant1Vm.Editors.Count, Is.EqualTo (2), "Did not add second editor to VM");

			var variant2Vm = stringVms.FirstOrDefault (svm => svm.Variation == variants[1]);
			Assert.That (variant2Vm, Is.Not.Null, "Missing variant property");
			Assert.That (variant2Vm.Editors.Count, Is.EqualTo (2), "Did not add second editor to VM");

			vm.SelectedObjects.Remove (target);

			stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assert.That (stringVms.Count, Is.EqualTo (2), "Not including correct number of properties with variants after remove");
			neutralVm = stringVms.FirstOrDefault (svm => svm.Variation == null);
			Assert.That (neutralVm, Is.Not.Null, "Did not include neutral property");
			Assert.That (neutralVm.Editors.Count, Is.EqualTo (1), "Did not remove second editor to VM");

			variant1Vm = stringVms.FirstOrDefault (svm => svm.Variation == variants[0]);
			Assert.That (variant1Vm, Is.Not.Null, "Missing variant property");
			Assert.That (variant1Vm.Editors.Count, Is.EqualTo (1), "Did not remove second editor to VM");

			variant2Vm = stringVms.FirstOrDefault (svm => svm.Variation == variants[1]);
			Assert.That (variant2Vm, Is.Null, "Variation property wasn't removed");
		}

		[Test]
		public void VariantsRemovedWhenPropertyIs ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);

			var properties = new ObservableCollection<IPropertyInfo> { property.Object };

			var variants = new[] {
				new PropertyVariation (variations[0]),
				new PropertyVariation (variations[0], variations[2])
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (properties);
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[1])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);

			var stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assume.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assume.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assume.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assume.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");

			properties.RemoveAt (0);
			Assert.That (vm.Properties, Is.Empty);
		}

		[Test]
		public void VariantsUpdated ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);

			var properties = new ObservableCollection<IPropertyInfo> { property.Object };

			var variation0 = new PropertyVariation (variations[0]);
			var variation1 = new PropertyVariation (variations[0], variations[2]);
			var variants = new List<PropertyVariation> {
				variation0,
				variation1
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (properties);
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variation0)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variation1)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);

			var stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assume.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assume.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assume.That (stringVms.First (svm => svm.Variation == null).HasVariantChildren, Is.True, "Did not report variant children");
			Assume.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assume.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");

			variants.RemoveAt (1);
			editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object));

			Assert.That (vm.Properties.Count, Is.EqualTo (2));
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.First (svm => svm.Variation == null).HasVariantChildren, Is.True, "Did not report variant children");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");

			variants.RemoveAt (0);
			editor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property.Object));

			Assert.That (vm.Properties.Count, Is.EqualTo (1));
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.First (svm => svm.Variation == null).HasVariantChildren, Is.False, "Did not update variant children");
		}

		[Test]
		public void VariantsAddedWhenPropertyIs ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);

			var properties = new ObservableCollection<IPropertyInfo>();

			var variants = new[] {
				new PropertyVariation (variations[0]),
				new PropertyVariation (variations[0], variations[2])
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (properties);
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[1])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);
			Assume.That (vm.Properties, Is.Empty);

			properties.Add (property.Object);

			var stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assert.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");
		}

		[Test]
		public void VariantsUpdateWhenPropertyReplaced ()
		{
			var variations = new[] {
				new PropertyVariationOption ("Width", "Compact"),
				new PropertyVariationOption ("Width", "Regular"),
				new PropertyVariationOption ("Gamut", "P3"),
				new PropertyVariationOption ("Gamut", "sRGB"),
			};

			var property = new Mock<IPropertyInfo> ();
			property.SetupGet (p => p.Name).Returns ("Variation");
			property.SetupGet (p => p.Type).Returns (typeof (string));
			property.SetupGet (p => p.RealType).Returns (typeof (string).ToTypeInfo ());
			property.SetupGet (p => p.CanWrite).Returns (true);
			property.SetupGet (p => p.ValueSources).Returns (ValueSources.Default | ValueSources.Local);
			property.SetupGet (p => p.Variations).Returns (variations);

			var properties = new ObservableCollection<IPropertyInfo> { property.Object };

			var secondSet = new PropertyVariation (variations[0], variations[2]);
			var variants = new List<PropertyVariation> {
				new PropertyVariation (variations[0]),
			};

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.TargetType).Returns (typeof (object).ToTypeInfo ());
			editor.SetupGet (oe => oe.Target).Returns (target);
			editor.SetupGet (oe => oe.Properties).Returns (properties);
			editor.Setup (oe => oe.GetPropertyVariantsAsync (property.Object)).ReturnsAsync (() => variants);
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, null)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Any",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, variants[0])).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact",
					Source = ValueSource.Local
				});
			editor.Setup (oe => oe.GetValueAsync<string> (property.Object, secondSet)).ReturnsAsync (
				new ValueInfo<string> {
					Value = "Compact+P3",
					Source = ValueSource.Local
				});

			var provider = new Mock<IEditorProvider> ();
			provider.Setup (p => p.GetObjectEditorAsync (target)).ReturnsAsync (editor.Object);

			var vm = CreateVm (provider.Object);
			vm.SelectedObjects.Add (target);

			var stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assume.That (stringVms.Count, Is.EqualTo (2), "Not including correct number of properties with variants");
			Assume.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assume.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");

			variants.Add (secondSet);

			properties[0] = property.Object;
			stringVms = vm.Properties.OfType<StringPropertyViewModel> ().ToList ();
			Assert.That (stringVms.Count, Is.EqualTo (3), "Not including correct number of properties with variants");
			Assert.That (stringVms.Count (svm => svm.Variation == null), Is.EqualTo (1), "Did not include neutral property");
			Assert.That (stringVms.First (svm => svm.Variation == null).HasVariantChildren, Is.True, "Did not report variant children");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[0]), Is.EqualTo (1), "Missing variant property");
			Assert.That (stringVms.Count (svm => svm.Variation == variants[1]), Is.EqualTo (1), "Missing variant property");
		}

		protected TViewModel CreateVm (IEditorProvider provider)
		{
			return CreateVm (new TargetPlatform (provider));
		}

		internal abstract TViewModel CreateVm (TargetPlatform platform);
	}
}
