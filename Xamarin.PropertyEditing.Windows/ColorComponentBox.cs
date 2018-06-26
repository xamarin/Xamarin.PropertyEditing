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
				new PropertyMetadata (0d));

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

			this.innerTextBox.GotKeyboardFocus += (s, e) => {
				this.previousText = this.innerTextBox.Text;
			};
			this.innerTextBox.LostKeyboardFocus += (s, e) => {
				UpdateValueIfChanged ();
			};
			this.innerTextBox.PreviewKeyUp += (s, e) => {
				this.latestText = this.innerTextBox.Text;
				if (e.Key == Key.Return) {
					UpdateValueIfChanged ();
				}
			};
		}

		private TextBoxEx innerTextBox;
		private string previousText;
		private string latestText;

		private void UpdateValueIfChanged()
		{
			if (this.latestText != this.previousText) {
				if (double.TryParse (this.latestText, NumberStyles.Float, CultureInfo.CurrentUICulture, out var value)) {
					Value = value;
					RaiseEvent (new RoutedEventArgs (ValueChangedEvent));
				}
			}
		}
	}
}
