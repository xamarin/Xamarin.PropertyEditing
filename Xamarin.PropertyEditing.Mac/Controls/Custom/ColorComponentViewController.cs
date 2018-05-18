using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	class ColorComponentViewController : NotifyingViewController<SolidBrushViewModel>
	{
		ColorComponentEditor editor;

		public ColorComponentViewController (ChannelEditorType type) : base ()
		{
			PreferredContentSize = new CGSize (200, 220);
			this.EditorType = type;
		}

		public ChannelEditorType EditorType { get; }

		public override void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			editor.ViewModel = ViewModel;
		}

		public override void ViewWillDisappear ()
		{
			base.ViewWillDisappear ();
			editor.ViewModel = null;
		}

		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();
			editor.ViewModel = ViewModel;
		}

		public override void LoadView ()
		{
			View = editor = new ColorComponentEditor (this.EditorType);
		}
	}
}
