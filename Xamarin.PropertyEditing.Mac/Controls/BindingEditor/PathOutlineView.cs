using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PathOutlineView : BaseSelectorOutlineView
	{
		private IReadOnlyCollection<object> itemsSource;
		private PropertyTreeRoot propertyTreeRoot;

		private void SetItemsSource (IReadOnlyCollection<object> value, string targetName)
		{
			if (this.itemsSource != value) {
				this.itemsSource = value;

				DataSource = new PathOutlineViewDataSource (this.itemsSource, targetName); ;
				Delegate = new PathOutlineViewDelegate ();

				ReloadData ();

				ExpandItem (ItemAtRow (0));
			}
		}

		public PropertyTreeRoot PropertyTreeRoot
		{
			get { return this.propertyTreeRoot; }
			internal set
			{
				if (this.propertyTreeRoot != value) {
					this.propertyTreeRoot = value;
					if (this.propertyTreeRoot != null) {
						SetItemsSource (this.propertyTreeRoot.Children, this.propertyTreeRoot.TargetType.Name);
					} else {
						SetItemsSource (null, string.Empty);
					}
				}
			}
		}
	}

	internal class PathOutlineViewDelegate : NSOutlineViewDelegate
	{
		private const string PathIdentifier = "path";

		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			var labelContainer = (UnfocusableTextField)outlineView.MakeView (PathIdentifier, this);
			if (labelContainer == null) {
				labelContainer = new UnfocusableTextField {
					Identifier = PathIdentifier,
				};
			}
			var target = (item as NSObjectFacade).Target;

			switch (target) {
			case PropertyTreeElement propertyTreeElement:
				labelContainer.StringValue = string.Format ("{0}: ({1})", propertyTreeElement.Property.Name, propertyTreeElement.Property.RealType.Name);
				break;

			case string targetName:
				labelContainer.StringValue = targetName;
				break;

			default:
				labelContainer.StringValue = Properties.Resources.TypeNotSupported;
				break;
			}

			return labelContainer;
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			if (item is NSObjectFacade facade) {
				switch (facade.Target) {
				case PropertyTreeElement propertyTreeElement:
					var propertyTreeResult = propertyTreeElement.Children.Task.Result;
					return propertyTreeResult.Count == 0;

				default:
					return false;
				}
			} else {
				return false;
			}
		}
	}

	internal class PathOutlineViewDataSource : NSOutlineViewDataSource
	{
		private readonly IReadOnlyCollection<object> itemsSource;
		private readonly string targetName;

		internal PathOutlineViewDataSource (IReadOnlyCollection<object> itemsSource, string targetName)
		{
			this.itemsSource = itemsSource;
			this.targetName = targetName;
		}

		public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (item == null) {
				return this.itemsSource != null ? this.itemsSource.Count + 1 : 0;
			} else {
				var target = (item as NSObjectFacade).Target;
				switch (target) {
				case PropertyTreeElement propertyTreeElement:
					IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
					return propertyTrees.Count;

				case string targetName:
					return this.itemsSource.Count;

				default:
					return 0;
				}
			}
		}

		public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
		{
			if (childIndex == 0 && item == null) {
				return new NSObjectFacade (targetName);
			}

			if (item is NSObjectFacade objectFacade) {
				var target = objectFacade.Target;
				switch (target) {
				case PropertyTreeElement propertyTreeElement:
					IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
					return new NSObjectFacade (propertyTrees.ElementAt ((int)childIndex));

				case string targetName:
					return new NSObjectFacade (this.itemsSource.ElementAt ((int)childIndex));

				default:
					return null;
				}

			}
			return null;
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			if (item is NSObjectFacade objectFacade) {
				var target = objectFacade.Target;
				switch (target) {
				case PropertyTreeElement propertyTreeElement:
					IReadOnlyCollection<PropertyTreeElement> propertyTrees = propertyTreeElement.Children.Task.Result;
					return propertyTrees.Count > 0;

				case string targetName:
					return true;

				default:
					return false;
				}
			}

			return false;
		}
	}
}
