using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class IntegerPropertyViewModel
		: ConstrainedPropertyViewModel<long>
	{
		public IntegerPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
		}

		protected override long IncrementValue (long value)
		{
			return value + 1;
		}

		protected override long DecrementValue (long value)
		{
			return value - 1;
		}
	}

	internal class FloatingPropertyViewModel
		: ConstrainedPropertyViewModel<double>
	{
		public FloatingPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
		}

		protected override double IncrementValue (double value)
		{
			return value + 1;
		}

		protected override double DecrementValue (double value)
		{
			return value - 1;
		}
	}

	internal class BytePropertyViewModel
		: ConstrainedPropertyViewModel<byte>
	{
		public BytePropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
		}

		protected override byte IncrementValue (byte value)
		{
			return (byte)(value + 1);
		}

		protected override byte DecrementValue (byte value)
		{
			return (byte)(value - 1);
		}
	}
}