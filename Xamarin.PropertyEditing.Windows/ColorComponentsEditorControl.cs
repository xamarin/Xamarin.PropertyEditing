using System;
using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorComponentsEditorControl : ColorEditorControlBase
	{
		public ColorComponentsEditorControl ()
		{
			DefaultStyleKey = typeof (ColorComponentsEditorControl);
		}

		public static readonly DependencyProperty RedProperty =
			DependencyProperty.Register (
				nameof (R), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte R {
			get => (byte)GetValue (RedProperty);
			set => SetValue (RedProperty, value);
		}

		public static readonly DependencyProperty GreenProperty =
			DependencyProperty.Register (
				nameof (G), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte G {
			get => (byte)GetValue (GreenProperty);
			set => SetValue (GreenProperty, value);
		}

		public static readonly DependencyProperty BlueProperty =
			DependencyProperty.Register (
				nameof (B), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte B {
			get => (byte)GetValue (BlueProperty);
			set => SetValue (BlueProperty, value);
		}

		public static readonly DependencyProperty AlphaProperty =
			DependencyProperty.Register (
				nameof (A), typeof (byte), typeof (ColorComponentsEditorControl),
				new PropertyMetadata ((byte)0));

		public byte A {
			get => (byte)GetValue (AlphaProperty);
			set => SetValue (AlphaProperty, value);
		}

		public static readonly DependencyProperty CyanProperty =
			DependencyProperty.Register (
				nameof (C), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double C {
			get => (double)GetValue (CyanProperty);
			set => SetValue (CyanProperty, value);
		}

		public static readonly DependencyProperty MagentaProperty =
			DependencyProperty.Register (
				nameof (M), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double M {
			get => (double)GetValue (MagentaProperty);
			set => SetValue (MagentaProperty, value);
		}

		public static readonly DependencyProperty YellowProperty =
			DependencyProperty.Register (
				nameof (Y), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double Y {
			get => (double)GetValue (YellowProperty);
			set => SetValue (YellowProperty, value);
		}

		public static readonly DependencyProperty BlackProperty =
			DependencyProperty.Register (
				nameof (K), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double K {
			get => (double)GetValue (BlackProperty);
			set => SetValue (BlackProperty, value);
		}

		public static readonly DependencyProperty HueProperty =
			DependencyProperty.Register (
				nameof (Hue), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double Hue {
			get => (double)GetValue (HueProperty);
			set => SetValue (HueProperty, value);
		}

		public static readonly DependencyProperty SaturationProperty =
			DependencyProperty.Register (
				nameof (Saturation), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double Saturation {
			get => (double)GetValue (SaturationProperty);
			set => SetValue (SaturationProperty, value);
		}

		public static readonly DependencyProperty LightnessProperty =
			DependencyProperty.Register (
				nameof (Lightness), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double Lightness {
			get => (double)GetValue (LightnessProperty);
			set => SetValue (LightnessProperty, value);
		}

		public static readonly DependencyProperty BrightnessProperty =
			DependencyProperty.Register (
				nameof (Brightness), typeof (double), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (0d));

		public double Brightness {
			get => (double)GetValue (BrightnessProperty);
			set => SetValue (BrightnessProperty, value);
		}

		public static readonly DependencyProperty ColorComponentModelProperty =
			DependencyProperty.Register (
				nameof (ColorComponentModel), typeof (ColorComponentModel), typeof (ColorComponentsEditorControl),
				new PropertyMetadata (ColorComponentModel.RGB, OnColorModelChanged));

		public ColorComponentModel ColorComponentModel {
			get => (ColorComponentModel)GetValue (ColorComponentModelProperty);
			set => SetValue (ColorComponentModelProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.rgbPane = GetTemplateChild ("rgbPane") as UIElement;
			this.cmykPane = GetTemplateChild ("cmykPane") as UIElement;
			this.hlsPane = GetTemplateChild ("hlsPane") as UIElement;
			this.hsbPane = GetTemplateChild ("hsbPane") as UIElement;

			if (ContextMenu != null) {
				foreach (MenuItem item in ContextMenu.Items) {
					item.Click += (s, e) => {
						ColorComponentModel = (ColorComponentModel)Enum.Parse (
							typeof (ColorComponentModel),
							item.Name.Substring (0, item.Name.Length - "MenuItem".Length),
							true);
					};
					CheckIfCurrentModel (item);
				}
			}

			foreach (ColorComponentBox componentBox in this.rgbPane.GetDescendants<ColorComponentBox> ()) {
				componentBox.ValueChanged += OnRGBComponentBoxChanged;
			}
			foreach (ColorComponentBox componentBox in this.cmykPane.GetDescendants<ColorComponentBox> ()) {
				componentBox.ValueChanged += OnCMYKComponentBoxChanged;
			}
			foreach (ColorComponentBox componentBox in this.hlsPane.GetDescendants<ColorComponentBox> ()) {
				componentBox.ValueChanged += OnHLSComponentBoxChanged;
			}
			foreach (ColorComponentBox componentBox in this.hsbPane.GetDescendants<ColorComponentBox> ()) {
				componentBox.ValueChanged += OnHSBComponentBoxChanged;
			}
			foreach (Button button in this.GetDescendants<Button> ()) {
				button.Click += OnComponentLabelClick;
			}
			foreach (TextBox textbox in this.GetDescendants<TextBox> ()) {
				// This will not get TextBoxes buried inside templates of derived TextBox types, such as ColorComponentBox,
				// those will have to do their own focus management.
				if (!textbox.GetType ().IsSubclassOf (typeof (TextBox))) {
					textbox.GotKeyboardFocus += (s, e) => {
						textbox.SelectAll ();
					};
					textbox.PreviewMouseLeftButtonDown += (s, e) => {
						textbox.Focus ();
						e.Handled = true;
					};
				}
			}
		}

		protected override void OnColorChanged (CommonColor oldColor, CommonColor newColor)
		{
			base.OnColorChanged (oldColor, newColor);

			if (R != newColor.R) R = newColor.R;
			if (G != newColor.G) G = newColor.G;
			if (B != newColor.B) B = newColor.B;
			if (A != newColor.A) A = newColor.A;
			if (C != newColor.C) C = newColor.C;
			if (M != newColor.M) M = newColor.M;
			if (Y != newColor.Y) Y = newColor.Y;
			if (K != newColor.K) K = newColor.K;
			if (Hue != newColor.Hue) Hue = newColor.Hue;
			if (Saturation != newColor.Saturation) Saturation = newColor.Saturation;
			if (Lightness != newColor.Lightness) Lightness = newColor.Lightness;
			if (Brightness != newColor.Brightness) Brightness = newColor.Brightness;
		}

		private UIElement rgbPane;
		private UIElement cmykPane;
		private UIElement hlsPane;
		private UIElement hsbPane;

		private void OnRGBComponentBoxChanged (object sender, RoutedEventArgs e)
		{
			var newColor = new CommonColor (R, G, B, A);
			if (!newColor.Equals (Color)) {
				Color = newColor;
			}

			RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
		}

		private void OnCMYKComponentBoxChanged (object sender, RoutedEventArgs e)
		{
			var newColor = CommonColor.FromCMYK (C, M, Y, K, A);
			if (!newColor.Equals (Color)) {
				Color = newColor;
			}

			RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
		}

		private void OnHLSComponentBoxChanged (object sender, RoutedEventArgs e)
		{
			var newColor = CommonColor.FromHLS (Hue, Lightness, Saturation, A);
			if (!newColor.Equals (Color)) {
				Color = newColor;
			}

			RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
		}

		private void OnHSBComponentBoxChanged (object sender, RoutedEventArgs e)
		{
			var newColor = CommonColor.FromHSB (Hue, Saturation, Brightness, A);
			if (!newColor.Equals (Color)) {
				Color = newColor;
			}

			RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
		}

		private void OnComponentLabelClick (object sender, RoutedEventArgs e)
		{
			if (ContextMenu != null) {
				ContextMenu.PlacementTarget = sender as UIElement;
				ContextMenu.IsOpen = true;
				foreach (MenuItem item in ContextMenu.Items) {
					CheckIfCurrentModel (item);
				}
			}
		}

		private static void OnColorModelChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var that = d as ColorComponentsEditorControl;

			var model = (ColorComponentModel)e.NewValue;

			void UpdateVisibility (UIElement el, ColorComponentModel elModel)
			{
				if (el != null) el.Visibility = model == elModel ? Visibility.Visible : Visibility.Hidden;
			}

			UpdateVisibility (that.rgbPane, ColorComponentModel.RGB);
			UpdateVisibility (that.cmykPane, ColorComponentModel.CMYK);
			UpdateVisibility (that.hlsPane, ColorComponentModel.HLS);
			UpdateVisibility (that.hsbPane, ColorComponentModel.HSB);
		}

		private void CheckIfCurrentModel (MenuItem item)
		{
			item.IsChecked = item.Header.ToString () == Enum.GetName (typeof (ColorComponentModel), ColorComponentModel);
		}
	}
}
