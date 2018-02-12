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
			MaximumValueProperty.OverrideMetadata (typeof (DoubleUpDownControl), new PropertyMetadata (Double.MaxValue));
			MinimumValueProperty.OverrideMetadata (typeof (DoubleUpDownControl), new PropertyMetadata (Double.MinValue));
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
			MaximumValueProperty.OverrideMetadata (typeof(IntegerUpDownControl), new PropertyMetadata (Int64.MaxValue));
			MinimumValueProperty.OverrideMetadata (typeof(IntegerUpDownControl), new PropertyMetadata (Int64.MinValue));
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
			MaximumValueProperty.OverrideMetadata (typeof (ByteUpDownControl), new PropertyMetadata (byte.MaxValue));
			MinimumValueProperty.OverrideMetadata (typeof (ByteUpDownControl), new PropertyMetadata (byte.MinValue));
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
			"Value", typeof(T), typeof(NumericUpDownControl<T>), new FrameworkPropertyMetadata (default(T), (d,e) => ((NumericUpDownControl<T>)d).OnValueChanged (e), (d,e) => ((NumericUpDownControl<T>)d).OnCoerceValue(e)) { BindsTwoWayByDefault = true });

		public T Value
		{
			get { return (T) GetValue (ValueProperty); }
			set { SetValue (ValueProperty, value); }
		}

		public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register (
			"MinimumValue", typeof(T), typeof(NumericUpDownControl<T>), new PropertyMetadata (default(T)));

		public T MinimumValue
		{
			get { return (T) GetValue (MinimumValueProperty); }
			set { SetValue (MinimumValueProperty, value); }
		}

		public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register (
			"MaximumValue", typeof(T), typeof(NumericUpDownControl<T>), new PropertyMetadata (default(T)));

		public T MaximumValue
		{
			get { return (T) GetValue (MaximumValueProperty); }
			set { SetValue (MaximumValueProperty, value); }
		}

		public static readonly DependencyProperty IsConstrainedProperty = DependencyProperty.Register (
			"IsConstrained", typeof(bool), typeof(NumericUpDownControl<T>), new PropertyMetadata (default(bool)));

		public bool IsConstrained
		{
			get { return (bool) GetValue (IsConstrainedProperty); }
			set { SetValue (IsConstrainedProperty, value); }
		}

		protected virtual object OnCoerceValue (object value)
		{
			if (!IsConstrained)
				return value;

			T v = (T)value;
			if (v.CompareTo (MinimumValue) < 0)
				v = MinimumValue;
			if (v.CompareTo (MaximumValue) > 0)
				v = MaximumValue;

			SetCurrentValue (TextProperty, v.ToString());

			return v;
		}

		protected T GetIncrementedValue (T value)
		{
			return Numeric<T>.Increment (value);
		}

		protected T GetDecrementedValue (T value)
		{
			return Numeric<T>.Decrement (value);
		}

		protected abstract bool TryParse (string text, out T value);

		protected override void OnPreviewKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Down) {
				SetCurrentValue (ValueProperty, GetDecrementedValue (Value));
				SelectAll();
				e.Handled = true;
			} else if (e.Key == Key.Up) {
				SetCurrentValue (ValueProperty, GetIncrementedValue (Value));
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
			CoerceValue (MaximumValueProperty);
			CoerceValue (MinimumValueProperty);
			SetCurrentValue (TextProperty, Value.ToString());
		}
	}
}
