using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.PropertyEditing.Reflection;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	public class ReflectionPropertyProviderTests
	{
		[Test]
		public async Task EditorHasSimpleProperty ()
		{
			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (new TestClass ());
			Assume.That (editor, Is.Not.Null);

			Assert.That (editor.Properties.Count, Is.EqualTo (1));

			var property = editor.Properties.Single();
			Assert.That (property.Name, Is.EqualTo (nameof (TestClass.Property)));
			Assert.That (property.Type, Is.EqualTo (typeof (string)));
		}

		private class TestClass
		{
			public string Property
			{
				get;
				set;
			}
		}
	}
}
