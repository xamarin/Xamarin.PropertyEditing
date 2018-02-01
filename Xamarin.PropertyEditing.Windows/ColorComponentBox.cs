using System;
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
				this.innerTextBox.SelectAll ();
				this.previousValue = Value;
			};
			this.innerTextBox.PreviewMouseLeftButtonDown += (s, e) => {
				this.innerTextBox.Focus ();
				e.Handled = true;
			};
			this.innerTextBox.LostFocus += (s, e) => {
				if (Value != this.previousValue) {
					RaiseEvent (new RoutedEventArgs (ValueChangedEvent));
				}
			};
			this.innerTextBox.PreviewKeyDown += (s, e) => {
				if (e.Key == Key.Return) {
					RaiseEvent (new RoutedEventArgs (ValueChangedEvent));
				}
			};
		}

		private TextBoxEx innerTextBox;
		private double previousValue;
	}
}
