using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	abstract class ColorEditorView : NSView, INotifyingListner<SolidBrushViewModel>
	{
		protected const float padding = 3;
		protected NotifyingViewAdaptor<SolidBrushViewModel> adaptor { get; }

		public ColorEditorView (IntPtr handle) : base (handle)
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		[Export ("initWithCoder:")]
		public ColorEditorView (NSCoder coder) : base (coder)
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		public ColorEditorView (CGRect frame) : base (frame)
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		public ColorEditorView () : base ()
		{
			adaptor = new NotifyingViewAdaptor<SolidBrushViewModel> (this);
		}

		public SolidBrushViewModel ViewModel
		{
			get => adaptor.ViewModel;
			set => adaptor.ViewModel = value;
		}

		protected virtual void OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (nameof (SolidBrushViewModel.Color)));
		}

		protected virtual void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			//base.MouseDragged (theEvent);
			UpdateFromEvent (theEvent);
		}

		public override void MouseDown (NSEvent theEvent)
		{
			//base.MouseDown (theEvent);
			UpdateFromEvent (theEvent);
		}

		public virtual void UpdateFromEvent (NSEvent theEvent)
		{
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			if (!disposing)
				return;

			adaptor.Disconnect ();
		}

		void INotifyingListner<SolidBrushViewModel>.OnViewModelChanged (SolidBrushViewModel oldModel)
		{
			OnViewModelChanged (oldModel);
		}

		void INotifyingListner<SolidBrushViewModel>.OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged (sender, e);
		}
	}
}
