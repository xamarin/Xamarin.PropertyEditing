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

		NSView INativeContainer.NativeView => this;

		public CommonBrushView (IHostResourceProvider hostResources)
		{
			Initialize (hostResources);
		}

		public CommonBrushView (IHostResourceProvider hostResources, CGRect frame)
			: base (frame)
		{
			Initialize (hostResources);
		}

		void IValueView.SetValue (object value)
		{
			var brush = value as CommonBrush;
			if (value != null && brush == null)
				throw new ArgumentException (nameof (value));

			Brush = brush;
		}

		private void Initialize (IHostResourceProvider hostResources)
		{
			WantsLayer = true;
			Layer = new CommonBrushLayer (hostResources) {
				Brush = Brush
			};
		}
    }
}
