using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorComponentViewController : NotifyingViewController<SolidBrushViewModel>
	{
		private ColorComponentEditor editor;

		public ColorComponentViewController (ChannelEditorType type) : base ()
		{
			PreferredContentSize = new CGSize (200, 220);
			EditorType = type;
		}

		public ChannelEditorType EditorType { get; }

		public override void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);
			this.editor.ViewModel = ViewModel;
		}

		public override void ViewWillDisappear ()
		{
			base.ViewWillDisappear ();
			this.editor.ViewModel = null;
		}

		public override void ViewWillAppear ()
		{
			base.ViewWillAppear ();
			this.editor.ViewModel = ViewModel;
		}

		public override void LoadView ()
		{
			View = this.editor = new ColorComponentEditor (this.EditorType);
		}
	}
}
