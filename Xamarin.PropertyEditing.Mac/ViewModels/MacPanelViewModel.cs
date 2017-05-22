using System;
using System.Linq;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac.ViewModels
{
	internal class MacPanelViewModel : PanelViewModel
	{
		public MacPanelViewModel (IEditorProvider provider)
					: base (provider)
		{
			ViewModelMap.Add (typeof (CoreGraphics.CGPoint), (p, e) => new PropertyViewModel<CoreGraphics.CGPoint> (p, e));
		}

		public int GetChildrenCount (NSObject item)
		{
			if (ArrangeMode == PropertyArrangeMode.Name) {
				return Properties.Count;
			}
			else {
				if (item == null) {
					var count = Properties.GroupBy (arg => arg.Category).Count ();
					return count;
				}
				else {
					var count = GetRootNodeByCategory (item).Count ();
					return count;
				}
			}
		}

		public NSObjectFacade GetChildObject (int childIndex, NSObject item)
		{
			if (ArrangeMode == PropertyArrangeMode.Name) {
				return NSObjectFacade.WrapIt (Properties[childIndex]);
			}
			else {
				// It item null it's a top level node
				if (item == null) {
					var listItem = Properties.GroupBy (arg => arg.Category).ToList ()[childIndex];
					return NSObjectFacade.WrapIt (null, listItem.Key);
				}
				else {
					var facade = (item as NSObjectFacade);
					if (!string.IsNullOrEmpty (facade.CategoryName)) {
						var rootNode = GetRootNodeByCategory (item);
						return NSObjectFacade.WrapIt (rootNode.ElementAt (childIndex));
					}
					else {
						return null;
					}
				}
			}
		}

		public IGrouping<string, PropertyViewModel> GetRootNodeByCategory (NSObject item)
		{
			var facade = (item as NSObjectFacade);
			var root = Properties.GroupBy (arg => arg.Category).First ((arg1) => arg1.Key == facade.CategoryName);
			return root;
		}

		public bool ItemExpandable (NSObject item)
		{
			if (ArrangeMode == PropertyArrangeMode.Name) {
				return false;
			}
			else {
				return string.IsNullOrEmpty ((item as NSObjectFacade).CategoryName) ? false : true;
			}
		}
	}

	class NSObjectFacade : NSObject
	{
		public object WrappedObject;
		public string CategoryName;

		public NSObjectFacade (object obj, string categoryName = null)
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
