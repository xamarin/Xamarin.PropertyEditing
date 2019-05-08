using System;
using System.Runtime.InteropServices;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal static class Extensions
	{
		internal const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";

		[DllImport (LIBOBJC_DYLIB, EntryPoint = "objc_msgSend")]
		public extern static void void_objc_msgSend_intptr (IntPtr receiver, IntPtr selector, IntPtr arg1);

		const string selSetFormatter = "setFormatter:";
		static readonly IntPtr selSetFormatter_Handle = ObjCRuntime.Selector.GetHandle (selSetFormatter);

		internal static void SetFormatter (this NumericSpinEditor control, NSFormatter formatter)
		{
			IntPtr pointer = formatter != null ? formatter.Handle : IntPtr.Zero;
			void_objc_msgSend_intptr (control.NumericEditor.Handle, selSetFormatter_Handle, pointer);
		}
	}
}
