using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal static class XamlHelper
	{
		public static IEnumerable<T> GetDescendants<T> (this UIElement parent)
			where T : UIElement
		{
			var childCount = VisualTreeHelper.GetChildrenCount (parent);
			for (var i = 0; i < childCount; i++) {
				if (VisualTreeHelper.GetChild (parent, i) is UIElement child) {
					if (child is T correctlyTypedChild) yield return correctlyTypedChild;
					IEnumerable<T> grandChildren = GetDescendants<T> (child);
					foreach (T grandChild in grandChildren) yield return grandChild;
				}
			}
		}
	}
}
