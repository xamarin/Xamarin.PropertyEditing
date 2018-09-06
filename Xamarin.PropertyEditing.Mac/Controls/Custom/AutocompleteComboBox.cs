using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using AppKit;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac.Controls
{
	internal class AutocompleteComboBox : NSComboBox
	{
		private readonly PropertyViewModel viewModel;
		private readonly PropertyInfo previewCustomExpressionPropertyInfo;
		private ObservableCollectionEx<string> values;

		public AutocompleteComboBox (PropertyViewModel viewModel, ObservableCollectionEx<string> values, PropertyInfo previewCustomExpressionPropertyInfo)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			if (values == null)
				throw new ArgumentNullException (nameof (values));

			if (previewCustomExpressionPropertyInfo == null)
				throw new ArgumentNullException (nameof (previewCustomExpressionPropertyInfo));

			this.viewModel = viewModel;
			this.values = values;
			this.previewCustomExpressionPropertyInfo = previewCustomExpressionPropertyInfo;

			this.values.CollectionChanged += (sender, e) => {
				switch (e.Action) {
					case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null) {
						var items = e.NewItems;
						int startIndex = e.NewStartingIndex;
						if (startIndex != -1 && startIndex < items.Count) {
							for (var i = 0; i < items.Count; i++) {
								Insert (new NSString ((string)items[i]), startIndex);
								startIndex++;
							}
						} else {
							PopulateComboBoxItems (items);
						}
					}
					break;

					case NotifyCollectionChangedAction.Remove:
						if (e.OldItems != null) {
							var items = e.OldItems;
							int startIndex = e.OldStartingIndex;
							if (startIndex != -1 && startIndex < items.Count) {
								for (var i = 0; i < items.Count; i++) {
									RemoveAt (startIndex);
								}
							}
					}
					break;

					case NotifyCollectionChangedAction.Move:
					case NotifyCollectionChangedAction.Replace:
					case NotifyCollectionChangedAction.Reset:
						RemoveAll ();
						PopulateComboBoxItems (this.values);
						break;

					default:
						break;
				}
			};

			// Maximum number of items in the drop-down before scrolling kicks in.
			VisibleItems = 8;

			Changed += (sender, e) => {
				// Pop the list open if we start typing so we can see what we have to choose from
				this.Cell.AccessibilityExpanded = true;

				this.previewCustomExpressionPropertyInfo.SetValue (this.viewModel, StringValue);
			};

			this.previewCustomExpressionPropertyInfo.SetValue (this.viewModel, StringValue);
		}

		private void PopulateComboBoxItems (IList items)
		{
			for (var i = 0; i < items.Count; i++) {
				Add (new NSString (this.values[i]));
			}
		}
	}
}
