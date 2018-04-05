using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class ColorPopoverViewController : NSViewController
	{
		SolidColorBrushEditor brushEditor;

		public ColorPopoverViewController () : base ()
		{
			PreferredContentSize = new CGSize (300, 300);
		}

		SolidBrushViewModel viewModel;
		internal SolidBrushViewModel ViewModel
		{
			get => viewModel;
			set
			{
				viewModel = value;
				if (brushEditor != null)
					brushEditor.ViewModel = value;
			}
		}

		public override void LoadView ()
		{
			View = brushEditor = new SolidColorBrushEditor ();
			brushEditor.ViewModel = ViewModel;
		}
	}
}
