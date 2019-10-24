using System;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class EventTableDelegate : BaseOutlineViewDelegate
	{
		public const string EventIdentifier = "EventContainer";

		public EventTableDelegate (IHostResourceProvider hostResources, BaseOutlineViewDataSource dataSource) : base (hostResources, dataSource)
		{
		}

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			if (item is NSObjectFacade facade) {
				if (facade.Target is EventViewModel eventViewModel) {
					var viewContainer = (NSView)outlineView.MakeView (EventIdentifier, this);
					if (viewContainer == null) {
						viewContainer = new NSView {
							Identifier = EventIdentifier,
						};

						var title = new UnfocusableTextField {
							Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultDescriptionLabelFontSize),
							TranslatesAutoresizingMaskIntoConstraints = false,
							StringValue = eventViewModel.Name,
						};
						viewContainer.AddSubview (title);

						viewContainer.AddConstraints (new[] {
							NSLayoutConstraint.Create (title, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, viewContainer, NSLayoutAttribute.CenterY, 1, 0),
							NSLayoutConstraint.Create (title, NSLayoutAttribute.Left, NSLayoutRelation.Equal, viewContainer,  NSLayoutAttribute.Left, 1f, 0f),
							NSLayoutConstraint.Create (title, NSLayoutAttribute.Right, NSLayoutRelation.Equal, viewContainer,  NSLayoutAttribute.Right, 1f, -10f),
							NSLayoutConstraint.Create (title, NSLayoutAttribute.Height, NSLayoutRelation.Equal, viewContainer, NSLayoutAttribute.Height, 1, 0),
						});
					}

					return viewContainer;
				}
			}

			return null;
		}

		public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
		{
			return 24f;
		}
	}
}