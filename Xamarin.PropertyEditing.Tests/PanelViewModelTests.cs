using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class PanelViewModelTests
	{
		private class TestClass
		{
			public string Property
			{
				get;
				set;
			}
		}

		[Test]
		public async Task PropertiesAddedFromEditor ()
		{
			var provider = new ReflectionEditorProvider ();
			var obj = new TestClass();
			var editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			var vm = new PanelViewModel (provider);
			vm.SelectedObjects.Add (obj);

			Assert.That (vm.Properties, Is.Not.Empty);
			Assert.That (vm.Properties[0].Property, Is.EqualTo (editor.Properties.Single()));
		}
	}
}