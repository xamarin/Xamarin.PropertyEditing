using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ObjectOutlineView : BaseSelectorOutlineView
	{
		private IReadOnlyList<ObjectTreeElement> itemsSource;
		public IReadOnlyList<ObjectTreeElement> ItemsSource
		{
			get => this.itemsSource;
			set
			{
				if (this.itemsSource != value) {
					this.itemsSource = value;

					DataSource = new ObjectOutlineViewDataSource (this.itemsSource); ;
					Delegate = new ObjectOutlineViewDelegate ();
				}

				ReloadData ();

				ExpandItem (null, true);
			}
		}
	}

	internal class ObjectOutlineViewDelegate : NSOutlineViewDelegate
	{
		private const string TypeIdentifier = "type";

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var labelContainer = (UnfocusableTextField)outlineView.MakeView (TypeIdentifier, this);
			if (labelContainer == null) {
				labelContainer = new UnfocusableTextField {
					Identifier = TypeIdentifier,
				};
			}
			var target = (item as NSObjectFacade).Target;

			switch (target) {
			case KeyValuePair<string, SimpleCollectionView> kvp:
				labelContainer.StringValue = kvp.Key;
				break;
			case TypeInfo info:
				labelContainer.StringValue = info.Name;
				break;
			default:
				labelContainer.StringValue = Properties.Resources.TypeNotSupported;
				break;
			}

			return labelContainer;
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			var target = (item as NSObjectFacade).Target;
			switch (target) {
			case KeyValuePair<string, SimpleCollectionView> kvp:
				return false;
			case TypeInfo info:
				return true;

			default:
				return false;
			}
		}
	}

	internal class ObjectOutlineViewDataSource : NSOutlineViewDataSource
	{
		public IReadOnlyList<ObjectTreeElement> ItemsSource { get; }

		internal ObjectOutlineViewDataSource (IReadOnlyList<ObjectTreeElement> itemsSource)
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
				case KeyValuePair<string, SimpleCollectionView> kvp:
					return kvp.Value.Count;
				case TypeInfo info:
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
				case KeyValuePair<string, SimpleCollectionView> kvp:
					element = kvp.Value[(int)childIndex];
					break;
				case TypeInfo info:
					element = info;
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
			case KeyValuePair<string, SimpleCollectionView> kvp:
				return kvp.Value.Count > 0;
			case TypeInfo info:
				return false;
			default:
				return false;
			}
		}
	}
}
