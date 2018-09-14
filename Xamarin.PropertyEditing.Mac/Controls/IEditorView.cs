using System;
using AppKit;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface IEditorView
	{
		/// <summary>
		/// Gets the native view.
		/// </summary>
		/// <value>Generally just `this`.</value>
		NSView NativeView { get; }

		EditorViewModel ViewModel { get; set; }

		bool IsDynamicallySized { get; }
		nint GetHeight (EditorViewModel vm);
	}
}
