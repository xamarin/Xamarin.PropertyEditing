using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[SingleThreaded]
	internal abstract class PropertyViewModelTests<TValue, TViewModel>
		where TViewModel : PropertyViewModel<TValue>
	{

		[SetUp]
		public void Setup ()
		{
			this.syncContext = new AsyncSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext (this.syncContext);
		}

		[TearDown]
		public void TearDown ()
		{
			this.syncContext.WaitForPendingOperationsToComplete ();
			SynchronizationContext.SetSynchronizationContext (null);
		}

		[Test]
		public void GetValue ()
		{
			TValue testValue = GetRandomTestValue ();
			
			var vm = GetBasicTestModel (testValue);
			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task SetValue ()
		{
			TValue testValue = GetRandomTestValue ();

			var vm = GetBasicTestModel();
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			vm.Value = testValue;

			ValueInfo<TValue> valueInfo = await vm.Editors.First ().GetValueAsync<TValue> (vm.Property);
			Assert.That (valueInfo.Value, Is.EqualTo (testValue));
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
		public async Task SetSameValueMultiEditor ()
		{
			TValue testValue = GetRandomTestValue ();

			var vm = GetBasicTestModel ();
			vm.Editors.Add (GetBasicEditor (vm.Property));
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));

			vm.Value = testValue;

			ValueInfo<TValue> valueInfo = await vm.Editors.First ().GetValueAsync<TValue> (vm.Property);
			Assert.That (valueInfo.Value, Is.EqualTo (testValue));

			valueInfo = await vm.Editors.Skip (1).First ().GetValueAsync<TValue> (vm.Property);
			Assert.That (valueInfo.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task EditorValueChanged ()
		{
			TValue testValue = GetRandomTestValue ();
			var vm = GetBasicTestModel();

			await vm.Editors.First().SetValueAsync (vm.Property, new ValueInfo<TValue> {
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
		public void ValueSourceDefaultWhenValuesDisagree ()
		{
			TValue value = GetNonDefaultRandomTestValue ();
			TValue otherValue = GetRandomTestValue ();
			while (Equals (otherValue, value))
				otherValue = GetRandomTestValue ();

			var vm = GetBasicTestModel (value);
			Assume.That (vm.Value, Is.EqualTo (value));

			var editor = GetBasicEditor (otherValue);

			vm.Editors.Add (editor);

			Assume.That (vm.Value, Is.EqualTo (default (TValue)));
			Assume.That (vm.MultipleValues, Is.True);

			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Default));
		}

		[Test]
		[Description ("Once an editor is removed we should not listen for its property changes")]
		public async Task UnsubscribedValueChanged ()
		{
			TValue testValue = GetNonDefaultRandomTestValue ();
			Assume.That (testValue, Is.Not.EqualTo (default(TValue)));

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var vm = GetBasicTestModel ();
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			var editor = vm.Editors.Single ();
			Assume.That (vm.Editors.Remove (editor), Is.True);
			await editor.SetValueAsync (vm.Property, new ValueInfo<TValue> { Source = ValueSource.Local, Value = testValue });

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
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

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
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			Assume.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False);

			bool changed = false;
			vm.SetValueResourceCommand.CanExecuteChanged += (o,e) => changed = true;
			vm.ResourceProvider = resourcesMock.Object;
			Assume.That (changed, Is.True);
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
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			vm.ResourceProvider = resourcesMock.Object;
			Assume.That (vm.SetValueResourceCommand, Is.Not.Null);

			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False, "Could set value to readonly resource");
		}

		[Test]
		public void CanRequestResource()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);

			var resourcesMock = new Mock<IResourceProvider>();

			var vm = GetViewModel (mockProperty.Object, new[] {  new Mock<IObjectEditor>().Object });
			vm.ResourceProvider = resourcesMock.Object;

			Assert.That (vm.RequestResourceCommand.CanExecute (null), Is.True);
		}

		[Test]
		[Description ("RequestResourceCommand's CanExecuteChanged should fire when SetValueResourceCommand's does")]
		public void CanRequestResourceSetValueChanges()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { new Mock<IObjectEditor> ().Object });
			Assume.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False);

			bool setChanged = false;
			vm.SetValueResourceCommand.CanExecuteChanged += (o,e) => setChanged = true;
			bool requestChanged = false;
			vm.RequestResourceCommand.CanExecuteChanged += (o, e) => requestChanged = true;

			vm.ResourceProvider = resourcesMock.Object;

			Assume.That (setChanged, Is.True);
			Assert.That (requestChanged, Is.True);
		}

		[Test]
		public void SetValueToResource ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);

			var resource = new Resource ("name");
			var value = GetNonDefaultRandomTestValue ();

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

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
		public async Task GetValueAlreadySetToResource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
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
		public async Task ValueNotChangedForSameValue ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
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
			editorMock.Setup (oe => oe.GetValueAsync<TValue> (mockProperty.Object, null)).Returns (Task.FromResult (new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			}));

			var vm = GetViewModel (mockProperty.Object, new[] { editorMock.Object });
			Assume.That (vm.Value, Is.EqualTo (value));

			vm.Value = value;

			editorMock.Verify (oe => oe.SetValueAsync (mockProperty.Object, It.IsAny<ValueInfo<TValue>> (), null), Times.Never);
		}

		[Test]
		[Description ("We need to ensure async value operations complete before moving on to other value operations")]
		public void ValueTriggersOtherPropertyChangeOutOfOrder ()
		{
			var mockProperty1 = GetPropertyMock ();
			var mockProperty2 = GetPropertyMock ();

			var property1Value = GetRandomTestValue ();
			var next1Value = GetRandomTestValue (notValue: property1Value);
			TValue original2Value = GetRandomTestValue ();
			var property2Value = original2Value;
			var next2Value = GetRandomTestValue (notValue: property2Value);

			var editorMock = new Mock<IObjectEditor> ();
			editorMock.SetupGet (ioe => ioe.Properties).Returns (new[] { mockProperty1.Object, mockProperty2.Object });
			editorMock.Setup (ioe => ioe.GetValueAsync<TValue> (mockProperty2.Object, null))
				.Returns (() => Task.FromResult (new ValueInfo<TValue> { Source = ValueSource.Local, Value = property2Value }));
			editorMock.Setup (ioe => ioe.GetValueAsync<TValue> (mockProperty1.Object, null))
				.Returns (() => Task.FromResult (new ValueInfo<TValue> { Source = ValueSource.Local, Value = property1Value }));

			var tcs = new TaskCompletionSource<bool> ();
			var info = new ValueInfo<TValue> { Source = ValueSource.Local, Value = next1Value };
			editorMock.Setup (ioe => ioe.SetValueAsync (mockProperty1.Object, info, null)).Callback (() => {
				property1Value = next1Value;
				property2Value = next2Value;
				editorMock.Raise (ioe => ioe.PropertyChanged += null, new EditorPropertyChangedEventArgs (mockProperty2.Object));
			}).Returns (tcs.Task);

			var vm1 = GetViewModel (mockProperty1.Object, new[] { editorMock.Object });
			var vm2 = GetViewModel (mockProperty2.Object, new[] { editorMock.Object });

			vm1.Value = next1Value;
			Assert.That (vm2.Value, Is.EqualTo (original2Value));

			tcs.SetResult (true);
			Assert.That (vm2.Value, Is.EqualTo (property2Value));
		}

		[Test]
		public void AvailabilityConstraintUnavailable ()
		{
			var constraint = new Mock<IAvailabilityConstraint> ();
			
			var prop = GetPropertyMock ();
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new List<IAvailabilityConstraint> { constraint.Object });
			
			IObjectEditor editor = GetBasicEditor (prop.Object);
			constraint.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (false);

			var vm = GetViewModel (prop.Object, new[] { editor });
			Assert.That (vm.IsAvailable, Is.False);
		}

		[Test]
		public void MultiAvailabilityConstraintUnavailable ()
		{
			var constraint1 = new Mock<IAvailabilityConstraint> ();
			var constraint2 = new Mock<IAvailabilityConstraint> ();
			
			var prop = GetPropertyMock ();
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new List<IAvailabilityConstraint> { constraint1.Object, constraint2.Object });
			
			IObjectEditor editor = GetBasicEditor (prop.Object);
			constraint1.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (false);
			constraint2.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (true);

			var vm = GetViewModel (prop.Object, new[] { editor });
			Assert.That (vm.IsAvailable, Is.False);
		}

		[Test]
		public void AvailabilityConstraintAvailable ()
		{
			var constraint = new Mock<IAvailabilityConstraint> ();
			
			var prop = GetPropertyMock ();
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new List<IAvailabilityConstraint> { constraint.Object });
			
			IObjectEditor editor = GetBasicEditor (prop.Object);
			constraint.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (true);

			var vm = GetViewModel (prop.Object, new[] { editor });
			Assert.That (vm.IsAvailable, Is.True);
		}

		[Test]
		public void MultiAvailabilityConstraintAvailable ()
		{
			var constraint1 = new Mock<IAvailabilityConstraint> ();
			var constraint2 = new Mock<IAvailabilityConstraint> ();
			
			var prop = GetPropertyMock ();
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new List<IAvailabilityConstraint> { constraint1.Object, constraint2.Object });
			
			IObjectEditor editor = GetBasicEditor (prop.Object);
			constraint1.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (true);
			constraint2.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (true);

			var vm = GetViewModel (prop.Object, new[] { editor });
			Assert.That (vm.IsAvailable, Is.True);
		}

		[Test]
		public async Task ConstrainingPropertyChangeRequeriesAvailability ()
		{
			var otherProp = GetPropertyMock ();

			var prop = GetPropertyMock ();

			var constraint = new Mock<IAvailabilityConstraint> ();
			constraint.SetupGet (c => c.ConstrainingProperties).Returns (new[] { otherProp.Object });
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new List<IAvailabilityConstraint> { constraint.Object });

			bool isAvailable = true;
			IObjectEditor editor = new MockObjectEditor (prop.Object, otherProp.Object);
			constraint.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (() => isAvailable);

			var vm = GetViewModel (prop.Object, new[] { editor });
			Assume.That (vm.IsAvailable, Is.True);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(PropertyViewModel.IsAvailable))
					changed = true;
			};

			isAvailable = false;
			await editor.SetValueAsync (otherProp.Object, new ValueInfo<TValue> {
				Value = GetRandomTestValue(),
				Source = ValueSource.Local
			});

			Assert.That (vm.IsAvailable, Is.False);
			Assert.That (changed, Is.True);
		}

		[Test]
		public async Task PropertyChangeRequeriesAvailability ()
		{
			var prop = GetPropertyMock ();

			var constraint = new Mock<IAvailabilityConstraint> ();
			constraint.SetupGet (c => c.ConstrainingProperties).Returns (new[] { prop.Object });
			prop.SetupGet (p => p.AvailabilityConstraints).Returns (new List<IAvailabilityConstraint> { constraint.Object });

			bool isAvailable = true;
			IObjectEditor editor = GetBasicEditor (prop.Object);
			constraint.Setup (c => c.GetIsAvailableAsync (editor)).ReturnsAsync (() => isAvailable);

			var vm = GetViewModel (prop.Object, new[] { editor });
			Assume.That (vm.IsAvailable, Is.True);

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(PropertyViewModel.IsAvailable))
					changed = true;
			};

			isAvailable = false;
			await editor.SetValueAsync (prop.Object, new ValueInfo<TValue> {
				Value = GetRandomTestValue(),
				Source = ValueSource.Local
			});

			Assert.That (vm.IsAvailable, Is.False);
			Assert.That (changed, Is.True);
		}

		[Test]
		public void ClearLocalValue ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local);

			var mockEditor = new MockObjectEditor {
				SupportsDefault = true,
				Properties = new [] {
					mockProperty.Object
				}
			};

			var vm = GetViewModel (mockProperty.Object, new[] { mockEditor });
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));

			Assert.That (vm.ClearValueCommand.CanExecute (null), Is.False);

			bool changed = false;
			vm.ClearValueCommand.CanExecuteChanged += (sender, args) => {
				changed = true;
			};

			vm.Value = value;
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));

			Assert.That (changed, Is.True);
			Assert.That (vm.ClearValueCommand.CanExecute (null), Is.True);

			vm.ClearValueCommand.Execute (null);

			Assert.That (vm.Value, Is.EqualTo (default(TValue)));
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

		protected TViewModel GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return GetViewModel (TargetPlatform.Default, property, editors);
		}

		protected abstract TViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors);

		protected virtual void AugmentPropertyMock (Mock<IPropertyInfo> propertyMock)
		{
		}

		protected internal Mock<IPropertyInfo> GetPropertyMock (string name = null, string category = null)
		{
			var mock = new Mock<IPropertyInfo> ();
			mock.SetupGet (pi => pi.Type).Returns (typeof(TValue));
			mock.SetupGet (pi => pi.Name).Returns (name);
			mock.SetupGet (pi => pi.Category).Returns (category);

			AugmentPropertyMock (mock);

			return mock;
		}

		protected Random Random => this.rand;

		protected internal TValue GetRandomTestValue ()
		{
			return GetRandomTestValue (this.rand);
		}

		protected internal TValue GetRandomTestValue (TValue notValue)
		{
			TValue value = GetRandomTestValue ();
			while (Equals (value, notValue)) {
				value = GetRandomTestValue ();
			}

			return value;
		}

		protected internal MockObjectEditor GetBasicEditor (IPropertyInfo property = null)
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

		protected internal MockObjectEditor GetBasicEditor (TValue value, IPropertyInfo property = null)
		{
			var editor = GetBasicEditor (property);
			editor.values[editor.Properties.First ()] = value;
			return editor;
		}

		protected internal TViewModel GetBasicTestModel ()
		{
			var editor = GetBasicEditor ();

			return GetViewModel (editor.Properties.First(), new[] { editor });
		}

		protected internal TViewModel GetBasicTestModel (TValue value)
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
		private AsyncSynchronizationContext syncContext;
	}
}