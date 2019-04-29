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
		public GroupEditorControl (IHostResourceProvider hostResources)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			this.container = new NSStackView (Bounds) {
				Alignment = NSLayoutAttribute.CenterX,
				AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable
			};

			this.table = new GroupedTableView {
				RowHeight = 24,
				IntercellSpacing = new CGSize (0, 0)
			};
			this.table.AddColumn (new NSTableColumn ());
			this.container.AddView (this.table, NSStackViewGravity.Top);

			this.host = new NSBox {
				BoxType = NSBoxType.NSBoxCustom,
				BorderWidth = 0,
				TranslatesAutoresizingMaskIntoConstraints = false,
				ContentViewMargins = new CGSize (0, 0)
			};

			this.container.AddView (this.host, NSStackViewGravity.Top);
			this.container.AddConstraint (NSLayoutConstraint.Create (this.host, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this.container, NSLayoutAttribute.Width, 1, 0));

			AddSubview (this.container);

			AppearanceChanged ();
		}

		NSView INativeContainer.NativeView => this;

		EditorViewModel IEditorView.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = (PropertyGroupViewModel)value; }
		}

		public bool IsDynamicallySized => true;

		public bool NeedsPropertyButton => false;

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
				view = this.selector.GetEditor (this.hostResources, gvm.Properties[0]);
			}

			nint editorHeight = view.GetHeight (gvm.Properties[0]);
			return ((nint)this.table.RowHeight * gvm.Properties.Count) + editorHeight;
		}

		public override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		private void AppearanceChanged ()
		{
			this.table.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.PadBackgroundColor);
			this.host.FillColor = this.hostResources.GetNamedColor (NamedResources.ValueBlockBackgroundColor);
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

		private readonly NSTableView table;
		private readonly NSStackView container;
		private readonly NSBox host;
		private readonly PropertyGroupedEditorSelector selector = new PropertyGroupedEditorSelector ();
		private readonly IHostResourceProvider hostResources;

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
				PropertyViewModel pvm = ViewModel.Properties[(int)row];

				string identifier = pvm.GetType ().FullName;
				var view = tableView.MakeView (identifier, tableView) as PreviewView;
				if (view == null) {
					IValueView valueView = this.selector.CreateView (this.host.hostResources, pvm.Property.Type);
					if (valueView == null)
						return new NSView ();

					view = new PreviewView (this.host.hostResources, valueView) {
						Identifier = identifier
					};
				}

				view.Label = pvm.Name;
				view.ViewModel = pvm;
				return view;
			}

			public override void SelectionDidChange (NSNotification notification)
			{
				this.host.UpdateHosted ();
			}

			private readonly GroupEditorControl host;
			private readonly PropertyInlinePreviewSelector selector = new PropertyInlinePreviewSelector ();
		}

		private class PreviewView
			: PropertyContainer
		{
			public PreviewView (IHostResourceProvider hostResources, IValueView valueView)
				: base (hostResources, valueView, includePropertyButton: true, vertInset: -6f)
			{
				this.view = valueView;
			}

			public PropertyViewModel ViewModel
			{
				get { return this.vm; }
				set
				{
					if (this.vm == value)
						return;

					if (this.vm != null)
						this.vm.PropertyChanged -= OnPropertyChanged;

					this.vm = value;
					if (this.vm != null) {
						this.view.SetValue (((IPropertyValue)this.vm).Value);
						this.vm.PropertyChanged += OnPropertyChanged;
					}

					PropertyButton.ViewModel = value;
				}
			}

			private PropertyViewModel vm;
			private IValueView view;

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
				this.hostedEditor = this.selector.GetEditor (this.hostResources, pvm);
				this.host.ContentView = this.hostedEditor.NativeView;
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
