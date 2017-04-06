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

		List<PropertyViewModel> properties;
		List<PropertyViewModel> Properties {
			get {
				if (properties == null) {
					properties = ViewModel.Properties.ToList ();
				}
				return properties;
			}
		}

		IEnumerable<IGrouping<string, PropertyViewModel>> propertiesGroupBy;
		IEnumerable<IGrouping<string, PropertyViewModel>> PropertiesGroupBy {
			get {
				if (propertiesGroupBy == null) {
					propertiesGroupBy = ViewModel.Properties.GroupBy (arg => arg.Category);
				}
				return propertiesGroupBy;
			}
		}

		List<IGrouping<string, PropertyViewModel>> propertiesGroupByList;
		List<IGrouping<string, PropertyViewModel>> PropertiesGroupByList {
			get {
				if (propertiesGroupByList == null) {
					propertiesGroupByList = PropertiesGroupBy.ToList ();
				}
				return propertiesGroupByList;
			}
		}

		internal PropertyTableDataSource (PanelViewModel viewModel)
		{
			ViewModel = viewModel;
			PanelViewModel.ViewModelMap.Add (typeof (CoreGraphics.CGPoint), (p, e) => new PropertyViewModel<CoreGraphics.CGPoint> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (CoreGraphics.CGRect), (p, e) => new PropertyViewModel<CoreGraphics.CGRect> (p, e));
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return ViewModel.Properties.ToList ().Count;
			}
			else {
				if (item == null) {
					var count = PropertiesGroupBy.Count ();
					return count;
				}
				else {
					var count = GetPropertyGroupByWhere (item).ToList ()[0].Count ();
					return count;
				}
			}
		}

		IEnumerable<IGrouping<string, PropertyViewModel>> GetPropertyGroupByWhere (NSObject item)
		{
			var facade = (item as NSObjectFacade);
			var where = PropertiesGroupBy.Where ((arg1, arg2) => arg1.Key == facade.CategoryName);
			return where;
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			if (ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return NSObjectFacade.WrapIt (Properties[(int)childIndex]);
			}
			else {
				// It item null it's a top level node
				if (item == null) {
					var listItem = PropertiesGroupByList[(int)childIndex];
					return NSObjectFacade.WrapIt (null, listItem.Key);
				}
				else {
					var facade = (item as NSObjectFacade);
					if (!string.IsNullOrEmpty (facade.CategoryName)) {
						var wherelist = GetPropertyGroupByWhere (item).ToList ();
						return NSObjectFacade.WrapIt (wherelist[0].ElementAt ((int)childIndex));
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
