using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using AppKit;
using Foundation;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class TypeSelectorControl
		: NotifyingView<TypeSelectorViewModel>
	{
		public TypeSelectorControl()
		{
			this.checkbox = NSButton.CreateCheckbox (Properties.Resources.ShowAllAssemblies, OnCheckedChanged);
			this.checkbox.TranslatesAutoresizingMaskIntoConstraints = false;
			AddSubview (this.checkbox);

			var scroll = new NSScrollView {
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			var d = new TypeSelectorDelegate ();
			d.SelectionChanged += OnSelectionChanged;
			this.outlineView = new NSOutlineView  {
				Delegate = d,
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				HeaderView = null
			};
			var col = new NSTableColumn ();
			this.outlineView.AddColumn (col);
			this.outlineView.OutlineTableColumn = col;
			scroll.DocumentView = this.outlineView;

			AddSubview (scroll);

			this.filter = new NSTextField {
				TranslatesAutoresizingMaskIntoConstraints = false,
				PlaceholderString = "Filter"
			};
			this.filter.Changed += OnFilterChanged;

			AddSubview (this.filter);

			AddConstraints (new[] {
				NSLayoutConstraint.Create (this.filter, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1, 10),
				NSLayoutConstraint.Create (this.filter, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1, -20),
				NSLayoutConstraint.Create (this.filter, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),

				NSLayoutConstraint.Create (scroll, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this.filter, NSLayoutAttribute.Bottom, 1, 8),
				NSLayoutConstraint.Create (scroll, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.filter, NSLayoutAttribute.Width, 1, 0),
				NSLayoutConstraint.Create (scroll, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1, 0),
				NSLayoutConstraint.Create (scroll, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this.checkbox, NSLayoutAttribute.Top, 1, -8),

				NSLayoutConstraint.Create (this.checkbox, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1, -10),
				NSLayoutConstraint.Create (this.checkbox, NSLayoutAttribute.Left, NSLayoutRelation.Equal, scroll, NSLayoutAttribute.Left, 1, 0)
			});
		}

		public override void KeyDown (NSEvent theEvent)
		{
			base.KeyDown (theEvent);
		}

		public override void OnViewModelChanged (TypeSelectorViewModel oldModel)
		{
			base.OnViewModelChanged (oldModel);

			this.outlineView.DataSource = new TypeSelectorDataSource (ViewModel);
			OnPropertyChanged (ViewModel, new PropertyChangedEventArgs (null));
		}

		public override void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.OnPropertyChanged (sender, e);

			switch (e.PropertyName) {
			case nameof (TypeSelectorViewModel.FilterText):
				UpdateFilter ();
				break;
			case nameof (TypeSelectorViewModel.Types):
				Reload ();
				break;
			default:
				UpdateFilter ();
				break;
			}
		}

		private readonly NSOutlineView outlineView;
		private readonly NSTextField filter;
		private readonly NSButton checkbox;

		private void Reload()
		{
			this.outlineView.ReloadData ();

			if (!String.IsNullOrWhiteSpace (ViewModel?.FilterText)) {
				for (int i = 0; i < this.outlineView.RowCount; i++) {
					this.outlineView.ExpandItem (this.outlineView.ItemAtRow (i));
				}
			}
		}

		private void OnSelectionChanged (object sender, EventArgs e)
		{
			if (ViewModel == null)
				return;

			if (this.outlineView.SelectedRow >= 0) {
				var facade = (NSObjectFacade)this.outlineView.ItemAtRow (this.outlineView.SelectedRow);
				ViewModel.SelectedType = facade.Target as ITypeInfo;
			} else {
				ViewModel.SelectedType = null;
			}
		}

		private void OnFilterChanged (object sender, EventArgs e)
		{
			if (ViewModel == null)
				return;

			ViewModel.FilterText = this.filter.StringValue;
			Reload ();
		}

		private void UpdateFilter()
		{
			this.filter.StringValue = ViewModel?.FilterText ?? String.Empty;
		}

		private void OnCheckedChanged ()
		{
			ViewModel.ShowAllAssemblies = this.checkbox.State == NSCellStateValue.On;
			Reload ();
		}

		private class TypeSelectorDataSource
			: NSOutlineViewDataSource
		{
			public TypeSelectorDataSource (TypeSelectorViewModel viewModel)
			{
				this.viewModel = viewModel;
			}

			public override nint GetChildrenCount (NSOutlineView outlineView, NSObject item)
			{
				if (item == null) {
					return this.viewModel.Types?.Count ?? 0;
				} else if (((NSObjectFacade)item).Target is KeyValuePair<string, SimpleCollectionView> kvp) {
					return kvp.Value.Count;
				}

				return base.GetChildrenCount (outlineView, item);
			}

			public override NSObject GetChild (NSOutlineView outlineView, nint childIndex, NSObject item)
			{
				if (item == null) {
					return new NSObjectFacade (this.viewModel.Types[(int)childIndex]);
				} else if (((NSObjectFacade)item).Target is KeyValuePair<string, SimpleCollectionView> kvp) {
					return new NSObjectFacade (kvp.Value[(int)childIndex]);
				}

				return base.GetChild (outlineView, childIndex, item);
			}

			public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
			{
				return !(((NSObjectFacade)item).Target is ITypeInfo);
			}

			private TypeSelectorViewModel viewModel;
		}

		private class TypeSelectorDelegate
			: NSOutlineViewDelegate
		{
			public event EventHandler SelectionChanged;

			public override NSView GetView (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
			{
				var label = (UnfocusableTextField)outlineView.MakeView (LabelId, outlineView);
				if (label == null) {
					label = new UnfocusableTextField {
						Identifier = LabelId
					};
				}

				string text = String.Empty;
				var facade = (NSObjectFacade)item;
				if (facade.Target is KeyValuePair<string, SimpleCollectionView> kvp)
					text = kvp.Key;
				else if (facade.Target is ITypeInfo type)
					text = type.Name;

				label.StringValue = text;
				return label;
			}

			public override bool ShouldSelectItem (NSOutlineView outlineView, NSObject item)
			{
				var f = (NSObjectFacade)item;
				return (f.Target is ITypeInfo);
			}

			public override void SelectionDidChange (NSNotification notification)
			{
				SelectionChanged?.Invoke (this, EventArgs.Empty);
			}

			private const string LabelId = "label";
		}
	}
}
