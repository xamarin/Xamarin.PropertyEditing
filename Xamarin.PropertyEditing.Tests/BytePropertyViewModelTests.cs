using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Tests
{
	[TestFixture]
	internal class BytePropertyViewModelTests
		: ConstrainedPropertyViewModelTests<byte, BytePropertyViewModel>
	{
		protected override Tuple<byte, byte> MaxMin => new Tuple<byte, byte> (byte.MaxValue, byte.MinValue);

		protected override byte GetRandomTestValue (Random rand)
		{
			return rand.NextByte ();
		}

		protected override byte GetConstrainedRandomValue (Random rand, out byte max, out byte min)
		{
			var value = (byte)rand.Next (2, byte.MaxValue - 2);
			max = (byte)rand.Next (value + 1, byte.MaxValue);
			min = (byte)rand.Next (0, value - 1);
			return value;
		}

		protected override byte GetConstrainedRandomValueAboveBounds (Random rand, out byte max, out byte min)
		{
			var value = (byte)rand.Next (2, byte.MaxValue - 2);
			min = (byte)rand.Next (0, value - 1);
			max = (byte)rand.Next ((byte)(min + 1), value - 1);

			return value;
		}

		protected override byte GetConstrainedRandomValueBelowBounds (Random rand, out byte max, out byte min)
		{
			var value = (byte)rand.Next (2, byte.MaxValue - 2);
			max = (byte)rand.Next (value + 1, byte.MaxValue);
			min = (byte)rand.Next (value + 1, (byte)(max - 1));

			return value;
		}

		protected override BytePropertyViewModel GetViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			return new BytePropertyViewModel (platform, property, editors);
		}
	}
}
