using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "up", Type = typeof(ButtonBase))]
	[TemplatePart (Name = "down", Type = typeof(ButtonBase))]
	internal class Spinner
		: Control
	{
		static Spinner ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof(Spinner), new FrameworkPropertyMetadata (typeof(Spinner)));
			FocusableProperty.OverrideMetadata (typeof(Spinner), new FrameworkPropertyMetadata (false));
		}

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register (
			"Value", typeof(int), typeof(Spinner), new FrameworkPropertyMetadata (default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d,p) => ((Spinner)d).OnValueChanged ()));

		public int Value
		{
			get { return (int) GetValue (ValueProperty); }
			set { SetValue (ValueProperty, value); }
		}

		public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register (
			"MinimumValue", typeof(int), typeof(Spinner), new PropertyMetadata (default(int)));

		public int MinimumValue
		{
			get { return (int) GetValue (MinimumValueProperty); }
			set { SetValue (MinimumValueProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.up = GetTemplateChild ("up") as ButtonBase;
			this.down = GetTemplateChild ("down") as ButtonBase;
			this.display = GetTemplateChild("display") as TextBlock;

			if (this.up == null || this.down == null)
				throw new InvalidOperationException ("Spinner's template needs an up and down button");

			this.up.Click += (sender, args) => {
				args.Handled = true;
				Adjust (1);
			};
			this.down.Click += (sender, args) => {
				args.Handled = true;
				Adjust (-1);
			};

			Adjust (MinimumValue);
			OnValueChanged();
		}

		private ButtonBase up, down;
		private TextBlock display;

		private void Adjust (int d)
		{
			SetCurrentValue (ValueProperty, Value + d);
			this.down.IsEnabled = Value > MinimumValue;
		}

		private void OnValueChanged ()
		{
			if (this.display == null)
				return;

			this.display.Text = Value.ToString ();
		}
	}
}
