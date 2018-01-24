using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal static class XamlHelper
	{
		public static IEnumerable<T> GetDescendants<T> (this UIElement self)
			where T : UIElement
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));

			var childCount = VisualTreeHelper.GetChildrenCount (self);
			for (var i = 0; i < childCount; i++) {
				if (VisualTreeHelper.GetChild (self, i) is UIElement child) {
					if (child is T correctlyTypedChild) yield return correctlyTypedChild;
					IEnumerable<T> grandChildren = GetDescendants<T> (child);
					foreach (T grandChild in grandChildren) yield return grandChild;
				}
			}
		}

		public static T FindParent<T> (this UIElement self)
			where T : UIElement
		{
			UIElement parent = VisualTreeHelper.GetParent (self) as UIElement;
			if (parent == null)
				return default(T);

			return parent.FindParentOrSelf<T>();
		}

		public static T FindParentOrSelf<T> (this UIElement self)
			where T : UIElement
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));

			DependencyObject parent = self;
			while (!(parent is T) && parent != null) {
				parent = VisualTreeHelper.GetParent (parent);
			}

			return (T)parent;
		}
	}
}
