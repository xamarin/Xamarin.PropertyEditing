using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorComponentBox : Control
	{
		static ColorComponentBox ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (
				typeof (ColorComponentBox),
				new FrameworkPropertyMetadata (typeof (ColorComponentBox)));
		}

		public static readonly DependencyProperty GradientBrushProperty =
			DependencyProperty.Register (
				nameof (GradientBrush), typeof (Brush), typeof (ColorComponentBox),
				new PropertyMetadata (null));

		public Brush GradientBrush {
			get => (Brush)GetValue (GradientBrushProperty);
			set => SetValue (GradientBrushProperty, value);
		}

		public static readonly DependencyProperty UnitProperty =
			DependencyProperty.Register (
				nameof (Unit), typeof (string), typeof (ColorComponentBox),
				new PropertyMetadata (""));

		public string Unit {
			get => (string)GetValue (UnitProperty);
			set => SetValue (UnitProperty, value);
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register (
				nameof (Value), typeof (double), typeof (ColorComponentBox),
				new PropertyMetadata (0d, OnValueChanged));

		private static void OnValueChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is ColorComponentBox colorBox && colorBox.innerTextBox != null) colorBox.SetTextFromValueAndUnit ();
		}

		public double Value {
			get => (double)GetValue (ValueProperty);
			set => SetValue (ValueProperty, value);
		}

		public static readonly RoutedEvent ValueChangedEvent =
			EventManager.RegisterRoutedEvent (
				nameof (ValueChanged), RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (ColorComponentBox));

		public event RoutedEventHandler ValueChanged {
			add { AddHandler (ValueChangedEvent, value); }
			remove { RemoveHandler (ValueChangedEvent, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.innerTextBox = GetTemplateChild ("innerTextBox") as TextBoxEx;

			if (this.innerTextBox == null)
				throw new InvalidOperationException ($"{nameof (ColorComponentBox)} is missing a child TextBoxEx named \"innerTextBox\"");

			SetTextFromValueAndUnit ();

			this.innerTextBox.GotKeyboardFocus += (s, e) => {
				var textValue = Value.ToString ("##0.#");
				this.previousText = this.innerTextBox.Text = textValue;
			};
			this.innerTextBox.LostKeyboardFocus += (s, e) => {
				UpdateValueIfChanged ();
			};
			this.innerTextBox.KeyUp += (s, e) => {
				if (e.Key == Key.Return) {
					UpdateValueIfChanged ();
				}
			};
		}

		private TextBoxEx innerTextBox;
		private string previousText;

		private void UpdateValueIfChanged()
		{
			if (this.innerTextBox != null && this.innerTextBox.Text != this.previousText) {
				if (double.TryParse (this.innerTextBox.Text, NumberStyles.Float, CultureInfo.CurrentUICulture, out var value)) {
					Value = value;
					RaiseEvent (new RoutedEventArgs (ValueChangedEvent));
				}
			}
			SetTextFromValueAndUnit ();
		}

		private void SetTextFromValueAndUnit()
		{
			if (this.innerTextBox != null) this.innerTextBox.Text = Value.ToString ("F0") + Unit;
		}
	}
}
