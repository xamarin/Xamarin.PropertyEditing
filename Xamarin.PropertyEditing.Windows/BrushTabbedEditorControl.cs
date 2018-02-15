using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xamarin.PropertyEditing.Drawing;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	public class BrushTabbedEditorControl
		: Control
	{
		public BrushTabbedEditorControl ()
		{
			DefaultStyleKey = typeof (BrushTabbedEditorControl);
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.brushChoice = GetTemplateChild ("brushChoice") as ChoiceControl;
			this.advancedPropertyPanel = GetTemplateChild ("advancedPropertyPanel") as Expander;
			this.solidBrushEditor = GetTemplateChild ("solidBrushEditor") as SolidBrushEditorControl;
			this.materialDesignColorEditor = GetTemplateChild ("materialDesignColorEditor") as MaterialDesignColorEditorControl;
			this.resourceBrushEditor = GetTemplateChild ("resourceBrushEditor") as ResourceBrushEditorControl;

			if (this.brushChoice == null)
				throw new InvalidOperationException ($"{nameof (BrushTabbedEditorControl)} is missing a child ChoiceControl named \"brushChoice\"");
			if (this.advancedPropertyPanel == null)
				throw new InvalidOperationException ($"{nameof (BrushTabbedEditorControl)} is missing a child Expander named \"advancedPropertyPanel\"");
			if (this.solidBrushEditor == null)
				throw new InvalidOperationException ($"{nameof (BrushTabbedEditorControl)} is missing a child SolidBrushEditorControl named \"solidBrushEditor\"");
			if (this.materialDesignColorEditor == null)
				throw new InvalidOperationException ($"{nameof (BrushTabbedEditorControl)} is missing a child MaterialDesignColorEditorControl named \"materialDesignColorEditor\"");
			if (this.resourceBrushEditor == null)
				throw new InvalidOperationException ($"{nameof (BrushTabbedEditorControl)} is missing a child ResourceBrushEditorControl named \"resourceBrushEditor\"");

			StorePreviousBrush ();
			SelectTabFromBrush ();

			if (ViewModel.MaterialDesign == null) {
				this.brushChoice.Items.Filter = o => ((ChoiceItem)o).Name != "materialDesignTab";
			}

			this.brushChoice.SelectedItemChanged += (s, e) => {
				if (ViewModel == null) return;
				StorePreviousBrush ();
				switch ((string)((ChoiceItem)(this.brushChoice.SelectedItem)).Value) {
				case none:
					if (ViewModel.Value != null) ViewModel.Value = null;
					break;
				case solid:
					ViewModel.Value = ViewModel.Solid?.PreviousSolidBrush ?? new CommonSolidBrush (new CommonColor (0, 0, 0));
					ViewModel.Solid.CommitLastColor ();
					ViewModel.Solid.CommitHue ();
					break;
				case resource:
					break;
				case materialDesign:
					ViewModel.Value = ViewModel.Solid?.PreviousSolidBrush ?? new CommonSolidBrush (new CommonColor (0, 0, 0));
					break;
				}
				ShowSelectedTab ();
			};

			this.brushChoice.KeyUp += (s, e) => {
				if (ViewModel == null) return;
				StorePreviousBrush ();
				switch (e.Key) {
				case Key.N:
					e.Handled = true;
					this.brushChoice.SelectedValue = none;
					ShowSelectedTab ();
					break;
				case Key.S:
					e.Handled = true;
					this.brushChoice.SelectedValue = solid;
					ShowSelectedTab ();
					break;
				case Key.R:
					e.Handled = true;
					this.brushChoice.SelectedValue = resource;
					ShowSelectedTab ();
					break;
				case Key.M:
					e.Handled = true;
					this.brushChoice.SelectedValue = materialDesign;
					ShowSelectedTab ();
					break;
					// TODO: add G, T, etc. for the other brush types when they are available.
				}
			};
		}

		public static readonly string None = none;
		public static readonly string Solid = solid;
		public static readonly string Resource = resource;
		public static readonly string MaterialDesign = materialDesign;

		internal void FocusFirstChild ()
		{
			this.brushChoice?.FocusSelectedItem ();
		}

		private const string none = nameof (none);
		private const string solid = nameof (solid);
		private const string resource = nameof (resource);
		private const string materialDesign = nameof (materialDesign);

		private ChoiceControl brushChoice;
		private Expander advancedPropertyPanel;
		private SolidBrushEditorControl solidBrushEditor;
		private ResourceBrushEditorControl resourceBrushEditor;
		private MaterialDesignColorEditorControl materialDesignColorEditor;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;

		private void StorePreviousBrush ()
		{
			if (ViewModel == null) return;
			if (ViewModel.Value is CommonSolidBrush solidBrush) {
				ViewModel.Solid.PreviousSolidBrush = solidBrush;
			}
		}

		internal void SelectTabFromBrush ()
		{
			if (ViewModel != null && ViewModel.MaterialDesign != null
				&& (ViewModel.MaterialDesign.NormalColor.HasValue || ViewModel.MaterialDesign.AccentColor.HasValue)) {
				this.brushChoice.SelectedValue = materialDesign;
				ShowSelectedTab ();
				return;
			}
			switch (ViewModel?.Value) {
			case null:
				this.brushChoice.SelectedValue = none;
				ShowSelectedTab ();
				break;
			case CommonSolidBrush _:
				switch (ViewModel.ValueSource) {
				case ValueSource.Local:
					this.brushChoice.SelectedValue = solid;
					break;
				case ValueSource.Resource:
					this.brushChoice.SelectedValue = resource;
					break;
				default:
					this.brushChoice.SelectedValue = solid;
					break;
				}
				ShowSelectedTab ();
				break;
			}
		}

		private void ShowSelectedTab()
		{
			switch ((string)((ChoiceItem)(this.brushChoice.SelectedItem)).Value) {
			case none:
				this.advancedPropertyPanel.Visibility = Visibility.Collapsed;
				this.solidBrushEditor.Visibility = Visibility.Collapsed;
				this.materialDesignColorEditor.Visibility = Visibility.Collapsed;
				this.resourceBrushEditor.Visibility = Visibility.Collapsed;
				break;
			case solid:
				this.advancedPropertyPanel.Visibility = Visibility.Visible;
				this.solidBrushEditor.Visibility = Visibility.Visible;
				this.materialDesignColorEditor.Visibility = Visibility.Collapsed;
				this.resourceBrushEditor.Visibility = Visibility.Collapsed;
				break;
			case materialDesign:
				this.advancedPropertyPanel.Visibility = Visibility.Visible;
				this.solidBrushEditor.Visibility = Visibility.Collapsed;
				this.materialDesignColorEditor.Visibility = Visibility.Visible;
				this.resourceBrushEditor.Visibility = Visibility.Collapsed;
				break;
			case resource:
				this.advancedPropertyPanel.Visibility = Visibility.Collapsed;
				this.solidBrushEditor.Visibility = Visibility.Collapsed;
				this.materialDesignColorEditor.Visibility = Visibility.Collapsed;
				this.resourceBrushEditor.Visibility = Visibility.Visible;
				break;
			}
		}
	}
}
