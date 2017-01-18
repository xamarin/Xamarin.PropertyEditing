using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

			editor.SetValue (editor.Properties.Single (), new ValueInfo<string> {
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

			var info = editor.GetValue<string> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (value));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		[Test]
		public async Task SetValueConvert ()
		{
			var obj = new TestClass ();

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			const string value = "1";

			editor.SetValue (editor.Properties.Single (), new ValueInfo<int> {
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

			var info = editor.GetValue<int> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (1));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
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

			editor.SetValue (property, new ValueInfo<string> {
				Value = value
			});

			Assert.That (changed, Is.True, "PropertyChanged was not raised for the given property");
		}

		[Test]
		public async Task TypeConverterTo ()
		{
			const string value = "value";
			var obj = new ConversionClass {
				Property = new TestClass {
					Property = value
				}
			};

			var provider = new ReflectionEditorProvider();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			ValueInfo<string> info = editor.GetValue<string> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (value));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		[Test]
		public async Task TypeConvertFrom ()
		{
			const string value = "value";
			var obj = new ConversionClass ();

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			editor.SetValue (editor.Properties.Single (), new ValueInfo<string> {
				Value = value,
				Source = ValueSource.Local
			});

			Assert.That (obj.Property, Is.Not.Null);
			Assert.That (obj.Property.Property, Is.EqualTo (value));
		}

		[Test]
		public async Task TypeConverterToPropertyAttribute ()
		{
			const string value = "value";
			var obj = new ConversionClass2 {
				Property = new TestClass2 {
					Property = value
				}
			};

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			ValueInfo<string> info = editor.GetValue<string> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (value));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		[Test]
		public async Task TypeConvertFromPropertyAttribute ()
		{
			const string value = "value";
			var obj = new ConversionClass2 ();

			var provider = new ReflectionEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			editor.SetValue (editor.Properties.Single (), new ValueInfo<string> {
				Value = value,
				Source = ValueSource.Local
			});

			Assert.That (obj.Property, Is.Not.Null);
			Assert.That (obj.Property.Property, Is.EqualTo (value));
		}

		private class Converter2
			: TypeConverter
		{
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				return (sourceType == typeof (string));
			}

			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				return (destinationType == typeof (string));
			}

			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType != typeof (string))
					throw new ArgumentException ();

				return (value as TestClass2)?.Property;
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				return new TestClass2 { Property = value as string };
			}
		}

		private class ConversionClass2
		{
			[TypeConverter (typeof (Converter2))]
			public TestClass2 Property
			{
				get;
				set;
			}
		}

		
		private class TestClass2
		{
			public string Property
			{
				get;
				set;
			}
		}

		private class Converter
			: TypeConverter
		{
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				return (sourceType == typeof(string));
			}

			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				return (destinationType == typeof(string));
			}

			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType != typeof(string))
					throw new ArgumentException();

				return (value as TestClass)?.Property;
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				return new TestClass { Property = value as string };
			}
		}

		private class ConversionClass
		{
			public TestClass Property
			{
				get;
				set;
			}
		}

		[TypeConverter (typeof (Converter))]
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
