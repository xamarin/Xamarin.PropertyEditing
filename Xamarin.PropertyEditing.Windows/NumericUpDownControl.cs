using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	internal class DoubleUpDownControl
		: NumericUpDownControl<double>
	{
		// TODO decimal placement
		static DoubleUpDownControl ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof(DoubleUpDownControl), new FrameworkPropertyMetadata (typeof(DoubleUpDownControl)));
		}

		protected override bool TryParse (string text, out double value)
		{
			if (text == nameof(Double.NaN)) {
				value = Double.NaN;
				return true;
			} else if (text == "∞" || text == nameof (Double.PositiveInfinity)) {
				value = Double.PositiveInfinity;
				return true;
			} else if (text == "-∞" || text == nameof (Double.NegativeInfinity)) {
				value = Double.NegativeInfinity;
				return true;
			}

			return Double.TryParse (text, out value);
		}
	}

	internal class IntegerUpDownControl
		: NumericUpDownControl<long>
	{
		static IntegerUpDownControl ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof(IntegerUpDownControl), new FrameworkPropertyMetadata (typeof(IntegerUpDownControl)));
		}

		protected override bool TryParse (string text, out long value)
		{
			return Int64.TryParse (text, out value);
		}
	}

	internal class ByteUpDownControl
		: NumericUpDownControl<byte>
	{
		static ByteUpDownControl ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof (ByteUpDownControl), new FrameworkPropertyMetadata (typeof (ByteUpDownControl)));
		}

		protected override bool TryParse (string text, out byte value)
		{
			return byte.TryParse (text, out value);
		}
	}

	internal abstract class NumericUpDownControl<T>
		: TextBoxEx
		where T : struct, IComparable<T>
	{
		public NumericUpDownControl ()
		{
			SetCurrentValue (TextProperty, Value.ToString());
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register (
			"Value", typeof(T?), typeof(NumericUpDownControl<T>), new FrameworkPropertyMetadata (default(T?), (d,e) => ((NumericUpDownControl<T>)d).OnValueChanged (e)) { BindsTwoWayByDefault = true });

		public T? Value
		{
			get { return (T?) GetValue (ValueProperty); }
			set { SetValue (ValueProperty, value); }
		}

		protected abstract bool TryParse (string text, out T value);

		protected override void OnPreviewKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Down) {
				SetCurrentValue (ValueProperty, Numeric<T?>.Decrement (Value));
				SelectAll();
				e.Handled = true;
			} else if (e.Key == Key.Up) {
				SetCurrentValue (ValueProperty, Numeric<T?>.Increment (Value));
				SelectAll();
				e.Handled = true;
			}

			base.OnPreviewKeyDown (e);
		}

		protected override void OnSubmit ()
		{
			base.OnSubmit ();

			T value;
			if (TryParse (Text, out value)) {
				SetCurrentValue (ValueProperty, value);
			} else {
				SetCurrentValue (TextProperty, Value.ToString());
			}
		}

		private void OnValueChanged (DependencyPropertyChangedEventArgs e)
		{
			SetCurrentValue (TextProperty, Value.ToString());
		}
	}
}
