using System;
using System.Collections.Specialized;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class CollectionEditorControl
		: NotifyingView<CollectionPropertyViewModel>
	{
		public CollectionEditorControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			this.collectionView = new NSTableView {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityCollectionEditorTable,
				HeaderView = null,
				RowHeight = 24
			};

			this.collectionView.RegisterForDraggedTypes (new[] { "public.data" });
			this.collectionView.AddColumn (new NSTableColumn ());

			var scroll = new NSScrollView {
				DocumentView = this.collectionView,
				HasHorizontalScroller = false,
			};

			var header = new DynamicBox (hostResources, NamedResources.ControlBackground, NamedResources.FrameBoxBorderColor) {
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				ContentView = new UnfocusableTextField { StringValue = Properties.Resources.CollectionTargetsHeader },
				TranslatesAutoresizingMaskIntoConstraints = false,
				ContentViewMargins = new CGSize (8, 0),
			};
			AddSubview (header);

			var headerBorder = new DynamicBox (hostResources, borderName: NamedResources.ListHeaderSeparatorColor) {
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ContentViewMargins = new CGSize (0, 0)
			};
			AddSubview (headerBorder);

			var scrollBorder = new DynamicBox (hostResources, borderName: NamedResources.FrameBoxBorderColor) {
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				ContentView = scroll,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ContentViewMargins = new CGSize (0, 0)
			};

			AddSubview (scrollBorder);

			this.typeSelector = new FocusableComboBox {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityCollectionTypeSelector,
				ControlSize = NSControlSize.Mini,
				Font = NSFont.SystemFontOfSize (NSFont.SystemFontSizeForControlSize (NSControlSize.Mini)),
				TranslatesAutoresizingMaskIntoConstraints = false,
			};
			this.typeSelector.SelectionChanged += OnSelectedTypeChanged;

			this.add = new FocusableButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityCollectionAddButton,
				BezelStyle = NSBezelStyle.SmallSquare,
				Bordered = false,
			};
			this.add.Activated += OnAddChild;

			var addBorder = new DynamicBox (hostResources, borderName: NamedResources.FrameBoxButtonBorderColor) {
				ContentView = this.add,
				TranslatesAutoresizingMaskIntoConstraints = false,
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				ContentViewMargins = new CGSize (0, 0)
			};

			this.remove = new FocusableButton {
				AccessibilityEnabled = true,
				AccessibilityTitle = Properties.Resources.AccessibilityCollectionRemoveButton,
				BezelStyle = NSBezelStyle.SmallSquare,
				Bordered = false
			};
			this.remove.Activated += OnRemoveChild;

			var removeBorder = new DynamicBox (hostResources, borderName: NamedResources.FrameBoxButtonBorderColor) {
				ContentView = this.remove,
				TranslatesAutoresizingMaskIntoConstraints = false,
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				ContentViewMargins = new CGSize (0, 0)
			};

			var controlsBorder = new DynamicBox (hostResources, NamedResources.FrameBoxBackgroundColor, NamedResources.FrameBoxBorderColor) {
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ContentViewMargins = new CoreGraphics.CGSize (2.5, 0)
			};
			AddSubview (controlsBorder);

			controlsBorder.AddSubview (this.typeSelector);
			controlsBorder.AddSubview (addBorder);
			controlsBorder.AddSubview (removeBorder);

			this.propertyList = new PropertyList {
				HostResourceProvider = hostResources,
				ShowHeader = false,
			};

			var listBorder = new DynamicBox (hostResources, borderName: NamedResources.FrameBoxBorderColor) {
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 1,
				ContentView = propertyList,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ContentViewMargins = new CoreGraphics.CGSize (0, 0)
			};

			AddSubview (listBorder);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, Mac.Layout.GoldenRatioLeft, 0),
				NSLayoutConstraint.Create (header, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 21),

				NSLayoutConstraint.Create (headerBorder, NSLayoutAttribute.Top, NSLayoutRelation.Equal, header, NSLayoutAttribute.Bottom, 1, -1),
				NSLayoutConstraint.Create (headerBorder, NSLayoutAttribute.Width, NSLayoutRelation.Equal, header, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (headerBorder, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 1),

				NSLayoutConstraint.Create (scrollBorder, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1, 0),
				NSLayoutConstraint.Create (scrollBorder, NSLayoutAttribute.Top, NSLayoutRelation.Equal, header, NSLayoutAttribute.Bottom, 1, -1),
				NSLayoutConstraint.Create (scrollBorder, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, Mac.Layout.GoldenRatioLeft, 0),
				NSLayoutConstraint.Create (scrollBorder, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.Top, 1, 1),

				NSLayoutConstraint.Create (controlsBorder, NSLayoutAttribute.Width, NSLayoutRelation.Equal, scrollBorder, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (controlsBorder, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 21),
				NSLayoutConstraint.Create (controlsBorder, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, 0),

				NSLayoutConstraint.Create (listBorder, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scrollBorder, NSLayoutAttribute.Right, 1, 8),
				NSLayoutConstraint.Create (listBorder, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 0),
				NSLayoutConstraint.Create (listBorder, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1, 0),
				NSLayoutConstraint.Create (listBorder, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.Bottom, 1, 0),
			
				NSLayoutConstraint.Create (this.typeSelector, NSLayoutAttribute.Height, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.Height, 1, -7),
				NSLayoutConstraint.Create (this.typeSelector, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (this.typeSelector, NSLayoutAttribute.Width, NSLayoutRelation.LessThanOrEqual, 1, 110),

				NSLayoutConstraint.Create (addBorder, NSLayoutAttribute.Height, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.Height, 1, 2),
				NSLayoutConstraint.Create (addBorder, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 22),
				NSLayoutConstraint.Create (addBorder, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (addBorder, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this.typeSelector, NSLayoutAttribute.Right, 1, 1.5f),

				NSLayoutConstraint.Create (removeBorder, NSLayoutAttribute.Left, NSLayoutRelation.Equal, addBorder, NSLayoutAttribute.Right, 1, -1),
				NSLayoutConstraint.Create (removeBorder, NSLayoutAttribute.Height, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.Height, 1, 2),
				NSLayoutConstraint.Create (removeBorder, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, controlsBorder, NSLayoutAttribute.CenterY, 1, 0),
				NSLayoutConstraint.Create (removeBorder, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 22),
			});

			AppearanceChanged ();
		}

		public override void OnViewModelChanged (CollectionPropertyViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			if (oldModel != null) {
				oldModel.TypeRequested -= OnTypeRequested; 
				oldModel.AddTargetCommand.CanExecuteChanged -= OnCanAddTargetChanged;
				oldModel.RemoveTargetCommand.CanExecuteChanged -= OnCanRemoveTargetChanged;

				if (this.suggestedTypes != null) {
					this.suggestedTypes.CollectionChanged -= OnSuggestedTypesChanged;
					this.suggestedTypes = null;
				}

				if (this.targets != null) {
					this.targets.CollectionChanged -= OnTargetsChanged;
					this.targets = null;
				}

				this.collectionView.Source = null;
				this.propertyList.ViewModel = null;
			}

			if (ViewModel != null) {
				ViewModel.TypeRequested += OnTypeRequested;
				ViewModel.AddTargetCommand.CanExecuteChanged += OnCanAddTargetChanged;
				ViewModel.RemoveTargetCommand.CanExecuteChanged += OnCanRemoveTargetChanged;

				this.collectionView.Source = new CollectionTableSource (this.hostResources, ViewModel);
				this.propertyList.ViewModel = ViewModel.Panel;

				OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (null));
				OnCanAddTargetChanged (this, EventArgs.Empty);
				OnCanRemoveTargetChanged (this, EventArgs.Empty);
			}
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged (sender, e);

			if (e.PropertyName == nameof(CollectionPropertyViewModel.SuggestedTypes) || String.IsNullOrEmpty (e.PropertyName)) {
				if (this.suggestedTypes != null) {
					this.suggestedTypes.CollectionChanged -= OnSuggestedTypesChanged;
				}

				this.suggestedTypes = ViewModel.SuggestedTypes as INotifyCollectionChanged;
				if (this.suggestedTypes != null)
					this.suggestedTypes.CollectionChanged += OnSuggestedTypesChanged;

				OnSuggestedTypesChanged (ViewModel, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			}

			if (e.PropertyName == nameof(CollectionPropertyViewModel.SelectedType) || String.IsNullOrEmpty (e.PropertyName)) {
				if (ViewModel.SelectedType != null)
					this.typeSelector.SelectItem (ViewModel.SuggestedTypes.IndexOf (ViewModel.SelectedType));
				else if (this.typeSelector.SelectedIndex >= 0)
					this.typeSelector.DeselectItem (this.typeSelector.SelectedIndex);
			}

			if (e.PropertyName == nameof (CollectionPropertyViewModel.Targets) || String.IsNullOrEmpty (e.PropertyName)) {
				if (this.targets != null)
					this.targets.CollectionChanged -= OnTargetsChanged;

				this.targets = ViewModel.Targets as INotifyCollectionChanged;
				if (this.targets != null)
					this.targets.CollectionChanged += OnTargetsChanged;

				OnTargetsChanged (ViewModel, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			}

			if (e.PropertyName == nameof (CollectionPropertyViewModel.SelectedTarget) || String.IsNullOrEmpty (e.PropertyName)) {
				if (ViewModel.SelectedTarget == null)
					this.collectionView.DeselectAll (this);
				else
					this.collectionView.SelectRow (ViewModel.Targets.IndexOf (ViewModel.SelectedTarget), byExtendingSelection: false);
			}
		}

		protected override void AppearanceChanged ()
		{
			base.AppearanceChanged ();

			this.add.Image = this.hostResources.GetNamedImage ("pe-list-add-16");
			this.remove.Image = this.hostResources.GetNamedImage ("pe-list-remove-16");
			this.add.Cell.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.FrameBoxButtonBackgroundColor);
			this.remove.Cell.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.FrameBoxButtonBackgroundColor);
		}

		private readonly IHostResourceProvider hostResources;
		private readonly NSTableView collectionView;
		private readonly NSComboBox typeSelector;
		private readonly NSButton add, remove;
		private readonly PropertyList propertyList;
		private INotifyCollectionChanged suggestedTypes, targets;

		private class CollectionTableSource
			: NSTableViewSource
		{
			public CollectionTableSource (IHostResourceProvider hostResources, CollectionPropertyViewModel vm)
			{
				this.viewModel = vm;
				this.hostResources = hostResources;
			}

			public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
			{
				var view = (CollectionItemView)tableView.MakeView (CollectionItemId, tableView);
				if (view == null)
					view = new CollectionItemView (this.viewModel.TargetPlatform.IconProvider, this.hostResources);

				view.ViewModel = this.viewModel.Targets[(int)row];
				return view;
			}

			public override nint GetRowCount (NSTableView tableView)
			{
				return this.viewModel.Targets.Count;
			}

			public override void SelectionDidChange (NSNotification notification)
			{
				NSTableView table = (NSTableView)notification.Object;
				this.viewModel.SelectedTarget = (table.SelectedRow != -1) ? this.viewModel.Targets[(int)table.SelectedRow] : null;
			}

			public override bool WriteRows (NSTableView tableView, NSIndexSet rowIndexes, NSPasteboard pboard)
			{
				var item = new NSPasteboardItem ();
				item.SetDataForType (NSKeyedArchiver.GetArchivedData (rowIndexes), DataTypeName);
				pboard.WriteObjects (new[] { item });
				return true;
			}

			[Export ("tableView:validateDrop:proposedRow:proposedDropOperation:")]
			public NSDragOperation ValidateDrop (NSTableView tableView, INSDraggingInfo info, nint row, NSTableViewDropOperation dropOperation)
			{
				if (info.DraggingPasteboard.GetDataForType (DataTypeName) != null)
					return NSDragOperation.Move;

				return NSDragOperation.None;
			}

			[Export ("tableView:acceptDrop:row:dropOperation:")]
			public bool AcceptDrop (NSTableView tableView, INSDraggingInfo info, nint row, NSTableViewDropOperation dropOperation)
			{
				NSData data = info.DraggingPasteboard.GetDataForType (DataTypeName);
				NSIndexSet indexes = NSKeyedUnarchiver.UnarchiveObject (data) as NSIndexSet;
				if (indexes == null)
					return false;

				// Dropping at the bottom gives a row at the count, but we shift indexes first
				if (row >= this.viewModel.Targets.Count)
					row = this.viewModel.Targets.Count - 1;

				this.viewModel.MoveTarget ((int)indexes.FirstIndex, (int)row);
				return true;
			}

			private const string DataTypeName = "public.data";

			private class CollectionItemView
				: NSView
			{
				public CollectionItemView (IIconProvider iconProvider, IHostResourceProvider hostResources)
				{
					Identifier = CollectionItemId;
					this.iconProvider = iconProvider;
					this.hostResources = hostResources;

					AddSubview (this.label);
					if (iconProvider != null) {
						AddSubview (this.iconView);
						AddConstraints (new[]{
							NSLayoutConstraint.Create (this.iconView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 7),
							NSLayoutConstraint.Create (this.iconView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
							NSLayoutConstraint.Create (this.iconView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 16),
							NSLayoutConstraint.Create (this.iconView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 16),

							NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this.iconView, NSLayoutAttribute.Trailing, 1, 0),
						});
					} else {
						AddConstraint (NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, this, NSLayoutAttribute.Leading, 1, 7));
					}

					AddConstraints (new[] {
						NSLayoutConstraint.Create (this.label, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterY, 1, 0),
						NSLayoutConstraint.Create (this.label, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1, 0)
					});

					AppearanceChanged ();
				}

				public CollectionPropertyItemViewModel ViewModel
				{
					get { return this.vm; }
					set
					{
						this.vm = value;

						if (value != null) {
							this.label.StringValue = value.TypeName;
							UpdateIcon ();
						}
					}
				}

				public sealed override void ViewDidChangeEffectiveAppearance ()
				{
					base.ViewDidChangeEffectiveAppearance ();

					AppearanceChanged ();
				}

				private void AppearanceChanged ()
				{
					UpdateIcon ();
				}

				private readonly IIconProvider iconProvider;
				private readonly IHostResourceProvider hostResources;
				private readonly NSImageView iconView = new NSImageView {
					ImageScaling = NSImageScale.ProportionallyDown,
					TranslatesAutoresizingMaskIntoConstraints = false
				};

				private readonly UnfocusableTextField label = new UnfocusableTextField {
					TranslatesAutoresizingMaskIntoConstraints = false
				};

				private CollectionPropertyItemViewModel vm;

				private void UpdateIcon()
				{
					if (this.iconProvider == null)
						return;

					var image = this.hostResources.GetNamedImage ("property-generic-item-16");
					image.Template = true;
					this.iconView.Image = image;

					if (this.vm == null)
						return;
				}
			}

			private const string CollectionItemId = "CollectionItem";
			private readonly CollectionPropertyViewModel viewModel;
			private readonly IHostResourceProvider hostResources;
		}

		private void OnSelectedTypeChanged (object sender, EventArgs e)
		{
			ITypeInfo type = null;
			int index = (int)this.typeSelector.SelectedIndex;
			if (index >= 0)
				type = ViewModel.SuggestedTypes[index];

			ViewModel.SelectedType = type;
		}

		private void OnTypeRequested (object sender, TypeRequestedEventArgs e)
		{
			e.SelectedType = e.RequestAt (this.hostResources, this.typeSelector, ViewModel.AssignableTypes);
		}

		private void OnAddChild (object sender, EventArgs e)
		{
			ViewModel.AddTargetCommand.Execute (null);
		}

		private void OnRemoveChild (object sender, EventArgs e)
		{
			ViewModel.RemoveTargetCommand.Execute (null);
		}

		private void OnCanAddTargetChanged (object sender, EventArgs e)
		{
			this.add.Enabled = ViewModel.AddTargetCommand.CanExecute (null);
		}

		private void OnCanRemoveTargetChanged (object sender, EventArgs e)
		{
			this.remove.Enabled = ViewModel.RemoveTargetCommand.CanExecute (null);
		}

		private void OnTargetsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Reset:
			default:
				this.collectionView.ReloadData ();
				break;
			}
		}

		private void OnSuggestedTypesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.typeSelector == null)
				return;

			this.typeSelector.RemoveAll ();

			foreach (ITypeInfo type in ViewModel?.SuggestedTypes) {
				this.typeSelector.Add (new NSString (type.Name));
			}
		}
	}
}
