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
		public PropertyInfo PreviewCustomExpressionPropertyInfo
		{
			get { return this.PreviewCustomExpressionPropertyInfo; }
			set {
				this.PreviewCustomExpressionPropertyInfo = value;
			}
		}
		private ObservableCollectionEx<string> values;

		protected IHostResourceProvider HostResources {
			get;
			private set;
		}

		public AutocompleteComboBox (IHostResourceProvider hostResources, PropertyViewModel viewModel, ObservableCollectionEx<string> values, PropertyInfo previewCustomExpressionPropertyInfo)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));

			if (values == null)
				throw new ArgumentNullException (nameof (values));

			if (previewCustomExpressionPropertyInfo == null)
				throw new ArgumentNullException (nameof (previewCustomExpressionPropertyInfo));

			HostResources = hostResources;

			this.viewModel = viewModel;
			this.values = values;
			this.previewCustomExpressionPropertyInfo = previewCustomExpressionPropertyInfo;

			this.values.CollectionChanged += (sender, e) => {
				switch (e.Action) {
					case NotifyCollectionChangedAction.Add:
						if (e.NewStartingIndex == -1 || e.NewItems == null) {
							Reset ();
						} else {
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
						if (e.OldStartingIndex == -1 || e.OldItems == null) {
							Reset ();
						} else {
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
					Reset ();
					break;

				default:
						break;
				}
			};

			// Maximum number of items in the drop-down before scrolling kicks in.
			VisibleItems = 8;

			var selectorPopup = new Selector ("popUp:");

			Changed += (sender, e) => {
				if (!Cell.AccessibilityExpanded) {
					Cell.PerformSelector (selectorPopup);
				}

				UpdatePropertyInfo ();
			};

			UpdatePropertyInfo ();

			ViewDidChangeEffectiveAppearance ();
		}

		private void Reset ()
		{
			RemoveAll ();
			PopulateComboBoxItems (this.values);
		}

		private void PopulateComboBoxItems (IList items)
		{
			for (var i = 0; i < items.Count; i++) {
				Add (new NSString (this.values[i]));
			}
		}

		public void UpdatePropertyInfo ()
		{
			this.previewCustomExpressionPropertyInfo.SetValue (this.viewModel, StringValue);
		}
	}
}
