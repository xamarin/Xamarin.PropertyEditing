using System;
using System.Collections.Generic;
using AppKit;
using Foundation;
using ObjCRuntime;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class ResourceTableDelegate
		: NSTableViewDelegate
	{
		const string iconIdentifier = "icon";
		const string typeIdentifier = "type";
		const string nameIdentifier = "name";
		const string valueIdentifier = "value";

		public event EventHandler ResourceSelected;

		public ResourceTableDelegate (IHostResourceProvider hostResources, ResourceTableDataSource resourceTableDatasource)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.datasource = resourceTableDatasource;
			this.hostResources = hostResources;
		}

		// the table is looking for this method, picks it up automagically
		public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			var resource = datasource.ViewModel.Resources [(int)row] as Resource;

			// Setup view based on the column
			switch (tableColumn.Identifier) {
				case RequestResourcePanel.ResourceImageColId:
					if (resource.Source.Type != ResourceSourceType.Application) {
						var iconView = (NSImageView)tableView.MakeView (iconIdentifier, this);
						if (iconView == null) {
							iconView = new NSImageView {
								Image = this.hostResources.GetNamedImage ("pe-resource-editor-32"),
								ImageScaling = NSImageScale.None,
								Identifier = iconIdentifier,
							};
						}
						return iconView;
					}

					// If we get here its a local resource
					return new NSView ();

				case RequestResourcePanel.ResourceTypeColId:
					var typeView = (UnfocusableTextField)tableView.MakeView (typeIdentifier, this);
					if (typeView == null) {
						typeView = new UnfocusableTextField {
							Identifier = typeIdentifier,
						};
					}

					typeView.StringValue = resource.Source.Name;
					return typeView;

				case RequestResourcePanel.ResourceNameColId:
					var nameView = (UnfocusableTextField)tableView.MakeView (nameIdentifier, this);
					if (nameView == null) {
						nameView = new UnfocusableTextField {
							Identifier = nameIdentifier,
						};
					}

					nameView.StringValue = resource.Name;
					return nameView;

				case RequestResourcePanel.ResourceValueColId:
					var valueView = MakeValueView (resource, tableView);

					// If still null we have no editor yet.
					valueView = valueView ?? new NSView ();

					return valueView;

				default:
					return base.GetViewForItem (tableView, tableColumn, row);
			}
		}

		public override nfloat GetRowHeight (NSTableView tableView, nint row)
		{
			return 24;
		}

		public override void SelectionDidChange (NSNotification notification)
		{
			if (notification.Object is NSTableView tableView) {
				if (previousRow != -1
				    && previousRow < tableView.RowCount
				    && tableView.GetView (0, previousRow, false) is NSImageView previousIconColumn) {
					previousIconColumn.Image = this.hostResources.GetNamedImage ("pe-resource-editor-32");
				}

				if (tableView.SelectedRow != -1
				    && tableView.GetView (0, tableView.SelectedRow, false) is NSImageView selectedIconColumn) {
					selectedIconColumn.Image = this.hostResources.GetNamedImage ("pe-resource-editor-32~sel");
					previousRow = tableView.SelectedRow;
				}
			}
			ResourceSelected?.Invoke (this, EventArgs.Empty);
		}

		private readonly IHostResourceProvider hostResources;
		private readonly ResourceTableDataSource datasource;
		private nint previousRow = -1;

		private NSView MakeValueView (Resource resource, NSTableView tableView)
		{
			var view = (NSView)tableView.MakeView (valueIdentifier, this);
			if (view == null) {
				view = GetValueView (resource.RepresentationType);
			}

			CommonBrush commonBrush = BrushPropertyViewModel.GetCommonBrushForResource (resource);

			if (commonBrush != null && view is CommonBrushView commonBrushView) {
				commonBrushView.Brush = commonBrush;
			}

			return view;
		}

		private NSView GetValueView (Type representationType)
		{
			Type[] genericArgs = null;
			Type valueRenderType;
			if (!ValueTypes.TryGetValue (representationType, out valueRenderType)) {
				if (representationType.IsConstructedGenericType) {
					genericArgs = representationType.GetGenericArguments ();
					var type = representationType.GetGenericTypeDefinition ();
					ValueTypes.TryGetValue (type, out valueRenderType);
				}
			}
			if (valueRenderType == null)
				return null;

			if (valueRenderType.IsGenericTypeDefinition) {
				if (genericArgs == null)
					genericArgs = representationType.GetGenericArguments ();
				valueRenderType = valueRenderType.MakeGenericType (genericArgs);
			}

			return SetUpRenderer (valueRenderType);
		}

		// set up the editor based on the type of view model
		private NSView SetUpRenderer (Type valueRenderType)
		{
			var view = (NSView)Activator.CreateInstance (valueRenderType, this.hostResources);

			return view;
		}

		internal static readonly Dictionary<Type, Type> ValueTypes = new Dictionary<Type, Type> {
			{typeof (CommonSolidBrush), typeof (CommonBrushView)},
			{typeof (CommonColor), typeof (CommonBrushView)},
		};
	}
}
