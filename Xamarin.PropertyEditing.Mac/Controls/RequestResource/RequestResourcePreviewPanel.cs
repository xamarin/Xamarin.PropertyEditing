using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Mac
{
	internal class RequestResourcePreviewPanel : NSView
	{
		public RequestResourcePreviewPanel (IHostResourceProvider hostResources, CGRect frame)
			: base (frame)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			var FrameHeightHalf = (Frame.Height - 32) / 2;
			var FrameWidthHalf = (Frame.Width - 32) / 2;
			var FrameWidthThird = (Frame.Width - 32) / 3;

			this.noPreviewAvailable = new UnfocusableTextField {
				StringValue = Properties.Resources.NoPreviewAvailable,
				Frame = new CGRect (50, FrameHeightHalf, 150, 50),
			};

			AddSubview (this.noPreviewAvailable);

			this.previewView = new NSView (new CGRect (20, 0, frame.Width - 30, frame.Height)) {
				Hidden = true // Hidden until a resource is selected and a preview is available for it.
			};
			AddSubview (this.previewView);
		}

		public Resource SelectedResource
		{
			get { return this.selectedResource; }
			set
			{
				if (this.selectedResource == value)
					return;

				this.selectedResource = value;

				if (this.selectedResource != null) {
					PreviewResource ();
				} else {
					ShowNoPreviewText ();
				}
			}
		}

		private readonly IHostResourceProvider hostResources;

		private UnfocusableTextField noPreviewAvailable;
		private NSView previewView;

		private Resource selectedResource;

		private void PreviewResource()
		{
			// Let's find the next View
			var pView = GetPreviewView (this.selectedResource);

			if (pView == null) {
				ShowNoPreviewText ();
			} else {
				this.noPreviewAvailable.Hidden = true;
				this.previewView.Hidden = false;

				switch (this.selectedResource) {
				case Resource<CommonColor> colour:
					if (pView is CommonBrushView cc) {
						cc.Brush = new CommonSolidBrush (colour.Value);
					}
					break;

				case Resource<CommonGradientBrush> gradient:
					if (pView is CommonBrushView vg) {
						vg.Brush = gradient.Value;
					}
					break;

				case Resource<CommonSolidBrush> solid:
					if (pView is CommonBrushView vs) {
						vs.Brush = solid.Value;
					}
					break;
				}

				NSView[] subviews = this.previewView.Subviews;
				if (subviews.Length > 0) {
					subviews[0].RemoveFromSuperview ();
				}
				// Free up anything from the previous view
				this.previewView.AddSubview (pView);
			}
		}

		private void ShowNoPreviewText ()
		{
			this.noPreviewAvailable.Hidden = false;
			this.previewView.Hidden = true;
		}

		private NSView SetUpPreviewer (Type previewRenderType)
		{
			var view = (NSView)Activator.CreateInstance (previewRenderType, this.hostResources);
			view.Identifier = previewRenderType.Name;
			view.Frame = new CGRect (0, 0, this.previewView.Frame.Width, this.previewView.Frame.Height);

			return view;
		}

		private NSView GetPreviewView (Resource resource)
		{
			Type[] genericArgs = null;
			Type previewRenderType;
			if (!PreviewValueTypes.TryGetValue (resource.RepresentationType, out previewRenderType)) {
				if (resource.RepresentationType.IsConstructedGenericType) {
					genericArgs = resource.RepresentationType.GetGenericArguments ();
					var type = resource.RepresentationType.GetGenericTypeDefinition ();
					PreviewValueTypes.TryGetValue (type, out previewRenderType);
				}
			}
			if (previewRenderType == null)
				return null;

			if (previewRenderType.IsGenericTypeDefinition) {
				if (genericArgs == null)
					genericArgs = resource.RepresentationType.GetGenericArguments ();
				previewRenderType = previewRenderType.MakeGenericType (genericArgs);
			}

			return SetUpPreviewer (previewRenderType);
		}

		internal static readonly Dictionary<Type, Type> PreviewValueTypes = new Dictionary<Type, Type> {
			{typeof (CommonSolidBrush), typeof (CommonBrushView)},
			{typeof (CommonColor), typeof (CommonBrushView)},
		};
	}
}
