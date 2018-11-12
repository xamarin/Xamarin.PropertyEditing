using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;

using AppKit;
using CoreGraphics;
using Foundation;

using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal class GroupEditorControl
		: NSView, IEditorView
	{
		public GroupEditorControl ()
		{
			this.container = new NSStackView (Bounds) {
				Alignment = NSLayoutAttribute.CenterX,
				AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable
			};

			this.table = new GroupedTableView ();
			this.table.AddColumn (new NSTableColumn (PropertyIdentifier));
			this.table.AddColumn (new NSTableColumn (PreviewIdentifier));
			this.container.AddView (this.table, NSStackViewGravity.Top);

			this.host = new NSView ();
			this.container.AddView (this.host, NSStackViewGravity.Top);

			AddSubview (this.container);
		}

		NSView IEditorView.NativeView => this;

		EditorViewModel IEditorView.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = (PropertyGroupViewModel)value; }
		}

		public bool IsDynamicallySized => true;

		public nint GetHeight (EditorViewModel viewModel)
		{
			if (!(viewModel is PropertyGroupViewModel gvm))
				throw new ArgumentException ("Invalid viewmodel type");

			Type propertyVmType = gvm.Properties[0].GetType ();
			IEditorView view;
			if (propertyVmType == ViewModel?.Properties[0].GetType()) {
				if (this.hostedEditor == null)
					UpdateHosted ();

				view = this.hostedEditor;
			} else {
				view = this.selector.GetEditor (gvm.Properties[0]);
			}

			nint editorHeight = view.GetHeight (gvm.Properties[0]);
			return ((nint)this.table.RowHeight * gvm.Properties.Count) + editorHeight;
		}

		internal PropertyGroupViewModel ViewModel
		{
			get { return this.source?.ViewModel; }
			set {
				if (this.incc != null)
					this.incc.CollectionChanged -= OnCollectionChanged;

				this.source = (value != null) ? new GroupedDataSource (value, this) : null;
				this.table.Source = this.source;
				this.incc = ViewModel as INotifyCollectionChanged;
				if (this.incc != null)
					this.incc.CollectionChanged += OnCollectionChanged;

				UpdateHosted ();
			}
		}

		private const string PropertyIdentifier = "property";
		private const string PreviewIdentifier = "preview";
		private const string ButtonIdentifier = "button";

		private readonly NSTableView table;
		private readonly NSStackView container;
		private readonly NSView host;
		private readonly PropertyGroupedEditorSelector selector = new PropertyGroupedEditorSelector ();

		private IEditorView hostedEditor;
		private GroupedDataSource source;
		private INotifyCollectionChanged incc;

		private class GroupedTableView
			: NSTableView
		{
			public GroupedTableView()
			{
				FocusRingType = NSFocusRingType.None;
			}

			// ValidateProposedFirstResponder is implemented as an extension method so we have to override the hard way (see xamarin-macios/4837)
			[DllImport ("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
			public extern static bool bool_objc_msgSendSuper_IntPtr_IntPtr (IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

			static readonly IntPtr selValidateProposedFirstResponder_ForEvent_Handle = ObjCRuntime.Selector.GetHandle ("validateProposedFirstResponder:forEvent:");


			[Export ("validateProposedFirstResponder:forEvent:")]
			public bool ValidateProposedFirstResponder (NSResponder responder, NSEvent forEvent)
			{
				if (responder is PropertyButton)
					return true;

				bool baseRet = bool_objc_msgSendSuper_IntPtr_IntPtr (this.SuperHandle, selValidateProposedFirstResponder_ForEvent_Handle, responder.Handle, forEvent == null ? IntPtr.Zero : forEvent.Handle);
				return true;
			}
		}

		private class GroupedDataSource
			: NSTableViewSource
		{
			public GroupedDataSource (PropertyGroupViewModel vm, GroupEditorControl host)
			{
				if (vm == null)
					throw new ArgumentNullException (nameof (vm));

				ViewModel = vm;
				this.host = host;
			}

			public PropertyGroupViewModel ViewModel
			{
				get;
			}

			public override nint GetRowCount (NSTableView tableView)
			{
				return ViewModel.Properties.Count;
			}

			public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
			{
				if (!this.resized) {
					var cols = tableView.TableColumns ();
					cols[0].Width = tableView.Bounds.Width / 2;
					cols[1].Width = tableView.Bounds.Width / 2;
					this.resized = true;
				}

				PropertyViewModel pvm = ViewModel.Properties[(int)row];

				switch (tableColumn.Identifier) {
				case PropertyIdentifier:
					return new UnfocusableTextField { StringValue = pvm.Property.Name + ":", Alignment = NSTextAlignment.Right };
				case PreviewIdentifier:
					return GetPreview (tableView, pvm);
				}

				return null;
			}

			public override void SelectionDidChange (NSNotification notification)
			{
				this.host.UpdateHosted ();
			}

			private readonly GroupEditorControl host;
			private readonly PropertyInlinePreviewSelector selector = new PropertyInlinePreviewSelector ();
			private bool resized;

			private NSView GetPreview (NSTableView table, PropertyViewModel pvm)
			{
				string identifier = pvm.GetType().FullName;
				PreviewView view = table.MakeView (identifier, table) as PreviewView;
				if (view == null) {
					IValueView valueView = this.selector.CreateView (pvm.Property.Type);
					if (valueView == null)
						return new NSView ();
						
					view = new PreviewView (valueView, new CGRect (0, 0, table.TableColumns ()[1].Width, table.RowHeight)) {
						Identifier = identifier
					};
				}

				view.ViewModel = pvm;
				return view;
			}
		}

		private class PreviewView
			: NSView
		{
			public PreviewView (IValueView valueView, CGRect frame)
			{
				Frame = frame;

				this.view = valueView;
				valueView.NativeView.Frame = new CGRect (0, 0, frame.Width - PropertyButton.DefaultSize, frame.Height);
				valueView.NativeView.AutoresizingMask = NSViewResizingMask.WidthSizable;
				AddSubview (valueView.NativeView);

				this.propertyButton = new PropertyButton {
					Frame = new CGRect (valueView.NativeView.Frame.Width, 0, PropertyButton.DefaultSize, PropertyButton.DefaultSize),
					AutoresizingMask = NSViewResizingMask.MinXMargin
				};

				AddSubview (this.propertyButton);
			}

			public PropertyViewModel ViewModel
			{
				get { return this.propertyButton.ViewModel; }
				set
				{
					if (this.vm == value)
						return;

					if (this.vm != null)
						this.vm.PropertyChanged -= OnPropertyChanged;

					this.vm = value;
					this.view.SetValue (((IPropertyValue)this.vm).Value);
					this.vm.PropertyChanged += OnPropertyChanged;
					this.propertyButton.ViewModel = value;
				}
			}

			private PropertyViewModel vm;
			private IValueView view;
			private PropertyButton propertyButton;

			private void OnPropertyChanged (object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == nameof (PropertyViewModel<string>.Value)) {
					this.view.SetValue (((IPropertyValue)this.vm).Value);
				}
			}
		}

		private void UpdateHosted()
		{
			nint index = this.table.SelectedRow;
			if (index < 0) {
				index = 0;
				this.table.SelectRow (0, false);
			}

			PropertyViewModel pvm = ViewModel.Properties[(int)index];
			if (this.hostedEditor == null) {
				this.hostedEditor = this.selector.GetEditor (pvm);
				this.host.Frame = this.hostedEditor.NativeView.Bounds;
				this.hostedEditor.NativeView.AutoresizingMask = NSViewResizingMask.WidthSizable;
				this.host.AddSubview (this.hostedEditor.NativeView);
			}

			this.hostedEditor.ViewModel = pvm;
		}

		private void OnCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				if (e.NewStartingIndex < 0 || e.NewItems == null || e.NewItems.Count == 0)
					goto default;

				this.table.InsertRows (NSIndexSet.FromNSRange (new NSRange (e.NewStartingIndex, e.NewItems.Count)), NSTableViewAnimation.SlideDown);
				break;
			case NotifyCollectionChangedAction.Remove:
				if (e.OldStartingIndex < 0 || e.OldItems == null || e.OldItems.Count == 0)
					goto default;

				this.table.RemoveRows (NSIndexSet.FromNSRange (new NSRange (e.OldStartingIndex, e.OldItems.Count)), NSTableViewAnimation.SlideDown);
				break;
			default:
				this.table.ReloadData ();
				break;
			}
		}
	}
}
