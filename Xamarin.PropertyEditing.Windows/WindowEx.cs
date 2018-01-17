using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Xamarin.PropertyEditing.Windows
{
	internal class WindowEx
		: Window
	{
		public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register ("ShowIcon", typeof(bool), typeof(WindowEx), new PropertyMetadata (true, (d, e) => ((WindowEx)d).UpdateExtendedStyles()));
		public bool ShowIcon
		{
			get { return (bool)GetValue (ShowIconProperty); }
			set { SetValue (ShowIconProperty, value); }
		}

		public static readonly DependencyProperty ShowMaximizeProperty = DependencyProperty.Register ("ShowMaximize", typeof(bool), typeof(WindowEx), new PropertyMetadata (true, (d, e) => ((WindowEx)d).UpdateExtendedStyles()));
		public bool ShowMaximize
		{
			get { return (bool)GetValue (ShowMaximizeProperty); }
			set { SetValue (ShowMaximizeProperty, value); }
		}

		public static readonly DependencyProperty ShowMinimizeProperty = DependencyProperty.Register ("ShowMinimize", typeof(bool), typeof(WindowEx), new PropertyMetadata (true, (d, e) => ((WindowEx)d).UpdateExtendedStyles()));
		public bool ShowMinimize
		{
			get { return (bool)GetValue (ShowMinimizeProperty); }
			set { SetValue (ShowMinimizeProperty, value); }
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

			bool updateExStyle = false;
			int currentExStyle = GetWindowLong (this.interop.Handle, WindowIndex.ExtendedStyle);
			bool showingIcon = (currentExStyle & WS_EX_DLGMODALFRAME) == 0;
			if (showingIcon != ShowIcon) {
				if (ShowIcon && !showingIcon)
					currentExStyle |= WS_EX_DLGMODALFRAME;
				else
					currentExStyle ^= WS_EX_DLGMODALFRAME;

				SetWindowLong (this.interop.Handle, WindowIndex.ExtendedStyle, currentExStyle);
				updateExStyle = true;
			}

			int currentStyle = GetWindowLong (this.interop.Handle, WindowIndex.Style);
			bool showingMinimize = (currentStyle & WS_MINIMIZEBOX) == WS_MINIMIZEBOX;
			bool showingMaximize = (currentStyle & WS_MAXIMIZEBOX) == WS_MAXIMIZEBOX;

			bool updateStyle = (showingMaximize != ShowMaximize || showingMinimize != ShowMinimize);
			if (showingMaximize != ShowMaximize) {
				if (ShowMaximize && !showingMaximize)
					currentStyle |= WS_MAXIMIZEBOX;
				else
					currentStyle ^= WS_MAXIMIZEBOX;
			}

			if (showingMinimize != ShowMinimize) {
				if (ShowMinimize && !showingMinimize)
					currentStyle |= WS_MINIMIZEBOX;
				else
					currentStyle ^= WS_MINIMIZEBOX;
			}

			if (updateStyle)
				SetWindowLong (this.interop.Handle, WindowIndex.Style, currentStyle);
			
			if (updateExStyle || updateStyle)
				SetWindowPos (this.interop.Handle, IntPtr.Zero, 0, 0, 0, 0, PositionFlags.NoMove | PositionFlags.NoSize | PositionFlags.NoZOrder | PositionFlags.FrameChanged);
		}

		private enum WindowIndex
		{
			Style = -16,
			ExtendedStyle = -20
		}

		const int WS_EX_DLGMODALFRAME = 0x001;
		const int WS_MAXIMIZEBOX = 0x10000;
		const int WS_MINIMIZEBOX = 0x20000;

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
