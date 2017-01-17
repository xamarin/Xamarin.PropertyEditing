using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public async Task ValueChanged ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var mockEditor = await CreateMockEditor (obj);
			var property = mockEditor.Object.Properties.First ();
			var vm = GetViewModel (property, new[] { mockEditor.Object });

			mockEditor.Setup (e => e.GetValueAsync<TValue> (property, null))
				.Returns ((IPropertyInfo p, PropertyVariation v) => Task.FromResult (new ValueInfo<TValue> { Value = testValue }));

			mockEditor.Raise (e => e.PropertyChanged += null, new EditorPropertyChangedEventArgs (property));

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		public async Task AllValuesChanged ()
		{
			TValue testValue = GetRandomTestValue ();

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var mockEditor = await CreateMockEditor (obj);
			var property = mockEditor.Object.Properties.First ();
			var vm = GetViewModel (property, new[] { mockEditor.Object });

			mockEditor.Setup (e => e.GetValueAsync<TValue> (property, null))
				.Returns ((IPropertyInfo p, PropertyVariation v) => Task.FromResult (new ValueInfo<TValue> { Value = testValue }));

			mockEditor.Raise (e => e.PropertyChanged += null, new EditorPropertyChangedEventArgs (null));

			Assert.That (vm.Value, Is.EqualTo (testValue));
		}

		[Test]
		[Description ("Once an editor is removed we should not listen for its property changes")]
		public async Task UnsubscribedValueChanged ()
		{
			TValue testValue = GetRandomTestValue ();
			Assume.That (testValue, Is.Not.EqualTo (default(TValue)));

			var obj = new TestClass ();
			Assume.That (obj.Property, Is.EqualTo (default (TValue)));

			var vm = await GetBasicTestModelAsync (obj);
			Assume.That (vm.Value, Is.EqualTo (default (TValue)));

			var editor = vm.Editors.Single ();
			Assume.That (vm.Editors.Remove (editor), Is.True);
			await editor.SetValueAsync (vm.Property, new ValueInfo<TValue> { Source = ValueSource.Local, Value = testValue });

			Assert.That (vm.Value, Is.Not.EqualTo (testValue));
		}

		protected abstract TValue GetRandomTestValue (Random rand);

		protected abstract PropertyViewModel<TValue> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors);

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