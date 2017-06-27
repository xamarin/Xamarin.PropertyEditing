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