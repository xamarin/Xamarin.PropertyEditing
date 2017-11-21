using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	/*
	[TestFixture]
	public class MockPropertyProviderTests
	{
		[Test]
		public async Task MockEditorHasSimpleProperty ()
		{
			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (new TestClass ());
			Assume.That (editor, Is.Not.Null);

			Assert.That (editor.Properties.Count, Is.EqualTo (1));

			var propertyInfo = editor.Properties.Single();
			Assert.That (propertyInfo.Name, Is.EqualTo (TestClass.PropertyName));
			Assert.That (propertyInfo.Type, Is.EqualTo (typeof (string)));
		}
		
		[Test]
		public async Task MockSetValueConvert ()
		{
			var obj = new TestClass ();

			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			const string value = "1";

			var propertyInfo = editor.Properties.Single ();
			await editor.SetValueAsync (propertyInfo, new ValueInfo<int> {
				Value = 1
			});

			Assert.That (obj.GetValue<string> (propertyInfo), Is.EqualTo (value));
		}

		[Test]
		public async Task MockGetValueConvert ()
		{
			const string value = "1";
			var obj = new TestClass(value);

			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			ValueInfo<int> info = await editor.GetValueAsync<int> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (1));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		[Test]
		public async Task MockPropertyChanged ()
		{
			var obj = new TestClass ();

			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);

			const string value = "value";
			var propertyInfo = editor.Properties.Single ();

			var changed = false;
			editor.PropertyChanged += (sender, args) => {
				if (Equals (args.Property, propertyInfo))
					changed = true;
			};

			await editor.SetValueAsync (propertyInfo, new ValueInfo<string> {
				Value = value
			});

			Assert.That (changed, Is.True, "PropertyChanged was not raised for the given property");
		}

		[Test]
		public async Task MockTypeConverterTo ()
		{
			const string value = "value";
			var obj = new ConversionClass ();
			obj.SetValue (ConversionClass.PropertyName, new TestClass (value));

			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			ValueInfo<string> info = await editor.GetValueAsync<string> (editor.Properties.Single ());
			Assert.That (info.Value, Is.EqualTo (value));
			Assert.That (info.Source, Is.EqualTo (ValueSource.Local));
		}

		[Test]
		public async Task MockTypeConvertFrom ()
		{
			const string value = "value";
			var obj = new ConversionClass ();

			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (obj);
			Assume.That (editor.Properties.Count, Is.EqualTo (1));

			await editor.SetValueAsync (editor.Properties.Single (), new ValueInfo<string> {
				Value = value,
				Source = ValueSource.Local
			});

			var val = obj.GetValue<TestClass> (ConversionClass.PropertyName);
			Assert.That (val, Is.Not.Null);
			Assert.That (val.GetValue<string>(TestClass.PropertyName), Is.EqualTo (value));
		}

		[Test]
		public async Task MockCombinableEnum ()
		{
			var enumObj = new EnumClass ();

			var provider = new MockEditorProvider ();
			IObjectEditor editor = await provider.GetObjectEditorAsync (enumObj);

			var values = new List<int> {
				(int)FlagsTestEnum.Flag1,
				(int)FlagsTestEnum.Flag3
			};

			FlagsTestEnum expected = FlagsTestEnum.Flag1 | FlagsTestEnum.Flag3;

			var propertyInfo = editor.Properties.Single ();

			await editor.SetValueAsync (propertyInfo, new ValueInfo<IReadOnlyList<int>> {
				Value = values
			});

			var underlying = await editor.GetValueAsync<int> (propertyInfo);
			Assert.That ((FlagsTestEnum)underlying.Value, Is.EqualTo (expected));

			var underlyingList = await editor.GetValueAsync<IReadOnlyList<int>> (propertyInfo);
			Assert.That (underlyingList.Value.First(), Is.EqualTo ((int)FlagsTestEnum.Flag1));
			Assert.That (underlyingList.Value.Skip(1).First(), Is.EqualTo ((int)FlagsTestEnum.Flag3));
		}

		private class Converter
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

				return (value as TestClass)?.GetValue<string>(TestClass.PropertyName);
			}

			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				var converted = new TestClass ();
				converted.SetValue<string> (TestClass.PropertyName, value as string);
				return converted;
			}
		}

		private class ConversionClass : MockControl
		{
			public static readonly string PropertyName = "TestProperty";

			public ConversionClass()
			{
				AddProperty<TestClass> (PropertyName, converterTypes: new[] { typeof (Converter) });
			}
		}

		private class TestClass : MockControl
		{
			public static readonly string PropertyName = "Property";

			public TestClass()
			{
				AddProperty<string> (PropertyName, converterTypes: new[] { typeof (StringConverter) });
			}

			public TestClass(string value) : this()
			{
				SetValue (PropertyName, value);
			}
		}

		private class EnumClass : MockControl
		{
			public static readonly string PropertyName = "Value";

			public EnumClass ()
			{
				AddProperty<FlagsTestEnum> (PropertyName, flag: true);
			}
		}
	}
	*/
}
