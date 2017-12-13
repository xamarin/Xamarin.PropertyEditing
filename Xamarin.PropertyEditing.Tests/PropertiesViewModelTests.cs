using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	public abstract class PropertiesViewModelTests
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

		private Exception unhandled;
		[SetUp]
		public void Setup ()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		}

		[TearDown]
		public void TearDown ()
		{
			AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;

			if (this.unhandled != null) {
				var ex = this.unhandled;
				this.unhandled = null;
				Assert.Fail ("Unhandled exception: {0}", ex);
			}
		}

		private void CurrentDomainOnUnhandledException (object sender, UnhandledExceptionEventArgs e)
		{
			this.unhandled = e.ExceptionObject as Exception;
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
			editor.SetupGet (e => e.Target).Returns (obj);
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
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Target).Returns (obj2);
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
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor2.SetupGet (e => e.Target).Returns (obj2);
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
			editor.As<INameableObject> ().Setup (n => n.GetNameAsync ()).ReturnsAsync (name);

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			editor2.SetupGet (e => e.Target).Returns (obj2);
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
			editor.SetupGet (e => e.Target).Returns (obj);
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
			editor.SetupGet (e => e.Properties).Returns (new IPropertyInfo[0]);
			var eeditor = editor.As<IObjectEventEditor> ();
			eeditor.SetupGet (e => e.Events).Returns (new[] { ev.Object });

			var editor2 = new Mock<IObjectEditor> ();
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
		}

		internal abstract PropertiesViewModel CreateVm (IEditorProvider provider);
	}
}