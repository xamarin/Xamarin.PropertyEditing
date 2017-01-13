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

		[Test]
		public async Task SetValue ()
		{
			var obj = new TestClass ();

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			const string value = "value";

			await editor.SetValueAsync (editor.Properties.Single (), new ValueInfo<string> {
				Value = value
			});

			Assert.That (obj.Property, Is.EqualTo (value));
		}

		[Test]
		public async Task GetValue ()
		{
			const string value = "value";
			var obj = new TestClass { Property = value };

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			var info = await editor.GetValueAsync<string> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (value));
		}

		[Test]
		public async Task SetValueConvert ()
		{
			var obj = new TestClass ();

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			const string value = "1";

			await editor.SetValueAsync (editor.Properties.Single (), new ValueInfo<int> {
				Value = 1
			});

			Assert.That (obj.Property, Is.EqualTo (value));
		}

		[Test]
		public async Task GetValueConvert ()
		{
			const string value = "1";
			var obj = new TestClass { Property = value };

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			var info = await editor.GetValueAsync<int> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (1));
		}

		[Test]
		public async Task PropertyChanged ()
		{
			var obj = new TestClass ();

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			const string value = "value";
			var property = editor.Properties.Single ();

			bool changed = false;
			editor.PropertyChanged += (sender, args) => {
				if (Equals (args.Property, property))
					changed = true;
			};

			await editor.SetValueAsync (property, new ValueInfo<string> {
				Value = value
			});

			Assert.That (changed, Is.True, "PropertyChanged was not raised for the given property");
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
