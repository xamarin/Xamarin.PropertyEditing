using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			Assume.That (obj.Property, Is.Null);

			var vm = await GetBasicTestModelAsync (obj);
			Assume.That (vm.Value, Is.Null);

			vm.Value = testValue;
			Assert.That (obj.Property, Is.EqualTo (testValue));
		}

		protected abstract TValue GetRandomTestValue (Random rand);

		protected abstract PropertyViewModel<TValue> GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors);

		private readonly Random rand = new Random (42);

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