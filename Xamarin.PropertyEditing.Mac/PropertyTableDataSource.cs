using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	public class PropertyTableDataSource : NSOutlineViewDataSource
	{
		internal PanelViewModel ViewModel { get; private set; }
		public ICollection<object> SelectedItems => ViewModel.SelectedObjects;

		internal PropertyTableDataSource (PanelViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return ViewModel.Properties.Count;
			}
			else {
				if (item == null) {
					var count = ViewModel.CachedPropertiesGroupBy.Count ();
					return count;
				}
				else {
					var count = GetRootNodeByCategory (item).Count ();
					return count;
				}
			}
		}

		IGrouping<string, PropertyViewModel> GetRootNodeByCategory (NSObject item)
		{
			var facade = (item as NSObjectFacade);
			var root = ViewModel.CachedPropertiesGroupBy.First ((arg1) => arg1.Key == facade.CategoryName);
			return root;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			if (ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return NSObjectFacade.WrapIt (ViewModel.CachedProperties[(int)childIndex]);
			}
			else {
				// It item null it's a top level node
				if (item == null) {
					var listItem = ViewModel.CachedPropertiesGroupBy.ToList ()[(int)childIndex];
					return NSObjectFacade.WrapIt (null, listItem.Key);
				}
				else {
					var facade = (item as NSObjectFacade);
					if (!string.IsNullOrEmpty (facade.CategoryName)) {
						var rootNode = GetRootNodeByCategory (item);
						return NSObjectFacade.WrapIt (rootNode.ElementAt ((int)childIndex));
					}
					else {
						return null;
					}
				}
			}
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			if (ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return false;
			}
			else {
				return string.IsNullOrEmpty ((item as NSObjectFacade).CategoryName) ? false : true;
			}
		}

		public override NSObject GetObjectValue (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			// I don't believe this is correct
			if (string.IsNullOrEmpty ((item as NSObjectFacade).CategoryName)) {
				return (NSString)((item as NSObjectFacade).WrappedObject as PropertyViewModel).Property.Name;
			}
			else {
				return (NSString)(item as NSObjectFacade).CategoryName;
			}
		}
	}

	class NSObjectFacade : NSObject
	{
		public object WrappedObject;
		public string CategoryName;

		public NSObjectFacade (object obj, string categoryName = null) : base ()
		{
			this.WrappedObject = obj;
			this.CategoryName = categoryName;
		}

		public static NSObjectFacade WrapIt (object obj, string categoryName = null)
		{
			return new NSObjectFacade (obj, categoryName);
		}
	}
}
