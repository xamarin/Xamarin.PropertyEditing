using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BindingTypeSelectorControl : NSView
	{
		internal class TypeOutlineView : NSOutlineView
		{
			private TypeSelectorViewModel viewModel;
			public TypeSelectorViewModel ViewModel {
				get => this.viewModel;
				set {
					if (this.viewModel != null) {
						this.viewModel.PropertyChanged -= OnPropertyChanged;
					}

					if (this.viewModel != value) {
						this.viewModel = value;
						var dataSource = new TypeOutlineViewDataSource (this.viewModel);
						Delegate = new TypeOutlineViewDelegate (dataSource);
						DataSource = dataSource;
					}

					OnPropertyChanged (this.viewModel, new PropertyChangedEventArgs (null));
					if (this.viewModel != null) {
						this.viewModel.PropertyChanged += OnPropertyChanged;
					}
				}
			}

			private void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
			{
				ReloadData ();

				ExpandItem (null, true);
			}

			public TypeOutlineView ()
			{
				Initialize ();
			}

			// Called when created from unmanaged code
			public TypeOutlineView (IntPtr handle) : base (handle)
			{
				Initialize ();
			}

			// Called when created directly from a XIB file
			[Export ("initWithCoder:")]
			public TypeOutlineView (NSCoder coder) : base (coder)
			{
				Initialize ();
			}

			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
			{
				return true;
			}

			public void Initialize ()
			{
				AutoresizingMask = NSViewResizingMask.WidthSizable;
				HeaderView = null;
				TranslatesAutoresizingMaskIntoConstraints = false;
			}
		}

		internal class TypeOutlineViewDelegate : NSOutlineViewDelegate
		{
			private TypeOutlineViewDataSource dataSource;

			public TypeOutlineViewDelegate (TypeOutlineViewDataSource dataSource)
			{
				this.dataSource = dataSource;
			}

			public override nfloat GetRowHeight (NSOutlineView outlineView, NSObject item)
			{
				return PropertyEditorControl.DefaultControlHeight;
			}

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var labelContainer = (UnfocusableTextField)outlineView.MakeView ("type", this);
				if (labelContainer == null) {
					labelContainer = new UnfocusableTextField {
						Identifier = "type",
					};
				}
				var target = (item as NSObjectFacade).Target;

				switch (target) {
					case KeyValuePair<string, SimpleCollectionView> kvp:
						labelContainer.StringValue = kvp.Key;
						break;
					case TypeInfo info:
						labelContainer.StringValue = info.Name;
						break;
					default:
						labelContainer.StringValue = "Type Not Supported";
						break;
				}

				return labelContainer;
			}

			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
					case KeyValuePair<string, SimpleCollectionView> kvp:
						return false;
					case TypeInfo info:
						return true;

					default:
						return false;
				}
			}
		}

		internal class TypeOutlineViewDataSource : NSOutlineViewDataSource
		{
			public TypeSelectorViewModel ViewModel { get; }

			internal TypeOutlineViewDataSource (TypeSelectorViewModel viewModel)
			{
				if (viewModel == null)
					throw new ArgumentNullException (nameof (viewModel));

				ViewModel = viewModel;
			}

			public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
			{
				var childCount = 0;
				if (item == null) {
					childCount = this.ViewModel.Types != null ? this.ViewModel.Types.Count () : 0;
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
						case KeyValuePair<string, SimpleCollectionView> kvp:
							childCount = kvp.Value.Count;
							break;
						case TypeInfo info:
							childCount = 0;
							break;
						default:
							childCount = 0;
							break;
					}
				}

				return childCount;
			}

			public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
			{
				object element;

				if (item == null) {
					element = this.ViewModel.Types.ElementAt ((int)childIndex);
				} else {
					var target = (item as NSObjectFacade).Target;
					switch (target) {
						case KeyValuePair<string, SimpleCollectionView> kvp:
							element = kvp.Value[(int)childIndex];
							break;
						case TypeInfo info:
							element = info;
							break;
						default:
							return null;
					}
				}

				return new NSObjectFacade (element);
			}

			public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
			{
				var target = (item as NSObjectFacade).Target;
				switch (target) {
					case KeyValuePair<string, SimpleCollectionView> kvp:
						return kvp.Value.Count > 0;
					case TypeInfo info:
						return false;
					default:
						return false;
				}
			}
		}
		internal const string TypeSelectorColId = "TypeSelectorColumn";

		internal TypeOutlineView typeOutlineView;
		private NSButton showAllAssembliesCheckBox;

		public CreateBindingViewModel ViewModel { get; set; }

		public BindingTypeSelectorControl (CreateBindingViewModel viewModel)
		{
			ViewModel = viewModel;
			TranslatesAutoresizingMaskIntoConstraints = false;

			this.typeOutlineView = new TypeOutlineView {

			};

			this.typeOutlineView.Activated += (sender, e) => {
				if (sender is TypeOutlineView tov) {
					if (tov.SelectedRow != -1) {
						if (tov.ItemAtRow (tov.SelectedRow) is NSObjectFacade item) {
							if (item.Target is ITypeInfo typeInfo) {
								viewModel.TypeSelector.SelectedType = typeInfo;
							}
						}
					}
				}
			};

			var typeColumn = new NSTableColumn (TypeSelectorColId);
			this.typeOutlineView.AddColumn (typeColumn);

			// Set OutlineTableColumn or the arrows showing children/expansion will not be drawn
			this.typeOutlineView.OutlineTableColumn = typeColumn;

			// create a table view and a scroll view
			var tableContainer = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			// add the panel to the window
			tableContainer.DocumentView = this.typeOutlineView;
			AddSubview (tableContainer);

			var filterObjects = new NSSearchField {
				ControlSize = NSControlSize.Mini,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				PlaceholderString = Properties.Resources.SearchObjectsTitle,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			filterObjects.Changed += (sender, e) => {
				viewModel.TypeSelector.FilterText = filterObjects.Cell.Title;
				this.typeOutlineView.ReloadData ();
				this.typeOutlineView.ExpandItem (null, true);
			};

			AddSubview (filterObjects);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (filterObjects, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 45f),
				NSLayoutConstraint.Create (filterObjects, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (filterObjects, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (filterObjects, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			// Position based on filterObjects bottom and All Assemblies Top.
			AddConstraints (new[] {
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, filterObjects, NSLayoutAttribute.Bottom, 1f, 5f),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 5f),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (tableContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, this, NSLayoutAttribute.Height, 1f, -105f),
			});

			this.showAllAssembliesCheckBox = new NSButton {
				ControlSize = NSControlSize.Small,
				Font = NSFont.FromFontName (PropertyEditorControl.DefaultFontName, PropertyEditorControl.DefaultFontSize),
				Title = Properties.Resources.ShowAllAssemblies,
				TranslatesAutoresizingMaskIntoConstraints = false,
			};

			this.showAllAssembliesCheckBox.SetButtonType (NSButtonType.Switch);
			this.showAllAssembliesCheckBox.Activated += SelectionChanged;

			AddSubview (this.showAllAssembliesCheckBox);
			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.showAllAssembliesCheckBox, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, -30f),
				NSLayoutConstraint.Create (this.showAllAssembliesCheckBox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 10f),
				NSLayoutConstraint.Create (this.showAllAssembliesCheckBox, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, -10f),
				NSLayoutConstraint.Create (this.showAllAssembliesCheckBox, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1f, 24),
			});

			viewModel.PropertyChanged += (sender, e) => {
				if (e.PropertyName == nameof (CreateBindingViewModel.ShowTypeSelector)) {
					Hidden = !viewModel.ShowTypeSelector;

					if (viewModel.ShowTypeSelector && viewModel.TypeSelector != null) {
						this.typeOutlineView.ViewModel = viewModel.TypeSelector;

						this.showAllAssembliesCheckBox.State = viewModel.TypeSelector.ShowAllAssemblies ? NSCellStateValue.On : NSCellStateValue.Off;
					}
				}
			};
		}

		private void SelectionChanged (object sender, EventArgs e)
		{
			if (sender is NSButton button) {
				this.typeOutlineView.ViewModel.ShowAllAssemblies = (button.State == NSCellStateValue.On) ? true : false;
			}
		}
	}
}
