using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal sealed class GradientBrushPreview : Adorner
	{
		public GradientBrushPreview (UIElement adornedElement) : base (adornedElement)
		{
			presenter = new ContentPresenter ();
			AddVisualChild (presenter);
			// Bind the `Content` property of the presenter to the `GradientBrush` attached property of the adorned element
			var dataContextBinding = new Binding {
				Path = new PropertyPath(GradientBrushProperty),
				Source = adornedElement
			};
			// PresentationTraceSources.SetTraceLevel (dataContextBinding, PresentationTraceLevel.High);
			BindingOperations.SetBinding (presenter, ContentPresenter.ContentProperty, dataContextBinding);
		}

		ContentPresenter presenter;

		static readonly DependencyProperty AdornerProperty =
			DependencyProperty.RegisterAttached ("Adorner", typeof (GradientBrushPreview), typeof (GradientBrushPreview),
				new PropertyMetadata (null));

		static GradientBrushPreview GetAdorner (DependencyObject obj)
			=> (GradientBrushPreview)obj.GetValue (AdornerProperty);
		static void SetAdorner (DependencyObject obj, GradientBrushPreview value)
			=> obj.SetValue (AdornerProperty, value);

		public static readonly DependencyProperty GradientBrushProperty =
			DependencyProperty.RegisterAttached (
				"GradientBrush", typeof (Brush), typeof (GradientBrushPreview),
				new PropertyMetadata (null, OnBrushChanged));

		public static Brush GetGradientBrush (DependencyObject obj)
			=> (Brush)obj.GetValue (GradientBrushProperty);
		public static void SetGradientBrush (DependencyObject obj, object value)
			=> obj.SetValue (GradientBrushProperty, value);

		private static void OnBrushChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var adornedElement = d as FrameworkElement;
			if (adornedElement == null) throw new InvalidOperationException ("Adorners can only be applied to FrameworkElement");
			var adornerLayer = AdornerLayer.GetAdornerLayer (adornedElement);
			if (adornerLayer == null) return;

			GradientBrushPreview adorner = GetAdorner (adornedElement);

			var brush = (Brush)e.NewValue;

			if (brush != null && adorner == null) {
				adorner = new GradientBrushPreview (adornedElement);

				SetAdorner (adornedElement, adorner);
				adornerLayer.Add (adorner);
			}
			else if (brush is null && adorner != null) {
				adornerLayer.Remove (adorner);
				SetAdorner (adornedElement, null);
			}
		}

		public static readonly DependencyProperty TemplateProperty =
			DependencyProperty.RegisterAttached (nameof (Template), typeof (DataTemplate), typeof (GradientBrushPreview),
				new PropertyMetadata (null, OnTemplateChanged));

		public static DataTemplate GetTemplate (DependencyObject obj) => (DataTemplate)obj.GetValue (TemplateProperty);
		public static void SetTemplate (DependencyObject obj, DataTemplate value) => obj.SetValue (TemplateProperty, value);

		public DataTemplate Template {
			get => presenter.ContentTemplate;
			set => presenter.ContentTemplate = value;
		}

		private static void OnTemplateChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is GradientBrushPreview adorner) adorner.Template = (DataTemplate)e.NewValue;
		}

		public static readonly DependencyProperty TemplateSelectorProperty =
			DependencyProperty.RegisterAttached (nameof (TemplateSelector), typeof (DataTemplateSelector), typeof (GradientBrushPreview),
				new PropertyMetadata (null, OnTemplateSelectorChanged));

		public static DataTemplateSelector GetTemplateSelector (DependencyObject obj) => (DataTemplateSelector)obj.GetValue (TemplateSelectorProperty);
		public static void SetTemplateSelector (DependencyObject obj, DataTemplateSelector value) => obj.SetValue (TemplateSelectorProperty, value);

		public DataTemplateSelector TemplateSelector {
			get => presenter.ContentTemplateSelector;
			set => presenter.ContentTemplateSelector = value;
		}

		private static void OnTemplateSelectorChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is GradientBrushPreview adorner) adorner.TemplateSelector = (DataTemplateSelector)e.NewValue;
		}

		protected override int VisualChildrenCount => 1;
		protected override Visual GetVisualChild (int index)
		{
			if (index != 0) throw new ArgumentOutOfRangeException (nameof (index));
			return presenter;
		}

		protected override Size MeasureOverride (Size constraint)
		{
			presenter.Measure (constraint);
			return presenter.DesiredSize;
		}

		protected override Size ArrangeOverride (Size finalSize)
		{
			presenter.Arrange (new Rect (new Point (0, 0), finalSize));
			return finalSize;
		}
	}
}
