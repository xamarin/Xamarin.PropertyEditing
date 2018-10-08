using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class PropertyTableDelegate
		: NSOutlineViewDelegate
	{
		public PropertyTableDelegate (PropertyTableDataSource datasource)
		{
			this.dataSource = datasource;
		}

		public void UpdateExpansions (NSOutlineView outlineView)
		{
			this.isExpanding = true;

			if (!String.IsNullOrWhiteSpace (this.dataSource.DataContext.FilterText)) {
				outlineView.ExpandItem (null, true);
			} else {
				foreach (IGrouping<string, EditorViewModel> g in this.dataSource.DataContext.ArrangedEditors) {
					NSObject item;
					if (!this.dataSource.TryGetFacade (g, out item))
						continue;

					if (this.dataSource.DataContext.GetIsExpanded (g.Key))
						outlineView.ExpandItem (item);
					else
						outlineView.CollapseItem (item);
				}
			}
			this.isExpanding = false;
		}

		public override void DidAddRowView (NSOutlineView outlineView, NSTableRowView rowView, nint row)
		{
			// Let's make the columns look pretty by applying the Golden Ratio
			if (!goldenRatioApplied) {
				int middleColumnWidth = 5;
				nfloat rightColumnWidth = (outlineView.Frame.Width - middleColumnWidth) / 1.618f;
				nfloat leftColumnWidth = outlineView.Frame.Width - rightColumnWidth - middleColumnWidth;
				outlineView.TableColumns ()[0].Width = leftColumnWidth;
				outlineView.TableColumns ()[1].Width = rightColumnWidth;
				goldenRatioApplied = true;
			}
		}

		// the table is looking for this method, picks it up automagically
		public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			PropertyViewModel vm;
			IGroupingList<string, EditorViewModel> group;
			string cellIdentifier;
			GetVMGroupCellItendifiterFromFacade (item, out vm, out group, out cellIdentifier);

			var isGrouping = group != null;
			// Setup view based on the column
			switch (tableColumn.Identifier) {
				case PropertyEditorPanel.PropertyListColId:
					if (vm != null || isGrouping) {

						var view = (UnfocusableTextField)outlineView.MakeView (LabelIdentifier, this);
						if (view == null) {
							view = new UnfocusableTextField {
								Identifier = LabelIdentifier,
								Alignment = NSTextAlignment.Right,
							};
						}

						view.StringValue = (isGrouping ? group.Key : vm.Property.Name + ":") ?? String.Empty;

						// Set tooltips only for truncated strings
						var stringWidth = view.AttributedStringValue.Size.Width + 30;
						if (stringWidth > tableColumn.Width) {
							view.ToolTip = vm.Property.Name;
						}

						return view;
					} else {
						var view = (PanelHeaderLabelControl)outlineView.MakeView (PanelHeaderLabelControl.PanelHeaderLabelIdentifierString, this);
						if (view == null) {
							view = new PanelHeaderLabelControl (dataSource.DataContext);
						}
						return view;
					}

				case PropertyEditorPanel.PropertyEditorColId:
					if (cellIdentifier != null && this.firstCache.TryGetValue (cellIdentifier, out PropertyEditorControl editor)) {
						this.firstCache.Remove (cellIdentifier);
					} else
						editor = GetEditor (cellIdentifier, vm, outlineView, isGrouping);

					// If still null we have no editor yet.
					if (editor == null)
						return new NSView ();

					if (vm != null) {
						// we must reset these every time, as the view may have been reused
						editor.TableRow = outlineView.RowForItem (item);
						editor.ViewModel = vm;

						// Force a row update due to new height, but only when we are non-default
						if (editor.TriggerRowChange)
							outlineView.NoteHeightOfRowsWithIndexesChanged (new NSIndexSet (editor.TableRow));
					}

					return editor;
			}

			throw new Exception ("Unknown column identifier: " + tableColumn.Identifier);
		}

		public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
		{
			return (!(item is NSObjectFacade) || !(((NSObjectFacade)item).Target is IGroupingList<string, EditorViewModel>));
		}

		public override void ItemDidExpand (NSNotification notification)
		{
			if (this.isExpanding)
				return;

			NSObjectFacade facade = notification.UserInfo.Values[0] as NSObjectFacade;
			var group = facade.Target as IGroupingList<string, EditorViewModel>;
			if (group != null)
				this.dataSource.DataContext.SetIsExpanded (group.Key, isExpanded: true);
		}

		public override void ItemDidCollapse (NSNotification notification)
		{
			if (this.isExpanding)
				return;

			NSObjectFacade facade = notification.UserInfo.Values[0] as NSObjectFacade;
			var group = facade.Target as IGroupingList<string, EditorViewModel>;
			if (group != null)
				this.dataSource.DataContext.SetIsExpanded (group.Key, isExpanded: false);
		}

		public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
		{
			PropertyViewModel vm;
			IGroupingList<string, EditorViewModel> group;
			string cellIdentifier;
			GetVMGroupCellItendifiterFromFacade (item, out vm, out group, out cellIdentifier);

			var isGrouping = group != null;
			if (isGrouping)
				return 30;

			if (!this.registrations.TryGetValue (cellIdentifier, out EditorRegistration registration)) {
				var view = GetEditor (cellIdentifier, vm, outlineView, isGrouping);
				if (view == null) {
					this.registrations[cellIdentifier] = registration = new EditorRegistration {
						RowSize = PropertyEditorControl.DefaultControlHeight
					};
				} else if (view.TriggerRowChange) {
					this.registrations[cellIdentifier] = registration = new EditorRegistration {
						SizingInstance = view
					};

					// We're cheating by declaring GetHeight should act static, so we can call it from
					// an instance that is being used elsewhere.
					this.firstCache[cellIdentifier] = view;
				} else {
					this.registrations[cellIdentifier] = registration = new EditorRegistration {
						RowSize = view.GetHeight (vm)
					};

					this.firstCache[cellIdentifier] = view;
				}
			}

			return registration.GetHeight (vm);
		}

		private class EditorRegistration
		{
			public nint RowSize;
			public PropertyEditorControl SizingInstance;

			public nint GetHeight (PropertyViewModel vm)
			{
				if (SizingInstance != null)
					return SizingInstance.GetHeight (vm);
				else
					return RowSize;
			}
		}

		public const string LabelIdentifier = "label";

		private PropertyTableDataSource dataSource;
		private bool isExpanding;
		private bool goldenRatioApplied = false;

		private readonly Dictionary<string, EditorRegistration> registrations = new Dictionary<string, EditorRegistration> ();
		private readonly Dictionary<string, PropertyEditorControl> firstCache = new Dictionary<string, PropertyEditorControl> ();

		private PropertyEditorControl GetEditor (string identifier, EditorViewModel vm, NSOutlineView outlineView, bool isGrouping)
		{
			var view = (PropertyEditorControl)outlineView.MakeView (identifier, this);
			if (view != null)
				return view;

			if (vm != null) {
				Type[] genericArgs = null;
				Type controlType;
				Type propertyType = vm.GetType ();
				if (!ViewModelTypes.TryGetValue (propertyType, out controlType)) {
					if (propertyType.IsConstructedGenericType) {
						genericArgs = propertyType.GetGenericArguments ();
						propertyType = propertyType.GetGenericTypeDefinition ();
						ViewModelTypes.TryGetValue (propertyType, out controlType);
					}
				}

				if (controlType == null)
					return null;

				if (controlType.IsGenericTypeDefinition) {
					if (genericArgs == null)
						genericArgs = propertyType.GetGenericArguments ();

					controlType = controlType.MakeGenericType (genericArgs);
				}

				view = (PropertyEditorControl)Activator.CreateInstance (controlType);
				view.Identifier = identifier;
				view.TableView = outlineView;

				return view;
			} else {
				if (isGrouping)
					return null;
				else
					return new PanelHeaderEditorControl (dataSource.DataContext);
			}
		}

		private void GetVMGroupCellItendifiterFromFacade (NSObject item, out PropertyViewModel vm, out IGroupingList<string, EditorViewModel> group, out string cellIdentifier)
		{
			var facade = (NSObjectFacade)item;
			vm = facade.Target as PropertyViewModel;
			group = facade.Target as IGroupingList<string, EditorViewModel>;
			cellIdentifier = facade.Target is PanelHeaderEditorControl pvh
								   ? nameof (PanelHeaderEditorControl)
								   : (group == null) ? vm.GetType ().FullName : group.Key;
		}

		private static readonly Dictionary<Type, Type> ViewModelTypes = new Dictionary<Type, Type> {
			{typeof (StringPropertyViewModel), typeof (StringEditorControl)},
			{typeof (NumericPropertyViewModel<>), typeof (NumericEditorControl<>)},
			{typeof (PropertyViewModel<bool?>), typeof (BooleanEditorControl)},
			{typeof (PredefinedValuesViewModel<>), typeof(PredefinedValuesEditor<>)},
			{typeof (CombinablePropertyViewModel<>), typeof(CombinablePropertyEditor<>)},
			{typeof (PropertyViewModel<CoreGraphics.CGPoint>), typeof (CGPointEditorControl)},
			{typeof (PropertyViewModel<CoreGraphics.CGRect>), typeof (CGRectEditorControl)},
			{typeof (PropertyViewModel<CoreGraphics.CGSize>), typeof (CGSizeEditorControl)},
			{typeof (PointPropertyViewModel), typeof (CommonPointEditorControl) },
			{typeof (RectanglePropertyViewModel), typeof (CommonRectangleEditorControl) },
			{typeof (SizePropertyViewModel), typeof (CommonSizeEditorControl) },
			{typeof (PropertyViewModel<Point>), typeof (SystemPointEditorControl)},
			{typeof (PropertyViewModel<Size>), typeof (SystemSizeEditorControl)},
			{typeof (PropertyViewModel<Rectangle>), typeof (SystemRectangleEditorControl)},
			{typeof (BrushPropertyViewModel), typeof (BrushEditorControl)},
			{typeof (RatioViewModel), typeof (RatioEditorControl<CommonRatio>)},
		};
	}
}
