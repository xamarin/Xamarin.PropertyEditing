using System;
using System.Linq;
using AppKit;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingResourceOutlineView : BaseSelectorOutlineView
	{
		private ILookup<ResourceSource, Resource> itemsSource;
		public ILookup<ResourceSource, Resource> ItemsSource
		{
			get => this.itemsSource;
			set
			{
				if (this.itemsSource != value) {
					this.itemsSource = value;

					DataSource = new BindingResourceOutlineViewDataSource (this.itemsSource);
					Delegate = new BindingResourceOutlineViewDelegate ();
				}

				ReloadData ();

				ExpandItem (null, true);
			}
		}
	}

	internal class BindingResourceOutlineViewDelegate : NSOutlineViewDelegate
	{
		private const string ResourceIdentifier = "resource";

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var labelContainer = (UnfocusableTextField)outlineView.MakeView (ResourceIdentifier, this);
			if (labelContainer == null) {
				labelContainer = new UnfocusableTextField {
					Identifier = ResourceIdentifier,
				};
			}
			var target = (item as NSObjectFacade).Target;

			switch (target) {
			case IGrouping<ResourceSource, Resource> kvp:
				labelContainer.StringValue = kvp.Key.Name;
				break;
			case Resource resource:
				labelContainer.StringValue = resource.Name;
				break;
			default:
				labelContainer.StringValue = Properties.Resources.ResourceNotSupported;
				break;
			}

			return labelContainer;
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			var target = (item as NSObjectFacade).Target;
			switch (target) {
			case IGrouping<ResourceSource, Resource> kvp:
				return false;
			case Resource resource:
				return true;

			default:
				return false;
			}
		}
	}

	internal class BindingResourceOutlineViewDataSource : NSOutlineViewDataSource
	{
		public ILookup<ResourceSource, Resource> ItemsSource { get; }

		internal BindingResourceOutlineViewDataSource (ILookup<ResourceSource, Resource> itemsSource)
		{
			if (itemsSource == null)
				throw new ArgumentNullException (nameof (itemsSource));

			ItemsSource = itemsSource;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (item == null) {
				return ItemsSource != null ? ItemsSource.Count : 0;
			} else {
				var target = (item as NSObjectFacade).Target;
				switch (target) {
				case IGrouping<ResourceSource, Resource> kvp:
					return kvp.Count ();
				case Resource resource:
					return 0;
				default:
					return 0;
				}
			}
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			object element;

			if (item == null) {
				element = ItemsSource.ElementAt ((int)childIndex);
			} else {
				var target = (item as NSObjectFacade).Target;
				switch (target) {
				case IGrouping<ResourceSource, Resource> kvp:
					element = kvp.ElementAt ((int)childIndex);
					break;
				case Resource resource:
					element = resource;
					break;
				default:
					return null;
				}
			}

			return new NSObjectFacade (element);
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			var target = (item as NSObjectFacade).Target;
			switch (target) {
			case IGrouping<ResourceSource, Resource> kvp:
				return kvp.Any ();
			case Resource resource:
				return false;
			default:
				return false;
			}
		}
	}
}
