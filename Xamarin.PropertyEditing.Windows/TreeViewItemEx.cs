using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xamarin.PropertyEditing.Windows
{
	internal class TreeViewEx
		: TreeView
	{
		static TreeViewEx()
		{
			setSelectedItem = (Action<TreeView, object>)typeof(TreeView).GetMethod ("SetSelectedItem", BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate (typeof(Action<TreeView, object>));
		}

		public event EventHandler ItemActivated;

		protected override DependencyObject GetContainerForItemOverride ()
		{
			return new TreeViewItemEx();
		}

		protected override bool IsItemItsOwnContainerOverride (object item)
		{
			return item is TreeViewItemEx;
		}

		protected override void OnMouseDoubleClick (MouseButtonEventArgs e)
		{
			var inputElement = InputHitTest (e.GetPosition (this)) as UIElement;
			if (inputElement != null) {
				var item = inputElement.FindParentOrSelf<TreeViewItemEx>();
				if (item != null && item.IsSelectable)
					ItemActivated?.Invoke (this, EventArgs.Empty);
			}

			base.OnMouseDoubleClick (e);
		}

		internal TreeViewItemEx SelectedTreeItem
		{
			get { return this.selectedTreeItem; }
			set
			{
				if (this.selectedTreeItem != null) {
					if (this.selectedTreeItem != value)
						this.selectedTreeItem.IsSelected = false;
					else if (this.selectedTreeItem.IsSelected && SelectedItem == this.selectedTreeItem.DataContext)
						return;
				}

				// This does break the selected value path features, but we don't care about those
				object oldItem = this.selectedTreeItem?.DataContext;
				this.selectedTreeItem = value;
				if (value != null) {
					value.IsSelected = true;
				}

				SetSelectedItem (oldItem, value?.DataContext);
			}
		}

		private TreeViewItemEx selectedTreeItem;

		private void SetSelectedItem (object oldItem, object item)
		{
			Dispatcher.InvokeAsync (() => {
				setSelectedItem (this, item);

				var e = new RoutedPropertyChangedEventArgs<object> (oldItem, item, SelectedItemChangedEvent);
				RaiseEvent (e);
			});
		}

		private static readonly Action<TreeView, object> setSelectedItem;
	}

	internal class TreeViewItemEx
		: TreeViewItem
	{
		public TreeViewItemEx()
		{
		}

		private static readonly DependencyPropertyKey IndentLevelKey = DependencyProperty.RegisterReadOnly ("IndentLevel", typeof(int), typeof(TreeViewItemEx), new PropertyMetadata());
		public static readonly DependencyProperty IndentLevelProperty = IndentLevelKey.DependencyProperty;

		public int IndentLevel
		{
			get { return (int)GetValue (IndentLevelProperty); }
			private set { SetValue (IndentLevelKey, value); }
		}

		public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register ("ChildMargin", typeof(Thickness), typeof(TreeViewItemEx), new PropertyMetadata());
		public Thickness ChildMargin
		{
			get { return (Thickness)GetValue (ChildMarginProperty); }
			set { SetValue (ChildMarginProperty, value); }
		}

		private static readonly DependencyPropertyKey IsMouseOverItemKey = DependencyProperty.RegisterReadOnly ("IsMouseOverItem", typeof(bool), typeof(TreeViewItemEx), new PropertyMetadata());
		public static readonly DependencyProperty IsMouseOverItemProperty = IsMouseOverItemKey.DependencyProperty;

		public bool IsMouseOverItem
		{
			get { return (bool)GetValue (IsMouseOverItemProperty); }
			private set { SetValue (IsMouseOverItemKey, value); }
		}

		public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register ("IsSelectable", typeof(bool), typeof(TreeViewItemEx), new PropertyMetadata (true));

		public bool IsSelectable
		{
			get { return (bool)GetValue (IsSelectableProperty); }
			set { SetValue (IsSelectableProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			int level = 0;
			var parent = VisualTreeHelper.GetParent (this);
			while (!(parent is TreeView)) {
				parent = VisualTreeHelper.GetParent (parent);

				if (parent is TreeViewItem)
					level++;
			}

			this.topParent = parent as TreeViewEx;

			IndentLevel = level;
		}

		protected override void OnMouseDown (MouseButtonEventArgs e)
		{
			if (IsSelectable) {
				if (Keyboard.IsKeyDown (Key.LeftCtrl) || Keyboard.IsKeyDown (Key.RightCtrl)) {
					IsSelected = !IsSelected;
					e.Handled = true;
				}
			}

			base.OnMouseDown (e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Up || e.Key == Key.Down) {
				var next = GetNextElement (e.Key == Key.Up) as ItemsControl;
				if (!(next is TreeView)) {
					if (next.HasItems) {
						var tree = GetParentTree();
						tree.SelectedTreeItem = null;
					}

					next.Focus();
					e.Handled = true;
				}
			}

			base.OnKeyDown (e);
		}

		protected override void OnSelected (RoutedEventArgs e)
		{
			TreeViewEx parent = GetParentTree();
			if (!IsSelectable) {
				IsSelected = false;
				e.Handled = true;

				parent.SelectedTreeItem = parent.SelectedTreeItem;
			} else if (parent.SelectedTreeItem != this) {
				parent.SelectedTreeItem = this;
			}
		}

		protected override DependencyObject GetContainerForItemOverride ()
		{
			return new TreeViewItemEx();
		}

		protected override bool IsItemItsOwnContainerOverride (object item)
		{
			return item is TreeViewItemEx;
		}

		protected override void OnMouseEnter (MouseEventArgs e)
		{
			base.OnMouseEnter (e);

			UpdateMouseOver (e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			UpdateMouseOver (e);
		}

		protected override void OnMouseLeave (MouseEventArgs e)
		{
			base.OnMouseLeave (e);
			IsMouseOverItem = false;
		}

		private TreeViewEx topParent;

		private UIElement GetNextElement (bool up)
		{
			if (!up && HasItems && IsExpanded)
				return (UIElement)ItemContainerGenerator.ContainerFromIndex (0);

			var parentContainer = this.FindParent<ItemsControl>();
			if (parentContainer == null)
				return null;

			int index = parentContainer.ItemContainerGenerator.IndexFromContainer (this);
			index += (up) ? -1 : 1;

			if (index == -1)
				return parentContainer;

			while (index == parentContainer.Items.Count) {
				if (parentContainer is TreeView)
					return parentContainer;
				
				var parentsParent = parentContainer.FindParent<ItemsControl>();
				index = parentsParent.ItemContainerGenerator.IndexFromContainer (parentContainer) + 1;

				parentContainer = parentsParent;
			}

			var item = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromIndex (index);
			if (!up || (!item.IsExpanded || !item.HasItems))
				return item;

			return (UIElement)item.ItemContainerGenerator.ContainerFromIndex (item.Items.Count - 1);
		}

		private TreeViewEx GetParentTree()
		{
			if (this.topParent == null)
				this.topParent = this.FindParent<TreeViewEx>();

			return this.topParent;
		}

		private void UpdateMouseOver (MouseEventArgs e)
		{
			UIElement element = InputHitTest (e.GetPosition (this)) as UIElement;
			if (element == null) {
				IsMouseOverItem = false;
				return;
			}

			IsMouseOverItem = element.FindParentOrSelf<TreeViewItemEx>() == this;
		}
	}
}
