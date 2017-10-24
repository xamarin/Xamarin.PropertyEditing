using System;
using System.Windows;
using System.Windows.Controls;

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

		protected override double GetIncrementedValue (double value)
		{
			return value + 1;
		}

		protected override double GetDecrementedValue (double value)
		{
			return value - 1;
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

		protected override long GetIncrementedValue (long value)
		{
			return value + 1;
		}

		protected override long GetDecrementedValue (long value)
		{
			return value - 1;
		}
	}

	[TemplatePart (Name = "TextBox", Type = typeof(TextBox))]
	[TemplatePart (Name = "Up", Type = typeof(Button))]
	[TemplatePart (Name = "Down", Type = typeof(Button))]
	internal abstract class NumericUpDownControl<T>
		: Control
		where T : struct, IComparable<T>
	{
		static NumericUpDownControl ()
		{
			FocusableProperty.OverrideMetadata (typeof (NumericUpDownControl<T>), new FrameworkPropertyMetadata (false));
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

		public static readonly DependencyProperty ShowSpinnerProperty = DependencyProperty.Register (
			"ShowSpinner", typeof(bool), typeof(NumericUpDownControl<T>), new PropertyMetadata (default(bool)));

		public bool ShowSpinner
		{
			get { return (bool) GetValue (ShowSpinnerProperty); }
			set { SetValue (ShowSpinnerProperty, value); }
		}

		public static readonly DependencyProperty IsConstrainedProperty = DependencyProperty.Register (
			"IsConstrained", typeof(bool), typeof(NumericUpDownControl<T>), new PropertyMetadata (default(bool)));

		public bool IsConstrained
		{
			get { return (bool) GetValue (IsConstrainedProperty); }
			set { SetValue (IsConstrainedProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.textBox = (TextBox) GetTemplateChild ("TextBox");
			this.textBox.Text = Value.ToString ();
			this.textBox.TextChanged += OnTextChanged;

			Button up = (Button) GetTemplateChild ("Up");
			up.Click += (sender, args) => SetCurrentValue (ValueProperty, GetIncrementedValue (Value));

			Button down = (Button) GetTemplateChild ("Down");
			down.Click += (sender, args) => SetCurrentValue (ValueProperty, GetDecrementedValue (Value));
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

			if (this.textBox != null)
				this.textBox.Text = v.ToString ();

			return v;
		}

		protected abstract T GetIncrementedValue (T value);
		protected abstract T GetDecrementedValue (T value);
		protected abstract bool TryParse (string text, out T value);

		private TextBox textBox;

		private void OnTextChanged (object sender, TextChangedEventArgs e)
		{
			T value;
			if (TryParse (this.textBox.Text, out value)) {
				SetCurrentValue (ValueProperty, value);
			} else {
				// TODO ignore and reset value
			}
		}

		private void OnValueChanged (DependencyPropertyChangedEventArgs e)
		{
			CoerceValue (MaximumValueProperty);
			CoerceValue (MinimumValueProperty);
		}
	}
}
