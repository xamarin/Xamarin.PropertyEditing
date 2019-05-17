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

		public static T FindChildOrSelf<T> (this UIElement self)
			where T : UIElement
		{
			if (self == null)
				throw new ArgumentNullException (nameof(self));

			if (self is T t)
				return t;

			int count = VisualTreeHelper.GetChildrenCount (self);
			for (int i = 0; i < count; i++) {
				if (VisualTreeHelper.GetChild (self, i) is UIElement element) {
					var child = FindChildOrSelf<T> (element);
					if (child != null)
						return child;
				}
			}

			return null;
		}

		public static T FindParent<T> (this UIElement self)
			where T : UIElement
		{
			UIElement parent = VisualTreeHelper.GetParent (self) as UIElement;
			if (parent == null)
				return default(T);

			return parent.FindParentOrSelf<T>();
		}

		public static FrameworkElement FindPropertiesHost (this FrameworkElement self)
		{
			DependencyObject parent = self;
			while (!(parent is IPropertiesHost) && parent != null) {
				parent = VisualTreeHelper.GetParent (parent);
			}

			return (FrameworkElement)parent;
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

		public static TParent FindParentUnless<TParent,TUnless> (this UIElement self)
			where TParent : UIElement
			where TUnless : UIElement
		{
			if (self == null)
				throw new ArgumentNullException (nameof (self));

			DependencyObject parent = VisualTreeHelper.GetParent (self);
			while (!(parent is TParent) && parent != null) {
				if (parent is TUnless)
					return default(TParent);

				parent = VisualTreeHelper.GetParent (parent);
			}

			return (TParent)parent;
		}
	}
}
