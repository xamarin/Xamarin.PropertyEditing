using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Xamarin.PropertyEditing.Windows
{
	internal class WindowEx
		: Window
	{
		public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register ("ShowIcon", typeof(bool), typeof(WindowEx), new PropertyMetadata ((d, e) => ((WindowEx)d).UpdateExtendedStyles()));
		public bool ShowIcon
		{
			get { return (bool)GetValue (ShowIconProperty); }
			set { SetValue (ShowIconProperty, value); }
		}

		protected override void OnSourceInitialized (EventArgs e)
		{
			base.OnSourceInitialized (e);

			this.interop = new WindowInteropHelper (this);

			UpdateExtendedStyles();
		}

		private WindowInteropHelper interop;

		private void UpdateExtendedStyles()
		{
			if (this.interop == null)
				return;

			int currentStyle = GetWindowLong (this.interop.Handle, WindowIndex.ExtendedStyle);
			bool showingIcon = (currentStyle & WS_EX_DLGMODALFRAME) == 0;
			if (ShowIcon == showingIcon)
				return;

			if (ShowIcon && !showingIcon)
				currentStyle |= WS_EX_DLGMODALFRAME;
			else
				currentStyle ^= WS_EX_DLGMODALFRAME;

			SetWindowLong (this.interop.Handle, WindowIndex.ExtendedStyle, currentStyle | WS_EX_DLGMODALFRAME);
			SetWindowPos (this.interop.Handle, IntPtr.Zero, 0, 0, 0, 0, PositionFlags.NoMove | PositionFlags.NoSize | PositionFlags.NoZOrder | PositionFlags.FrameChanged);
		}

		private enum WindowIndex
		{
			ExtendedStyle = -20
		}

		const int WS_EX_DLGMODALFRAME = 0x001;

		[Flags]
		private enum PositionFlags
		{
			NoSize = 0x0001,
			NoMove = 0x0002,
			NoZOrder = 0x0004,
			FrameChanged = 0x0020,
		}

		[DllImport ("user32.dll")]
		private static extern bool SetWindowPos (IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int theight, PositionFlags flags);

		[DllImport ("user32.dll")]
		private static extern int GetWindowLong (IntPtr hwnd, WindowIndex index);

		[DllImport ("user32.dll")]
		private static extern int SetWindowLong (IntPtr hWnd, WindowIndex index, int dwNewLong);
	}
}
