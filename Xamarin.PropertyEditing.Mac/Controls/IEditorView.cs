using System;
using AppKit;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal interface IEditorView
		: INativeContainer
	{
		EditorViewModel ViewModel { get; set; }

		bool NeedsPropertyButton { get; }
		bool IsDynamicallySized { get; }
		nint GetHeight (EditorViewModel vm);
	}

	internal interface IValueView
		: INativeContainer
	{
		void SetValue (object value);
	}

	internal interface INativeContainer
	{
		/// <summary>
		/// Gets the native view.
		/// </summary>
		/// <value>Generally just `this`.</value>
		NSView NativeView { get; }
	}
}
