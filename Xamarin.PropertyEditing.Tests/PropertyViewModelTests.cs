using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class PropertyViewModelTests<TValue>
	{
		[Test]
		public async Task GetValue ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj = new TestClass {
				Property = testValue
			};
			var vm = await GetBasicTestModelAsync (obj);
			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task SetValue ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var vm = await GetBasicTestModelAsync (obj);
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			vm.Value = testValue;
			Assert.That (obj.Property, Is.EqualTo (testValue));
		}

		[Test]
		public async Task GetSameValueMultiEditor ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj1 = new TestClass { Property = testValue };
			var obj2 = new TestClass { Property = testValue };
			var vm = await GetBasicTestModelAsync (obj1);
			vm.Editors.Add (await new ReflectionEditorProvider ().GetObjectEditorAsync (obj2));

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task SetSameValueMultiEditor ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj1 = new TestClass();
			var obj2 = new TestClass();
			Assume.That (obj1.Property, Is.EqualTo (default (TValue)));
			Assume.That (obj2.Property, Is.EqualTo (default (TValue)));

			var vm = await GetBasicTestModelAsync (obj1);
			vm.Editors.Add (await new ReflectionEditorProvider ().GetObjectEditorAsync (obj2));
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));

			vm.Value = testValue;
			Assert.That (obj1.Property, Is.EqualTo (testValue));
			Assert.That (obj2.Property, Is.EqualTo (testValue));
		}

		[Test]
		public async Task EditorValueChanged ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var mockEditor = await CreateMockEditor (obj);
			var property = mockEditor.Object.Properties.First ();
			var vm = GetViewModel (property, new[] { mockEditor.Object });

			mockEditor.Setup (e => e.GetValue<TValue> (property, null))
				.Returns ((IPropertyInfo p, PropertyVariation v) => new ValueInfo<TValue> { Value = testValue });

			mockEditor.Raise (e => e.PropertyChanged += null, new EditorPropertyChangedEventArgs (property));

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task AllEditorValuesChanged ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var mockEditor = await CreateMockEditor (obj);
			var property = mockEditor.Object.Properties.First ();
			var vm = GetViewModel (property, new[] { mockEditor.Object });

			mockEditor.Setup (e => e.GetValue<TValue> (property, null))
				.Returns ((IPropertyInfo p, PropertyVariation v) => new ValueInfo<TValue> { Value = testValue });

			mockEditor.Raise (e => e.PropertyChanged += null, new EditorPropertyChangedEventArgs (null));

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task MultipleValuesDontMatch ()
		{
			TValue value = GetNonDefaultRandomTestValue ();
			TValue otherValue = GetRandomTestValue ();
			while (Equals (otherValue, value))
				otherValue = GetRandomTestValue ();

			var obj1 = new TestClass { Property = value };
			var obj2 = new TestClass { Property = otherValue };

			var vm = await GetBasicTestModelAsync (obj1);
			Assume.That (vm.Value, Is.EqualTo (value));

			var provider = new ReflectionEditorProvider ();
			var editor = await provider.GetObjectEditorAsync (obj2).ConfigureAwait (false);
			vm.Editors.Add (editor);

			Assert.That (vm.Value, Is.EqualTo (default(TValue)));
			Assert.That (vm.MultipleValues, Is.True);
		}

		[Test]
		public async Task ValueChangedWhenValuesDisagree ()
		{
			TValue value = GetNonDefaultRandomTestValue ();
			TValue otherValue = GetRandomTestValue ();
			while (Equals (otherValue, value))
				otherValue = GetRandomTestValue ();

			var obj1 = new TestClass { Property = value };
			var obj2 = new TestClass { Property = otherValue };

			var vm = await GetBasicTestModelAsync (obj1);
			Assume.That (vm.Value, Is.EqualTo (value));

			var provider = new ReflectionEditorProvider ();
			var editor = await provider.GetObjectEditorAsync (obj2).ConfigureAwait (false);

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof (PropertyViewModel<TValue>.Value))
					changed = true;
			};

			vm.Editors.Add (editor);

			Assume.That (vm.Value, Is.EqualTo (default (TValue)));
			Assume.That (vm.MultipleValues, Is.True);
			Assert.That (changed, Is.True, "PropertyChanged was not raised for Value when values began to disagree");
		}

		[Test]
		[Description ("Once an editor is removed we should not listen for its property changes")]
		public async Task UnsubscribedValueChanged ()
		{
			TValue testValue = GetNonDefaultRandomTestValue ();
			Assume.That (testValue, Is.Not.EqualTo (default(TValue)));

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var vm = await GetBasicTestModelAsync (obj);
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			var editor = vm.Editors.Single ();
			Assume.That (vm.Editors.Remove (editor), Is.True);
			editor.SetValue (vm.Property, new ValueInfo<TValue> { Source = ValueSource.Local, Value = testValue });

			Assert.That (vm.Value, Is.Not.EqualTo (testValue));
		}

		[Test]
		public void CantSetResourceBeforeProvider ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var resource = new Resource ("name");
			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			Assume.That (vm.ResourceProvider, Is.Null);
			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False);
		}

		[Test]
		public void CantSetValueToUnknownResource ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			vm.ResourceProvider = resourcesMock.Object;
			Assume.That (vm.SetValueResourceCommand, Is.Not.Null);

			Assert.That (vm.SetValueResourceCommand.CanExecute (new Resource ("unknown")), Is.False, "Could set value to resource unknown");
		}

		[Test]
		public void CanSetValueToResource ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof(TValue));

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			vm.ResourceProvider = resourcesMock.Object;
			Assume.That (vm.SetValueResourceCommand, Is.Not.Null);

			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.True, "Could not set value to resource");
		}

		[Test]
		public void CantSetReadonlyPropertyToResource ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			vm.ResourceProvider = resourcesMock.Object;
			Assume.That (vm.SetValueResourceCommand, Is.Not.Null);

			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False, "Could set value to readonly resource");
		}

		[Test]
		public void SetValueToResource ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var resource = new Resource ("name");
			var value = GetNonDefaultRandomTestValue ();

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var editor = new MockObjectEditor (mockProperty.Object);
			editor.ValueEvaluator = (info, o) => {
				if (o == resource)
					return value;

				return default(TValue);
			};

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			vm.ResourceProvider = resourcesMock.Object;
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));

			vm.SetValueResourceCommand.Execute (resource);
			Assert.That (vm.Value, Is.EqualTo (value));
		}

		[Test]
		public void ResourcesListed ()
		{
			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			Assume.That (vm.ResourceProvider, Is.Null);
			Assume.That (vm.Resources, Is.Empty);
			vm.ResourceProvider = resourcesMock.Object;

			Assert.That (vm.Resources, Has.Member (resource));
		}

		[Test]
		public void GetValueAlreadySetToResource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var editor = new MockObjectEditor (mockProperty.Object);
			editor.SetValue (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Resource,
				ValueDescriptor = resource,
				Value = value
			});

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			vm.ResourceProvider = resourcesMock.Object;

			Assert.That (vm.Value, Is.EqualTo (value));
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Resource));
		}

		[Test]
		[Description ("For performance reasons, we should never raise a value change when it hasn't changed")]
		public void ValueNotChangedForSameValue ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof(TValue));

			var editor = new MockObjectEditor (mockProperty.Object);
			editor.SetValue (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Local,
				Value = value
			});

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (value));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				changed = true;
			};

			vm.Value = value;

			Assert.That (changed, Is.False, "PropertyChanged raised when value set to same value");
		}

		[Test]
		public void ValueChanged ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var editor = new MockObjectEditor (mockProperty.Object);
			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.Not.EqualTo (value));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				changed = true;
			};

			vm.Value = value;

			Assert.That (changed, Is.True, "PropertyChanged was not raised when value changed for Value");
		}

		[Test]
		[Description ("For performance reasons, we should never invoke the editor for a value change when it hasn't changed")]
		public void SetValueNotCalledOnEditorForSameValue ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = new Mock<IPropertyInfo> ();
			mockProperty.SetupGet (pi => pi.Type).Returns (typeof (TValue));

			var editorMock = new Mock<IObjectEditor> ();
			editorMock.Setup (oe => oe.GetValue<TValue> (mockProperty.Object, null)).Returns (new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (mockProperty.Object, new[] { editorMock.Object });
			Assume.That (vm.Value, Is.EqualTo (value));

			vm.Value = value;

			editorMock.Verify (oe => oe.SetValue (mockProperty.Object, It.IsAny<ValueInfo<TValue>> (), null), Times.Never);
		}

		protected TValue GetNonDefaultRandomTestValue ()
		{
			TValue value = default (TValue);
			while (Equals (value, default (TValue))) {
				value = GetRandomTestValue (Random);
			}

			return value;
		}

		protected abstract TValue GetRandomTestValue (Random rand);

		protected abstract PropertyViewModel<TValue> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors);

		protected Random Random => this.rand;

		private readonly Random rand = new Random (42);

		private async Task<Mock<IObjectEditor>> CreateMockEditor (object obj)
		{
			var provider = new ReflectionEditorProvider ();
			var editor = await provider.GetObjectEditorAsync (obj);

			var m = new Mock<IObjectEditor> ();
			m.SetupGet (e => e.Properties).Returns (editor.Properties);
			
			return m;
		}

		private TValue GetRandomTestValue ()
		{
			return GetRandomTestValue (this.rand);
		}

		private async Task<PropertyViewModel<TValue>> GetBasicTestModelAsync (TestClass instance)
		{
			var provider = new ReflectionEditorProvider ();
			var editor = await provider.GetObjectEditorAsync (instance).ConfigureAwait (false);

			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			return GetViewModel (editor.Properties.Single (), new[] { editor });
		}

		private class TestClass
		{
			public TValue Property
			{
				get;
				set;
			}
		}
	}
}