using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal static class BindingPath
	{
		public static string FromTree (PropertyTreeElement tree)
		{
			if (tree == null)
				throw new ArgumentNullException (nameof(tree));

			string newPath = String.Empty;
			while (tree != null) {
				string sep = (newPath != String.Empty) ? ((!tree.IsCollection) ? "." : "/") : String.Empty;
				newPath = tree.Property.Name + sep + newPath;
				tree = tree.Parent;
			}

			return newPath;
		}

		public static PropertyTreeElement GetSelectedElement (PropertyTreeRoot root, string path)
		{
			if (root == null)
				throw new ArgumentNullException (nameof(root));

			if (String.IsNullOrWhiteSpace (path))
				return null;

			path = path.Trim ();
			if (path == ".")
				return null;

			PropertyTreeElement element = null;
			int index = 0;
			while (index < path.Length) {
				int sepIndex = path.IndexOfAny (new[] { '.', '/', '[' }, index);
				if (sepIndex == 0) {
					index++;
					continue;
				}

				string part = path.Substring (index, (sepIndex == -1) ? path.Length : sepIndex).Trim();
				if (part != "." && path != String.Empty) {
					element = root.Children.FirstOrDefault (e => e.Property.Name == part);
					if (element == null)
						return null;
				}

				if (sepIndex != -1) {
					char sep = path[sepIndex];
					if (sep == '[')
						index = path.IndexOf (']', sepIndex);
					else
						index = sepIndex + 1;
				} else {
					break;
				}
			}

			return element;
		}
	}
}
