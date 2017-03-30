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
		internal PropertyTableDataSource (PanelViewModel viewModel)
		{
			ViewModel = viewModel;
			PanelViewModel.ViewModelMap.Add (typeof (CoreGraphics.CGPoint), (p, e) => new PropertyViewModel<CoreGraphics.CGPoint> (p, e));
			PanelViewModel.ViewModelMap.Add (typeof (CoreGraphics.CGRect), (p, e) => new PropertyViewModel<CoreGraphics.CGRect> (p, e));
		}

		internal PanelViewModel ViewModel { get; private set; }
		public ICollection<object> SelectedItems => this.ViewModel.SelectedObjects;
		List<PropertyViewModel> properties => this.ViewModel.Properties.ToList ();
		IEnumerable<IGrouping<string, PropertyViewModel>> propertiesGroupBy => this.properties.GroupBy (arg => arg.Category);
		List<IGrouping<string, PropertyViewModel>> propertiesGroupByList => this.propertiesGroupBy.ToList ();

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (this.ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return this.properties.Count;
			}
			else {
				if (item == null) {
					var count = propertiesGroupBy.Count ();
					return count;
				}
				else {
					var facade = (item as NSObjectFacade);
					var where = propertiesGroupBy.Where ((arg1, arg2) => arg1.Key == facade.CategoryName);
					var count = where.ToList ()[0].Count ();
					return count;
				}
			}
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			if (this.ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
				return NSObjectFacade.WrapIt (this.properties[(int)childIndex]);
			}
			else {
				if (item == null) {
					var listItem = propertiesGroupByList[(int)childIndex];
					return NSObjectFacade.WrapIt (null, listItem.Key);
				}
				else {
					var facade = (item as NSObjectFacade);
					if (!string.IsNullOrEmpty (facade.CategoryName)) {
						var where = propertiesGroupBy.Where ((arg1, arg2) => arg1.Key == facade.CategoryName);
						var wherelist = where.ToList ();
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
			if (this.ViewModel.ArrangeMode == PropertyArrangeMode.Name) {
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
