using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

			if (ViewModel.MaterialDesign == null) {
				this.brushChoice.Items.Filter =
					o => ((KeyValuePair<string, CommonBrushType>)o).Value != CommonBrushType.MaterialDesign;
			}
			/*
			this.brushChoice.KeyUp += (s, e) => {
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
			};*/
		}

		internal void FocusFirstChild ()
		{
			this.brushChoice?.FocusSelectedItem ();
		}

		private ChoiceControl brushChoice;
		private Expander advancedPropertyPanel;
		private SolidBrushEditorControl solidBrushEditor;
		private ResourceBrushEditorControl resourceBrushEditor;
		private MaterialDesignColorEditorControl materialDesignColorEditor;

		private BrushPropertyViewModel ViewModel => DataContext as BrushPropertyViewModel;
	}
}
