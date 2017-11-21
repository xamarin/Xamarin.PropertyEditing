using System.Windows;
using System.Windows.Controls;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class CurrentColorEditorControl : ColorEditorControlBase
	{
		public static readonly DependencyProperty InitialColorProperty =
			DependencyProperty.Register (
				nameof(InitialColor), typeof (CommonColor), typeof (CurrentColorEditorControl),
				new PropertyMetadata (new CommonColor (0, 0, 0)));

		public CommonColor InitialColor {
			get => (CommonColor)GetValue (InitialColorProperty);
			set => SetValue (InitialColorProperty, value);
		}

		public static readonly DependencyProperty LastColorProperty =
			DependencyProperty.Register (
				nameof(LastColor), typeof (CommonColor), typeof (CurrentColorEditorControl),
				new PropertyMetadata (new CommonColor (0, 0, 0)));

		public CommonColor LastColor {
			get => (CommonColor)GetValue (LastColorProperty);
			set => SetValue (LastColorProperty, value);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			var initialColorBox = (Button)GetTemplateChild ("initialColorButton");
			if (initialColorBox != null) {
				initialColorBox.Click += (s, e) => {
					Color = InitialColor;
					RaiseEvent (new RoutedEventArgs (CommitCurrentColorEvent));
				};
			}
		}
	}
}
