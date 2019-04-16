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
	internal abstract class PropertyViewModelTests<TValue, TViewModel>
		: PropertyViewModelTests<TValue, TValue, TViewModel>
		where TViewModel : PropertyViewModel<TValue>
	{
	}

	[SingleThreaded]
	internal abstract class PropertyViewModelTests<TValue, TValueReal, TViewModel>
		where TViewModel : PropertyViewModel<TValue>
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
		public void GetValue ()
		{
			TValue testValue = GetRandomTestValue ();
			
			var vm = GetBasicTestModel (testValue);
			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task ValueTask ()
		{
			TValue testValue = GetRandomTestValue ();

			var editor = GetBasicEditor (testValue, delay: TimeSpan.FromMilliseconds (100));
			var vm = GetViewModel (editor.Properties.First (), editor);

			Assert.That (vm.ValueTask, Is.Not.Null);
			Assert.That (vm.Value, Is.EqualTo (default(TValue)));

			await vm.ValueTask;
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
		public async Task AllEditorValuesChanged ()
		{
			TValue testValue = GetRandomTestValue ();

			var property = GetPropertyMock ();
			var mockEditor = new Mock<IObjectEditor> ();
			mockEditor.SetupGet (oe => oe.Properties).Returns (new[] { property.Object });

			var vm = GetViewModel (property.Object, new[] { mockEditor.Object });

			SetupPropertyGet (mockEditor, property.Object, testValue);
			mockEditor.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (null));

			await vm.ValueTask;
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

			var editor = GetBasicEditor (otherValue, vm.Property);
			vm.Editors.Add (editor);

			Assert.That (vm.Value, Is.EqualTo (default(TValue)));
			Assert.That (vm.MultipleValues, Is.True);
		}

		[Test]
		public void MultipleValuesNull ()
		{
			var basicEditor = GetBasicEditor ();
			var prop = basicEditor.Properties.First ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (o => o.Target).Returns (new TestClass ());
			editor.SetupGet (oe => oe.Properties).Returns (new [] { prop });
			editor.Setup (oe => oe.GetValueAsync<TValue> (prop, null)).ReturnsAsync ((ValueInfo<TValue>)null);
			var e2 = new Mock<IObjectEditor> ();
			e2.SetupGet (o => o.Target).Returns (new TestClass ());
			e2.SetupGet (oe => oe.Properties).Returns (new [] { prop });
			e2.Setup (oe => oe.GetValueAsync<TValue> (prop, null)).ReturnsAsync ((ValueInfo<TValue>)null);
			var vm = GetViewModel (basicEditor.Properties.First (), new [] { e2.Object, editor.Object });

			Assert.That (vm.Value, Is.EqualTo (default (TValue)));
			Assert.That (vm.MultipleValues, Is.False);
		}

		[Test]
		public void MultipleValuesOneNull ()
		{
			TValue value = GetNonDefaultRandomTestValue ();

			var basicEditor = GetBasicEditor (value);
			var prop = basicEditor.Properties.First ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (o => o.Target).Returns (new TestClass ());
			editor.SetupGet (oe => oe.Properties).Returns (new [] { prop });
			editor.Setup (oe => oe.GetValueAsync<TValue> (prop, null)).ReturnsAsync ((ValueInfo<TValue>)null);
			var vm = GetViewModel (basicEditor.Properties.First (), new [] { basicEditor, editor.Object });

			Assert.That (vm.Value, Is.EqualTo (default (TValue)));
			Assert.That (vm.MultipleValues, Is.True);
		}

		[Test]
		public void EmptyEditorList ()
		{
			TValue value = GetNonDefaultRandomTestValue ();

			var vm = GetBasicTestModel (value);
			Assume.That (vm.Value, Is.EqualTo (value));
			vm.Editors.Remove (vm.Editors.First ());

			Assert.That (vm.Value, Is.EqualTo (default (TValue)));
			Assert.That (vm.MultipleValues, Is.False);
		}

		[Test]
		public void MultipleValuesDontMatchButSourcesDo ()
		{
			TValue value = GetNonDefaultRandomTestValue ();
			TValue otherValue = GetRandomTestValue ();
			while (Equals (otherValue, value))
				otherValue = GetRandomTestValue ();

			var vm = GetBasicTestModel (value);
			Assume.That (vm.Value, Is.EqualTo (value));

			var editor = GetBasicEditor (otherValue, vm.Property);
			vm.Editors.Add (editor);

			Assume.That (vm.Value, Is.EqualTo (default(TValue)));
			Assume.That (vm.MultipleValues, Is.True);

			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));
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

			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Unknown));
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
			var vm = GetViewModel (mockProperty.Object, new[] { GetBasicEditor (mockProperty.Object) });

			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False);
		}

		[Test]
		public void CanSetValueToResource ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Resource);

			var resource = new Resource ("name");

			var editor = GetBasicEditor (mockProperty.Object);

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (editor.Target, mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourcesMock.Object), mockProperty.Object, new[] { editor });
			Assume.That (vm.SupportsResources, Is.True, "Does not support resources");
			Assume.That (vm.SetValueResourceCommand, Is.Not.Null);
			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.True, "Could not set value to resource");
		}

		[Test]
		public void CantSetReadonlyPropertyToResource ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);

			var resource = new Resource ("name");

			var editor = GetBasicEditor (mockProperty.Object);
			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (editor.Target, mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourcesMock.Object), mockProperty.Object, new[] { editor });
			Assume.That (vm.SetValueResourceCommand, Is.Not.Null);

			Assert.That (vm.SetValueResourceCommand.CanExecute (resource), Is.False, "Could set value to readonly resource");
		}


		[Test]
		public void CanRequestResource()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Resource);

			var resourcesMock = new Mock<IResourceProvider>();

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourcesMock.Object), mockProperty.Object, new[] { GetBasicEditor (mockProperty.Object) });

			Assert.That (vm.RequestResourceCommand.CanExecute (null), Is.True);
		}
		
		[Test]
		public void CanRequestResourceNoProvider()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (true);

			var resource = new Resource ("name");

			var editor = GetBasicEditor (mockProperty.Object);

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (editor.Target, mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			Assert.That (vm.RequestResourceCommand.CanExecute (null), Is.False);
		}
	
		[Test]
		public void SetValueToResource ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.CanWrite).Returns (false);

			var resource = new Resource ("name");
			var value = GetNonDefaultRandomTestValue ();

			var editor = new MockObjectEditor (mockProperty.Object);

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (editor.Target, mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			editor.ValueEvaluator = (info, val, source) => {
				if ((Resource)source == resource)
					return value;

				return default(TValue);
			};

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourcesMock.Object), mockProperty.Object, new[] { editor });
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
				SourceDescriptor = resource,
				Value = value
			});

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourcesMock.Object), mockProperty.Object, new[] { editor });
			
			Assert.That (vm.Value, Is.EqualTo (value));
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Resource));
		}

		[Test]
		public async Task ConvertToLocalValue ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();

			var resource = new Resource ("name");

			var resourcesMock = new Mock<IResourceProvider> ();
			resourcesMock.Setup (rp => rp.GetResourcesAsync (new object(), mockProperty.Object, It.IsAny<CancellationToken> ())).ReturnsAsync (new[] { resource });

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Resource,
				SourceDescriptor = resource,
				Value = value
			});

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourcesMock.Object), mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (value));
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Resource));

			Assert.That (vm.ConvertToLocalValueCommand.CanExecute (null), Is.True);

			bool changed = false;
			vm.ConvertToLocalValueCommand.CanExecuteChanged += (o, e) => changed = true;
			vm.ConvertToLocalValueCommand.Execute (null);

			Assert.That (vm.Value, Is.EqualTo (value));
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));
			Assert.That (changed, Is.True, "CanExecuteChanged didn't fire"); // Converitng to local should make the command unexecutable because its now already local
		}

		[Test]
		public async Task ConvertToLocalValueAlreadyLocal ()
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
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));

			Assert.That (vm.ConvertToLocalValueCommand.CanExecute (null), Is.False);
		}

		[Test]
		public async Task ConvertToLocalValueUnset ()
		{
			var mockProperty = GetPropertyMock ();

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Unset
			});

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Unset));

			Assert.That (vm.ConvertToLocalValueCommand.CanExecute (null), Is.False);
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
		public async Task ValueNotChangedForSameValueDifferentSource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Resource,
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
			SetupPropertyGet (editorMock, mockProperty.Object, value);

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
			SetupPropertyGet (editorMock, mockProperty2.Object, () => new ValueInfo<TValue> { Source = ValueSource.Local, Value = property2Value });
			SetupPropertyGet (editorMock, mockProperty1.Object, () => new ValueInfo<TValue> { Source = ValueSource.Local, Value = property1Value });

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

		[Test]
		public void CustomExpression ()
		{
			var value = GetNonDefaultRandomTestValue ();
			string custom = value.ToString ();

			var platform = new TargetPlatform (new MockEditorProvider()) {
				SupportsCustomExpressions = true
			};

			var mockProperty = GetPropertyMock ();

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Properties).Returns (new[] { mockProperty.Object });
			SetupPropertyGet (editor, mockProperty.Object, default(TValue));

			var vm = GetViewModel (platform, mockProperty.Object, new[] { editor.Object });
			Assume.That (vm.CustomExpression, Is.Null);

			vm.CustomExpression = custom;
			editor.Verify (oe => oe.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				CustomExpression = custom
			}, null));
		}

		[Test]
		[Description ("Since CustomExpression != null is the determiner for whether to set a custom expression, we need to distinguish when its being set as null")]
		public void CustomExpressionNullUnsets ()
		{
			var value = GetNonDefaultRandomTestValue ();
			string custom = value.ToString ();

			var platform = new TargetPlatform (new MockEditorProvider()) {
				SupportsCustomExpressions = true
			};

			var mockProperty = GetPropertyMock ();

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Properties).Returns (new[] { mockProperty.Object });

			SetupPropertyGet (editor, mockProperty.Object, () => new ValueInfo<TValue> {
				CustomExpression = custom
			});

			var vm = GetViewModel (platform, mockProperty.Object, new[] { editor.Object });
			Assume.That (vm.CustomExpression, Is.EqualTo (custom));

			vm.CustomExpression = null;
			editor.Verify (oe => oe.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Unset
			}, null));
		}

		[Test]
		public void CoercedProperty ()
		{
			var value = GetNonDefaultRandomTestValue ();
			var value2 = GetRandomTestValue (value);

			var mockProperty = GetPropertyMock ();
			var coerce = mockProperty.As<ICoerce<TValue>> ();
			coerce.Setup (v => v.CoerceValue (value)).Returns (value2);

			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (oe => oe.Properties).Returns (new[] { mockProperty.Object });
			SetupPropertySetAndGet (editor, mockProperty.Object);

			var vm = GetViewModel (mockProperty.Object, editor.Object);
			vm.Value = value;
			Assert.That (vm.Value, Is.EqualTo (value2));
		}

		[Test]
		public async Task ValidatedProperty ()
		{
			var value = GetNonDefaultRandomTestValue ();
			var value2 = GetRandomTestValue (value);

			var mockProperty = GetPropertyMock ();
			var validator = mockProperty.As<IValidator<TValue>> ();
			validator.Setup (v => v.IsValid (It.IsAny<TValue> ())).Returns ((TValue v) => !Equals (v, value));

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Value = value2,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (mockProperty.Object, editor);
			Assume.That (vm.Value, Is.EqualTo (value2));

			bool changed = false;
			vm.PropertyChanged += (sender, args) => {
				if (args.PropertyName == nameof(PropertyViewModel<TValue>.Value))
					changed = true;
			};

			vm.Value = value;
			Assert.That (vm.Value, Is.EqualTo (value2));
			Assert.That (changed, Is.True);
		}

		[Test]
		public async Task NavigateToSource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			var nav = mockProperty.As<ICanNavigateToSource> ();
			nav.Setup (n => n.CanNavigateToSource (It.IsAny<IObjectEditor> ())).Returns (true);

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (mockProperty.Object, editor);
			Assume.That (vm.Value, Is.EqualTo (value));
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));

			Assert.That (vm.SupportsValueSourceNavigation, Is.True);
			Assert.That (vm.NavigateToValueSourceCommand.CanExecute (null), Is.True);

			vm.NavigateToValueSourceCommand.Execute (null);
			nav.Verify (n => n.NavigateToSource (editor));
		}

		[Test]
		public async Task CantNavigateToMultipleObjectValueSources ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			var nav = mockProperty.As<ICanNavigateToSource> ();
			nav.Setup (n => n.CanNavigateToSource (It.IsAny<IObjectEditor> ())).Returns (true);

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});

			var editor2 = new MockObjectEditor (mockProperty.Object);
			await editor2.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (mockProperty.Object, new[] { editor, editor2 });
			Assume.That (vm.Value, Is.EqualTo (value));
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));

			Assert.That (vm.SupportsValueSourceNavigation, Is.True);
			Assert.That (vm.NavigateToValueSourceCommand.CanExecute (null), Is.False, "Navigate not disabled when multi-selecting");
		}

		[Test]
		public async Task CantNavigateToValueSource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			var nav = mockProperty.As<ICanNavigateToSource> ();
			nav.Setup (n => n.CanNavigateToSource (It.IsAny<IObjectEditor> ())).Returns (false);

			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (value));
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Local));

			Assert.That (vm.SupportsValueSourceNavigation, Is.True);
			Assert.That (vm.NavigateToValueSourceCommand.CanExecute (null), Is.False, "Navigate not disabled when unnavigable");
		}

		[Test]
		public async Task CanNavigateToSourceUpdated ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			var nav = mockProperty.As<ICanNavigateToSource> ();
			nav.Setup (n => n.CanNavigateToSource (It.IsAny<IObjectEditor> ())).Returns (true);

			var editor = new MockObjectEditor (mockProperty.Object);

			var vm = GetViewModel (mockProperty.Object, new[] { editor });
			Assume.That (vm.Value, Is.EqualTo (default(TValue)));
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Default).Or.EqualTo (ValueSource.Unset));
			Assume.That (vm.SupportsValueSourceNavigation, Is.True);

			Assert.That (vm.NavigateToValueSourceCommand.CanExecute (null), Is.False, "Navigate not disabled when unset/default");

			bool changed = false;
			vm.NavigateToValueSourceCommand.CanExecuteChanged += (o, e) => { changed = true; };

			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});

			Assert.That (changed, Is.True, "CanExecuteChanged did not fire");
			Assert.That (vm.NavigateToValueSourceCommand.CanExecute (null), Is.True, "Navigate not enabled once value source became valid");
		}

		[Test]
		public void CanCreateResource ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local | ValueSources.Resource);
			var editor = new MockObjectEditor (mockProperty.Object);

			var resourceProvider = new Mock<IResourceProvider> ();
			resourceProvider.SetupGet (rp => rp.CanCreateResources).Returns (true);

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourceProvider.Object), mockProperty.Object, new[] { editor });
			Assert.That (vm.CanCreateResources, Is.True);
		}

		[Test]
		public async Task CanRequestCreateResource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local | ValueSources.Resource);
			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Local,
				Value = value
			});

			var resourceProvider = new Mock<IResourceProvider> ();
			resourceProvider.SetupGet (rp => rp.CanCreateResources).Returns (true);

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourceProvider.Object), mockProperty.Object, new[] { editor });
			Assert.That (vm.RequestCreateResourceCommand.CanExecute (null), Is.True, "Can't create resources");
		}

		[Test]
		public async Task CanRequestCreateResourceMultipleValues ()
		{
			var value = GetNonDefaultRandomTestValue ();
			var value2 = GetRandomTestValue (notValue: value);

			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local | ValueSources.Resource);
			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Local,
				Value = value
			});
			var editor2 = new MockObjectEditor (mockProperty.Object);
			await editor2.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Local,
				Value = value2
			});

			var resourceProvider = new Mock<IResourceProvider> ();
			resourceProvider.SetupGet (rp => rp.CanCreateResources).Returns (true);

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourceProvider.Object), mockProperty.Object, new[] { editor });
			Assume.That (vm.RequestCreateResourceCommand.CanExecute (null), Is.True, "Can't create resources initially");

			bool changed = false;
			vm.RequestCreateResourceCommand.CanExecuteChanged += (sender, args) => {
				changed = true;
			};

			vm.Editors.Add (editor2);
			Assume.That (vm.MultipleValues, Is.True);
			Assert.That (changed, Is.True, "RequestCreateResourceCommand didn't change can execute");
			Assert.That (vm.RequestCreateResourceCommand.CanExecute (null), Is.False, "CanCreateResources was true after differing value added");
		}

		[Test]
		public async Task CreateResource ()
		{
			var value = GetNonDefaultRandomTestValue ();

			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local | ValueSources.Resource);
			var editor = new MockObjectEditor (mockProperty.Object);
			await editor.SetValueAsync (mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Local,
				Value = value
			});

			var resourceProvider = new Mock<IResourceProvider> ();
			resourceProvider.SetupGet (rp => rp.CanCreateResources).Returns (true);

			var vm = GetViewModel (new TargetPlatform (new MockEditorProvider (), resourceProvider.Object), mockProperty.Object, new[] { editor });
			Assume.That (vm.RequestCreateResourceCommand.CanExecute (null), Is.True, "Can't create resources");

			bool requested = false;
			vm.CreateResourceRequested += (o, e) => {
				requested = true;
			};

			vm.RequestCreateResourceCommand.Execute (null);
			Assert.That (requested, Is.True, "CreateResourceRequested did not fire");
		}

		[TestCase (true, true, true)]
		[TestCase (false, true, false)]
		[TestCase (true, false, false)]
		public void AutocompleteEnabled (bool customExpressions, bool hasInterface, bool expected)
		{
			var target = new object ();
			var property = GetPropertyMock ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			SetupPropertySetAndGet (editor, property.Object);

			if (hasInterface) {
				string[] results = { "Foo", "Bar", "Baz" };
				var complete = editor.As<ICompleteValues> ();
				complete.Setup (c => c.GetCompletionsAsync (property.Object, It.IsAny<string> (), It.IsAny<CancellationToken> ()))
					.ReturnsAsync (results);
			}

			var mockProvider = new MockEditorProvider (editor.Object);
			var resources = new MockResourceProvider ();
			var platform = new TargetPlatform (mockProvider, resources) {
				SupportsCustomExpressions = customExpressions
			};

			var vm = GetViewModel (platform, property.Object, new[] { editor.Object });
			Assert.That (vm.SupportsAutocomplete, Is.EqualTo (expected));
		}

		[Test]
		public void AutocompleteResults ()
		{
			var target = new object ();
			var property = GetPropertyMock ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			SetupPropertySetAndGet (editor, property.Object);

			string[] results = new[] { "Foo", "Bar", "Baz" };
			var complete = editor.As<ICompleteValues> ();
			complete.Setup (c => c.GetCompletionsAsync (property.Object, It.IsAny<string> (), It.IsAny<CancellationToken> ())).ReturnsAsync (results);

			var mockProvider = new MockEditorProvider (editor.Object);
			var resources = new MockResourceProvider();
			var platform = new TargetPlatform (mockProvider, resources) {
				SupportsCustomExpressions = true
			};

			var vm = GetViewModel (platform, property.Object, new[] { editor.Object });
			Assume.That (vm.SupportsAutocomplete, Is.True);
			vm.PreviewCustomExpression = "preview";

			CollectionAssert.AreEqual (vm.AutocompleteItems, results);
			complete.Verify (c => c.GetCompletionsAsync (property.Object, "preview", It.IsAny<CancellationToken> ()));
		}

		[Test]
		public void AutocompleteCancels ()
		{
			var target = new object ();
			var property = GetPropertyMock ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			SetupPropertySetAndGet (editor, property.Object);

			string[] results = new[] { "Foo", "Bar", "Baz" };
			var tcs = new TaskCompletionSource<IReadOnlyList<string>> ();

			var complete = editor.As<ICompleteValues> ();
			complete.Setup (c => c.GetCompletionsAsync (property.Object, It.IsAny<string> (), It.IsAny<CancellationToken> ()))
				.Returns<IPropertyInfo,string,CancellationToken> ((a,b,c) => {
					c.Register (() => {
						tcs.TrySetCanceled ();
					});
					return tcs.Task;
				});

			var mockProvider = new MockEditorProvider (editor.Object);
			var resources = new MockResourceProvider ();
			var platform = new TargetPlatform (mockProvider, resources) {
				SupportsCustomExpressions = true
			};

			var vm = GetViewModel (platform, property.Object, new[] { editor.Object });
			Assume.That (vm.SupportsAutocomplete, Is.True);
			vm.PreviewCustomExpression = "preview";

			vm.PreviewCustomExpression = "attempt2";
			Assert.That (tcs.Task.IsCanceled, Is.True);
		}

		[Test]
		public void AutocompleteResults1NotUpdatedWhen2ndSearchCancelled ()
		{
			var target = new object ();
			var property = GetPropertyMock ();

			var editor1 = new Mock<IObjectEditor> ();
			editor1.SetupGet (e => e.Target).Returns (target);
			SetupPropertySetAndGet (editor1, property.Object);

			var tcs = new TaskCompletionSource<IReadOnlyList<string>> ();

			string[] results = new[] { "Foo", "Bar", "Baz" };
			string[] results2 = new[] { "Foo2", "Bar2", "Baz2" };
			var changeResultSet = false;
			var complete1 = editor1.As<ICompleteValues> ();
			complete1.Setup (c => c.GetCompletionsAsync (property.Object, It.IsAny<string> (), It.IsAny<CancellationToken> ()))
				.Returns<IPropertyInfo, string, CancellationToken> ((a, b, c) => {
					if (!changeResultSet) {
						tcs.TrySetResult (results);
					} else {
						tcs.TrySetResult (results2);
					}
					return tcs.Task;
				 });

			var editor2 = new Mock<IObjectEditor> ();
			editor2.SetupGet (e => e.Target).Returns (target);
			SetupPropertySetAndGet (editor2, property.Object);

			var complete2 = editor2.As<ICompleteValues> ();
			complete2.Setup (c => c.GetCompletionsAsync (property.Object, It.IsAny<string> (), It.IsAny<CancellationToken> ()))
				.Returns<IPropertyInfo, string, CancellationToken> ((a, b, c) => {
					c.Register (() => {
						tcs.TrySetCanceled ();
					});
					return tcs.Task;
				});

			var mockProvider = new MockEditorProvider (editor1.Object);
			var resources = new MockResourceProvider ();
			var platform = new TargetPlatform (mockProvider, resources) {
				SupportsCustomExpressions = true,
			};

			var vm = GetViewModel (platform, property.Object, new[] { editor1.Object, editor2.Object });
			Assume.That (vm.SupportsAutocomplete, Is.True);

			vm.PreviewCustomExpression = "preview";

			changeResultSet = true;
			Assume.That (vm.AutocompleteItems[0] == results[0], Is.True);
			Assume.That (vm.AutocompleteItems[1] == results[1], Is.True);
			Assume.That (vm.AutocompleteItems[2] == results[2], Is.True);

			vm.PreviewCustomExpression = "preview2";

			CollectionAssert.AreEqual (vm.AutocompleteItems, results);
		}

		[Test]
		public void DoesntHaveInputModes ()
		{
			var vm = GetBasicTestModel ();
			Assert.That (vm.HasInputModes, Is.False);
		}

		[Test]
		public void HasInputModes ()
		{
			var modes = new[] { new InputMode ("TestMode") };

			var property = GetPropertyMock ();
			var input = property.As<IHaveInputModes> ();
			input.SetupGet (im => im.InputModes).Returns (modes);

			var vm = GetViewModel (property.Object, new MockObjectEditor (property.Object));

			Assert.That (vm.HasInputModes, Is.True, "HasInputModes was false");
			Assert.That (vm.InputModes, Contains.Item (modes[0]));
			Assert.That (vm.InputModes.Count, Is.EqualTo (1));
			Assert.That (vm.InputMode, Is.EqualTo (modes[0]), "InputMode not set to a default on no value set for it");
		}

		[Test]
		public void InputModeCommits ()
		{
			var modes = new[] { new InputMode ("TestMode"), new InputMode("TestMode2"),  };

			var property = GetPropertyMock ();
			var input = property.As<IHaveInputModes> ();
			input.SetupGet (im => im.InputModes).Returns (modes);

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			editor.SetupGet (e => e.Properties).Returns (new[] { property.Object });
			SetupPropertySetAndGet (editor, property.Object);

			var vm = GetViewModel (property.Object, editor.Object);
			Assume.That (vm.InputMode, Is.EqualTo (modes[0]));

			vm.InputMode = modes[1];
			editor.Verify (oe => oe.SetValueAsync (property.Object, It.Is<ValueInfo<TValue>> (vi => vi.ValueDescriptor == modes[1]), It.IsAny<PropertyVariation> ()));
		}

		[TestCase (1)]
		[TestCase (2)]
		public async Task InputModeRestores (int mode)
		{
			var modes = new[] { new InputMode ("TestMode"), new InputMode ("TestMode2"), new InputMode ("TestMode3", true) };

			var property = GetPropertyMock ();
			var input = property.As<IHaveInputModes> ();
			input.SetupGet (im => im.InputModes).Returns (modes);

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			editor.SetupGet (e => e.Properties).Returns (new[] { property.Object });
			SetupPropertySetAndGet (editor, property.Object);

			TValue value = GetRandomTestValue ();

			await editor.Object.SetValueAsync (property.Object, new ValueInfo<TValue> {
				Value = value,
				ValueDescriptor = modes[mode]
			});

			var vm = GetViewModel (property.Object, editor.Object);
			Assert.That (vm.InputMode, Is.EqualTo (modes[mode]));
			Assert.That (vm.IsInputEnabled, Is.EqualTo (!modes[mode].IsSingleValue));
		}

		[TestCase (1)]
		[TestCase (2)]
		public async Task InputModeUpdates (int mode)
		{
			var modes = new[] { new InputMode ("TestMode"), new InputMode ("TestMode2"), new InputMode ("TestMode3", true) };

			var property = GetPropertyMock ();
			var input = property.As<IHaveInputModes> ();
			input.SetupGet (im => im.InputModes).Returns (modes);

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			editor.SetupGet (e => e.Properties).Returns (new[] { property.Object });
			SetupPropertySetAndGet (editor, property.Object);

			TValue value = GetRandomTestValue ();

			var vm = GetViewModel (property.Object, editor.Object);

			bool modeChanged = false, enabledChanged = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(PropertyViewModel<TValue>.InputMode))
					modeChanged = true;
				else if (e.PropertyName == nameof(PropertyViewModel<TValue>.IsInputEnabled))
					enabledChanged = true;
			};

			await editor.Object.SetValueAsync (property.Object, new ValueInfo<TValue> {
				Value = value,
				ValueDescriptor = modes[mode]
			});

			Assert.That (vm.InputMode, Is.EqualTo (modes[mode]));
			Assert.That (modeChanged, Is.True);
			Assert.That (vm.IsInputEnabled, Is.EqualTo (!modes[mode].IsSingleValue));
			Assert.That (enabledChanged, Is.True);
		}

		[TestCase (true)]
		[TestCase (false)]
		public void InputEnabled (bool writeEnabled)
		{
			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.CanWrite).Returns (writeEnabled);

			var vm = GetViewModel (property.Object, new MockObjectEditor (property.Object));
			Assert.That (vm.IsInputEnabled, Is.EqualTo (writeEnabled));
		}

		[TestCase (true, false, true)]
		[TestCase (true, true, false)]
		[TestCase (false, true, false)]
		[TestCase (false, false, false)]
		public void InputEnabledSingleValueInputMode (bool writeEnabled, bool singleValue, bool expectation)
		{
			var modes = new[] { new InputMode ("TestMode"), new InputMode ("TestMode2", singleValue), };

			var property = GetPropertyMock ();
			property.SetupGet (pi => pi.CanWrite).Returns (writeEnabled);
			var input = property.As<IHaveInputModes> ();
			input.SetupGet (im => im.InputModes).Returns (modes);

			var target = new object ();
			var editor = new Mock<IObjectEditor> ();
			editor.SetupGet (e => e.Target).Returns (target);
			editor.SetupGet (e => e.Properties).Returns (new[] { property.Object });
			SetupPropertySetAndGet (editor, property.Object);

			var vm = GetViewModel (property.Object, editor.Object);
			Assume.That (vm.InputMode, Is.EqualTo (modes[0]));
			Assume.That (vm.IsInputEnabled, Is.EqualTo (writeEnabled), "Initial state didn't match property");

			bool changed = false;
			vm.PropertyChanged += (o, e) => {
				if (e.PropertyName == nameof(PropertyViewModel<TValue>.IsInputEnabled))
					changed = true;
			};

			vm.InputMode = modes[1];

			Assert.That (changed, Is.True);
			Assert.That (vm.IsInputEnabled, Is.EqualTo (expectation));
		}

		[Test]
		public void HasVariations ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (v => v.Variations).Returns (new[] { new PropertyVariationOption ("Category", "Value") });
			var editor = new MockObjectEditor (mockProperty.Object);

			var vm = GetViewModel (mockProperty.Object, editor);
			Assert.That (vm.HasVariations, Is.True);
		}

		[Test]
		public void CreateBinding ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local | ValueSources.Binding);

			var editor = new Mock<IObjectEditor> ();
			SetupPropertySetAndGet (editor, mockProperty.Object);

			var vm = GetViewModel (mockProperty.Object, editor.Object);

			var bindObject = new object ();

			bool requested = false;
			vm.CreateBindingRequested += (o, e) => {
				requested = true;
				e.BindingObject = bindObject;
			};

			vm.RequestCreateBindingCommand.Execute (null);
			Assert.That (requested, Is.True, "Binding wasn't requested");
			Assert.That (vm.ValueSource, Is.EqualTo (ValueSource.Binding));

			editor.Verify (oe => oe.SetValueAsync (mockProperty.Object, It.Is<ValueInfo<TValue>> (vi =>
				vi.Source == ValueSource.Binding && vi.SourceDescriptor == bindObject
			), It.IsAny<PropertyVariation> ()));
		}

		[Test]
		public void CreateBindingExistingPreserved ()
		{
			var mockProperty = GetPropertyMock ();
			mockProperty.SetupGet (pi => pi.ValueSources).Returns (ValueSources.Default | ValueSources.Local | ValueSources.Binding);

			var bindObject = new object ();

			var editor = new Mock<IObjectEditor> ();
			SetupPropertySetAndGet (editor, mockProperty.Object, new ValueInfo<TValue> {
				Source = ValueSource.Binding,
				SourceDescriptor = bindObject
			});

			var vm = GetViewModel (mockProperty.Object, editor.Object);
			Assume.That (vm.ValueSource, Is.EqualTo (ValueSource.Binding));

			bool requested = false;
			vm.CreateBindingRequested += (o, e) => {
				requested = true;
				Assert.That (e.BindingObject, Is.SameAs (bindObject));
			};

			vm.RequestCreateBindingCommand.Execute (null);
			Assert.That (requested, Is.True, "Binding wasn't requested");
		}

		protected TViewModel GetViewModel (IPropertyInfo property, IObjectEditor editor)
		{
			return GetViewModel (property, new[] { editor });
		}

		protected TViewModel GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return GetViewModel (MockEditorProvider.MockPlatform, property, editors);
		}

		protected abstract TViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors);

		protected virtual void AugmentPropertyMock (Mock<IPropertyInfo> propertyMock)
		{
		}

		protected virtual void SetupPropertyGet (Mock<IObjectEditor> editorMock, IPropertyInfo property, TValue value)
		{
			editorMock.Setup (oe => oe.GetValueAsync<TValue> (property, null)).ReturnsAsync (new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			});
		}

		protected virtual void SetupPropertySetAndGet (Mock<IObjectEditor> editorMock, IPropertyInfo property, TValue value = default(TValue))
		{
			ValueInfo<TValue> valueInfo = new ValueInfo<TValue> {
				Value = value,
				Source = (Equals (value, default(TValue))) ? ValueSource.Default : ValueSource.Local
			};

			SetupPropertySetAndGet (editorMock, property, valueInfo);
		}

		protected virtual void SetupPropertySetAndGet (Mock<IObjectEditor> editorMock, IPropertyInfo property, ValueInfo<TValue> valueInfo)
		{
			editorMock.Setup (oe => oe.SetValueAsync (property, It.IsAny<ValueInfo<TValue>> (), null))
				.Callback<IPropertyInfo, ValueInfo<TValue>, PropertyVariation> ((p, vi, v) => {
					valueInfo = vi;
					editorMock.Raise (oe => oe.PropertyChanged += null, new EditorPropertyChangedEventArgs (property));
				})
				.Returns (Task.FromResult (true));
			editorMock.Setup (oe => oe.GetValueAsync<TValue> (property, null)).ReturnsAsync (() => valueInfo);
		}

		protected virtual void SetupPropertyGet (Mock<IObjectEditor> editorMock, IPropertyInfo property, Func<ValueInfo<TValue>> value)
		{
			editorMock.Setup (oe => oe.GetValueAsync<TValue> (property, null)).ReturnsAsync (value);
		}

		protected internal Mock<IPropertyInfo> GetPropertyMock (string name = null, string category = null)
		{
			var mock = new Mock<IPropertyInfo> ();
			mock.SetupGet (pi => pi.Type).Returns (typeof(TValueReal));
			mock.SetupGet (pi => pi.Name).Returns (name);
			mock.SetupGet (pi => pi.Category).Returns (category);
			mock.SetupGet (pi => pi.CanWrite).Returns (true);
			AugmentPropertyMock (mock);

			return mock;
		}

		protected Random Random => this.rand;

		protected TValue GetNonDefaultRandomTestValue ()
		{
			TValue value = default (TValue);
			while (Equals (value, default (TValue))) {
				value = GetRandomTestValue (Random);
			}

			return value;
		}

		protected abstract TValue GetRandomTestValue (Random rand);

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

		protected internal MockObjectEditor GetBasicEditor (IPropertyInfo property = null, TimeSpan? delay = null)
		{
			if (property == null) {
				var propertyMock = GetPropertyMock ();
				propertyMock.SetupGet (p => p.Name).Returns (nameof(TestClass.Property));

				property = propertyMock.Object;
			}

			var editor = new MockObjectEditor (property) {
				Target = new TestClass(),
				Delay = delay
			};

			return editor;
		}

		protected internal MockObjectEditor GetBasicEditor (TValue value, IPropertyInfo property = null, TimeSpan? delay = null)
		{
			var editor = GetBasicEditor (property, delay);
			editor.SetValueAsync (editor.Properties.First (), new ValueInfo<TValue> {
				Value = value,
				Source = ValueSource.Local
			}).Wait();
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
		private TestContext syncContext;
	}
}
