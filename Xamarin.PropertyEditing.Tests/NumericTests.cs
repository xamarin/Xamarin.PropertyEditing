using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class NumericTests
	{
		[Test]
		public void SByte ()
		{
			sbyte v = 0;
			v = Numeric<sbyte>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<sbyte>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
			v = Numeric<sbyte>.Decrement (v);
			Assert.That (v, Is.EqualTo (-1));
		}

		[Test]
		public void Byte ()
		{
			byte v = 0;
			v = Numeric<byte>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<byte>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
		}

		[Test]
		public void Int16 ()
		{
			short v = 0;
			v = Numeric<short>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<short>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
			v = Numeric<short>.Decrement (v);
			Assert.That (v, Is.EqualTo (-1));
		}

		[Test]
		public void UIn16 ()
		{
			ushort v = 0;
			v = Numeric<ushort>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<ushort>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
		}

		[Test]
		public void Int32 ()
		{
			int v = 0;
			v = Numeric<int>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<int>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
			v = Numeric<int>.Decrement (v);
			Assert.That (v, Is.EqualTo (-1));
		}

		[Test]
		public void UIn32 ()
		{
			uint v = 0;
			v = Numeric<uint>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<uint>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
		}

		[Test]
		public void Int64()
		{
			long v = 0;
			v = Numeric<long>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<long>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
			v = Numeric<long>.Decrement (v);
			Assert.That (v, Is.EqualTo (-1));
		}

		[Test]
		public void UIn64 ()
		{
			ulong v = 0;
			v = Numeric<ulong>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<ulong>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
		}

		[Test]
		public void Single()
		{
			float v = 0;
			v = Numeric<float>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<float>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
			v = Numeric<float>.Decrement (v);
			Assert.That (v, Is.EqualTo (-1));
		}

		[Test]
		public void Double()
		{
			double v = 0;
			v = Numeric<double>.Increment (v);
			Assert.That (v, Is.EqualTo (1));
			v = Numeric<double>.Decrement (v);
			Assert.That (v, Is.EqualTo (0));
			v = Numeric<double>.Decrement (v);
			Assert.That (v, Is.EqualTo (-1));
		}
	}
}
