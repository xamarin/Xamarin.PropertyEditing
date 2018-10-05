using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CommonBrushView
		: NSView, IValueView
	{
		public CommonBrush Brush {
			get => (Layer as CommonBrushLayer)?.Brush;
			set => (Layer as CommonBrushLayer).Brush = value;
		}

		NSView IValueView.NativeView => this;

		public CommonBrushView ()
		{
			Initialize ();
		}

		public CommonBrushView (CGRect frame) : base (frame)
		{
			Initialize ();
		}

		public CommonBrushView (IntPtr handle) : base (handle)
		{
		}

		void IValueView.SetValue (object value)
		{
			var brush = value as CommonBrush;
			if (value != null && brush == null)
				throw new ArgumentException (nameof (value));

			Brush = brush;
		}

		private void Initialize () {
			WantsLayer = true;
			Layer = new CommonBrushLayer
			{
				Brush = Brush
			};
		}
    }
}
