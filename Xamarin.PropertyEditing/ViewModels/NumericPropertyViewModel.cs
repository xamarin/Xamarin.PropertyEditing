using System;
using System.Collections.Generic;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class IntegerPropertyViewModel
		: ConstrainedPropertyViewModel<long>
	{
		public IntegerPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			if (!IsConstrained) {
				MinimumValue = Int64.MinValue;
				MaximumValue = Int64.MaxValue;
			}
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
		public FloatingPropertyViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			if (!IsConstrained) {
				MaximumValue = Double.MaxValue;
				MinimumValue = Double.MinValue;
			}
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
}