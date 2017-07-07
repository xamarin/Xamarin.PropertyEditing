using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	internal abstract class PropertyViewModelTests<TValue, TViewModel>
		where TViewModel : PropertyViewModel<TValue>
	{
		[Test]
		public void GetValue ()
		{
			TValue testValue = GetRandomTestValue ();
			
			var vm = GetBasicTestModel (testValue);
			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public void SetValue ()
		{
			TValue testValue = GetRandomTestValue ();

			var vm = GetBasicTestModel();
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			vm.Value = testValue;
			Assert.That (vm.Editors.First().GetValue<TValue> (vm.Property).Value, Is.EqualTo (testValue));
		}

		[Test]
		public void GetSameValueMultiEditor ()
		{
			TValue testValue = GetRandomTestValue ();

			var vm = GetBasicTestModel (testValue);
			Assume.That (vm.Value, Is.EqualTo (testValue));

			vm.Editors.Add (GetBasicEditor (testValue, vm.Property));

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public void SetSameValueMultiEditor ()
		{
			TValue testValue = GetRandomTestValue ();

			var vm = GetBasicTestModel ();
			vm.Editors.Add (GetBasicEditor (vm.Property));
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));

			vm.Value = testValue;

			Assert.That (vm.Editors.First().GetValue<TValue> (vm.Property).Value, Is.EqualTo (testValue));
			Assert.That (vm.Editors.Skip (1).First().GetValue<TValue> (vm.Property).Value, Is.EqualTo (testValue));
		}

		[Test]
		public void EditorValueChanged ()
		{
			TValue testValue = GetRandomTestValue ();
			var vm = GetBasicTestModel();

			vm.Editors.First().SetValue (vm.Property, new ValueInfo<TValue> {
				Source = ValueSource.Local,
				Value = testValue
			});

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public void AllEditorValuesChanged ()
		{
			TValue testValue = GetRandomTestValue ();

			var mockEditor = GetBasicEditor ();
			var property = mockEditor.Properties.First ();
			var vm = GetViewModel (property, new[] { mockEditor });

			mockEditor.values[property] = new ValueInfo<TValue> {
				Value = testValue,
				Source = ValueSource.Local
			};

			mockEditor.ChangeAllProperties();

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public void MultipleValuesDontMatch ()
		{
			TValue value = GetNonDefaultRandomTestValue ();
			TValue otherValue = GetRandomTestValue ();
			while (Equals (otherValue, value))
				otherValue = GetRandomTestValue ();

			var vm = GetBasicTestModel (value);
			Assume.That (vm.Value, Is.EqualTo (value));

			var editor = GetBasicEditor (otherValue);
			vm.Editors.Add (editor);

			Assert.That (vm.Value, Is.EqualTo (default(TValue)));
			Assert.That (vm.MultipleValues, Is.True);
		}

		[Test]
		public void ValueChangedWhenValuesDisagree ()
		{
			TValue value = GetNonDefaultRandomTestValue ();
			TValue otherValue = GetRandomTestValue ();
			while (Equals (otherValue, value))
				otherValue = GetRandomTestValue ();

			var vm = GetBasicTestModel (value);
			Assume.That (vm.Value, Is.EqualTo (value));

			var editor = GetBasicEditor (otherValue);

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
		public void UnsubscribedValueChanged ()
		{
			TValue testValue = GetNonDefaultRandomTestValue ();
			Assume.That (testValue, Is.Not.EqualTo (default(TValue)));

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var vm = GetBasicTestModel ();
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			var editor = vm.Editors.Single ();
			Assume.That (vm.Editors.Remove (editor), Is.True);
			editor.SetValue (vm.Property, new ValueInfo<TValue> { Source = ValueSource.Local, Value = testValue });

			Assert.That (vm.Value, Is.Not.EqualTo (testValue));
		}

		[Test]
		public void CantSetResourceBeforeProvider ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);

			var resource = new Resource ("name");
			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			Assume.That (vm.ResourceProvider, Is.Null);
			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False);
		}

		[Test]
		public void CantSetValueToUnknownResource ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);

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
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);

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
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);

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
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);

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
			var mockProperty = GetPropertyMock ();

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

			var mockProperty = GetPropertyMock ();

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

			var mockProperty = GetPropertyMock ();

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

			var mockProperty = GetPropertyMock ();

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

			var mockProperty = GetPropertyMock ();

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

		protected abstract TViewModel GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors);

		protected virtual void AugmentPropertyMock (Mock<IPropertyInfo> propertyMock)
		{
		}

		protected Mock<IPropertyInfo> GetPropertyMock ()
		{
			var mock = new Mock<IPropertyInfo> ();
			mock.SetupGet (pi => pi.Type).Returns (typeof(TValue));

			AugmentPropertyMock (mock);

			return mock;
		}

		protected Random Random => this.rand;

		protected TValue GetRandomTestValue ()
		{
			return GetRandomTestValue (this.rand);
		}

		protected MockObjectEditor GetBasicEditor (IPropertyInfo property = null)
		{
			if (property == null) {
				var propertyMock = GetPropertyMock ();
				propertyMock.SetupGet (p => p.Name).Returns (nameof(TestClass.Property));

				property = propertyMock.Object;
			}

			var editor = new MockObjectEditor (property) {
				Target = new TestClass()
			};

			return editor;
		}

		protected MockObjectEditor GetBasicEditor (TValue value, IPropertyInfo property = null)
		{
			var editor = GetBasicEditor (property);
			editor.values[editor.Properties.First ()] = value;
			return editor;
		}

		protected TViewModel GetBasicTestModel ()
		{
			var editor = GetBasicEditor ();

			return GetViewModel (editor.Properties.First(), new[] { editor });
		}

		protected TViewModel GetBasicTestModel (TValue value)
		{
			var editor = GetBasicEditor (value);

			return GetViewModel (editor.Properties.First(), new[] { editor });
		}

		protected class TestClass
		{
			public TValue Property
			{
				get;
				set;
			}
		}

		private readonly Random rand = new Random (42);
	}
}