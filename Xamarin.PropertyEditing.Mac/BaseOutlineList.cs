using System;
using AppKit;
using CoreGraphics;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Mac
{
	internal abstract class BaseOutlineList
		: NSView
	{
		private IHostResourceProvider hostResources;
		private readonly NSOutlineView outlineViewTable;
		private readonly NSScrollView scrollView;
		protected PanelViewModel viewModel;
		public NSOutlineView OutlineViewTable => this.outlineViewTable;
		protected BaseOutlineViewDataSource dataSource;

		public IHostResourceProvider HostResourceProvider
		{
			get => this.hostResources;
			set {
				if (this.hostResources == value)
					return;
				if (value == null)
					throw new ArgumentNullException (nameof (value), "Cannot set HostResourceProvider to null");

				this.hostResources = value;
				UpdateResourceProvider ();
			}
		}

		public virtual PanelViewModel ViewModel {
			get => this.viewModel;
			set {
				this.viewModel = value;
			}
		}

		internal BaseOutlineList (IHostResourceProvider hostResources, string columnID)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			if (columnID == null)
				throw new ArgumentNullException (nameof (columnID));

			this.hostResources = hostResources;

			this.outlineViewTable = new FirstResponderOutlineView {
				IndentationPerLevel = 0,
				SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.None,
				HeaderView = null,
				IntercellSpacing = new CGSize (0, 0)
			};

			var outlineViewColumn = new NSTableColumn (columnID);
			this.outlineViewTable.AddColumn (outlineViewColumn);

			this.scrollView = new NSScrollView {
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
				HasHorizontalScroller = false,
				HasVerticalScroller = true,
			};

			this.scrollView.DocumentView = this.outlineViewTable;
			AddSubview (this.scrollView);
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			UpdateResourceProvider ();
		}

		protected virtual void UpdateResourceProvider ()
		{
			if (this.outlineViewTable == null || this.hostResources == null)
				return;

			this.outlineViewTable.BackgroundColor = this.hostResources.GetNamedColor (NamedResources.PadBackgroundColor);
		}
	}
}