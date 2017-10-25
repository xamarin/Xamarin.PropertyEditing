using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal abstract class CurrentColorCommitterControlBase : Control
	{
		public static readonly RoutedEvent CommitCurrentColorEvent =
			EventManager.RegisterRoutedEvent (
				nameof(CommitCurrentColor), RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (CurrentColorCommitterControlBase));

		public event RoutedEventHandler CommitCurrentColor {
			add { AddHandler (CommitCurrentColorEvent, value); }
			remove { RemoveHandler (CommitCurrentColorEvent, value); }
		}

		public static readonly RoutedEvent CommitShadeEvent =
			EventManager.RegisterRoutedEvent (
				nameof(CommitShade), RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (CurrentColorCommitterControlBase));

		public event RoutedEventHandler CommitShade {
			add { AddHandler (CommitShadeEvent, value); }
			remove { RemoveHandler (CommitShadeEvent, value); }
		}
	}

	internal abstract class ColorEditorControlBase : CurrentColorCommitterControlBase
	{
		public static readonly DependencyProperty ColorProperty =
			DependencyProperty.Register (
				"Color", typeof (CommonColor), typeof (ColorEditorControlBase),
				new PropertyMetadata (new CommonColor (0, 0, 0), OnColorChanged));

		public CommonColor Color {
			get => (CommonColor)GetValue (ColorProperty);
			set => SetValue (ColorProperty, value);
		}

		static void OnColorChanged (DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			var control = (ColorEditorControlBase)source;
			control.OnColorChanged ((CommonColor)e.OldValue, (CommonColor)e.NewValue);
		}

		protected virtual void OnColorChanged (CommonColor oldColor, CommonColor newColor) { }
	}
}
