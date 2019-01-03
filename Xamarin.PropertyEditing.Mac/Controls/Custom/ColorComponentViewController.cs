using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ColorComponentViewController
		: NotifyingViewController<SolidBrushViewModel>
	{
		public ColorComponentViewController (IHostResourceProvider hostResources, ChannelEditorType type)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;
			PreferredContentSize = new CGSize (100, 400);
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
			View = this.editor = new ColorComponentEditor (this.hostResources, EditorType);
		}

		private readonly IHostResourceProvider hostResources;
		private ColorComponentEditor editor;
	}
}
