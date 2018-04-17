using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class ColorEventArgs
		: EventArgs
	{
		public ColorEventArgs (CommonColor color)
		{
			Color = color;
		}

		public CommonColor Color
		{
			get;
		}
	}

	internal class ColorComittedEventArgs
		: ColorEventArgs
	{
		public ColorComittedEventArgs (CommonColor color)
			: base (color)
		{
		}

		public ColorComittedEventArgs (Exception exception)
			: base (default(CommonColor))
		{
			Exception = exception;
		}

		public Exception Exception
		{
			get;
		}
	}

	internal class Eyedropper
		: IDisposable
	{
		public Eyedropper ()
		{
			this.dc = GetDC (IntPtr.Zero);
			this.updaterThread = new Thread (Updater);
			this.updaterThread.Start ();
		}

		public event EventHandler<ColorEventArgs> ColorChanged;
		public event EventHandler<ColorComittedEventArgs> ColorComitted;

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~Eyedropper ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!this.running)
				return;

			this.running = false;
			this.updaterThread.Join ();
			ReleaseDC (IntPtr.Zero, this.dc);
		}

		private readonly IntPtr dc;
		private volatile bool running = true;
		private readonly Thread updaterThread;
		private readonly SynchronizationContext context = SynchronizationContext.Current;

		private void Updater ()
		{
			// AFAICT it's this or global hooks. Global hooks are bad.
			int pixel = 0;
			while (this.running) {
				if (!this.running)
					return;

				if (!GetCursorPos (out POINT point)) {
					Finish (new ColorComittedEventArgs (new Win32Exception ()));
					return;
				}

				short keyState = GetKeyState (VK_LBUTTON);
				int newPixel = GetPixel (this.dc, point.x, point.y);
				if (keyState > 0) {
					Finish (new ColorComittedEventArgs (GetColor (newPixel)));
					return;
				}

				if (pixel != newPixel)
					Update (new ColorEventArgs (GetColor (newPixel)));

				pixel = newPixel;
				Thread.Sleep (0);
			}
		}

		private CommonColor GetColor (int pixel)
		{
			return new CommonColor ((byte)(pixel & 0x000000ff), (byte)((pixel & 0x0000ff00) >> 0x8), (byte)((pixel & 0x00ff0000) >> 0x10));
		}

		private void Update (ColorEventArgs args)
		{
			this.context.Post (s => { ColorChanged?.Invoke (this, (ColorEventArgs) s); }, args);
		}

		private void Finish (ColorComittedEventArgs args)
		{
			this.context.Post (s => {
				ColorComitted?.Invoke (this, (ColorComittedEventArgs)s);
			}, args);
		}

		private struct POINT
		{
			public int x;
			public int y;
		}

		private const short VK_LBUTTON = 0x01;

		[DllImport ("Gdi32.dll")]
		private static extern int GetPixel (IntPtr hdc, int x, int y);

		[DllImport ("User32.dll")]
		private static extern short GetKeyState (short vkey);

		[DllImport ("User32.dll")]
		private static extern bool GetCursorPos (out POINT point);

		[DllImport ("User32.dll")]
		private static extern IntPtr GetDC (IntPtr hwnd);

		[DllImport ("User32.dll")]
		private static extern int ReleaseDC (IntPtr hwnd, IntPtr dc);
	}
}
