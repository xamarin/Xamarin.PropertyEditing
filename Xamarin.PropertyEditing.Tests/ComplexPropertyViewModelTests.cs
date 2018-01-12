using NUnit.Framework;
using Xamarin.PropertyEditing.Tests.MockPropertyInfo;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.Tests
{
	internal class ComplexPropertyViewModelTests
	{
		[Test]
		public async Task SubPropertyCanBeEdited ()
		{
			var property = new MockComplexPropertyInfo<int> ("Property");
			MockSubPropertyInfo<double> subProperty = property.AddSubProperty<double> ("SubProperty");
			var editor = new MockObjectEditor (property);

			var changed = false;
			editor.PropertyChanged += (s, e) => {
				if (e.Property == subProperty) {
					changed = true;
				}
			};
			Assert.AreEqual (default (double), (await editor.GetValueAsync<double> (subProperty)).Value);
			await editor.SetValueAsync (subProperty, new ValueInfo<double> { Source = ValueSource.Local, Value = 1.0 });
			Assert.IsTrue (changed);
			Assert.AreEqual (1.0, (await editor.GetValueAsync<double> (subProperty)).Value);
		}
	}
}
