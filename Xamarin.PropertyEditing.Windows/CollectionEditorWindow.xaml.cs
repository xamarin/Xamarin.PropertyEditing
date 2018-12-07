using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal partial class CollectionEditorWindow
		: WindowEx, IPropertiesHost
	{
		public CollectionEditorWindow (IEnumerable<ResourceDictionary> mergedResources)
		{
			Resources.MergedDictionaries.AddItems (mergedResources);
			InitializeComponent ();
			DataContextChanged += OnDataContextChanged;
		}

		protected override void OnClosing (CancelEventArgs e)
		{
			if (DataContext is CollectionPropertyViewModel vm) {
				if (!DialogResult.HasValue || !DialogResult.Value)
					vm.CancelCommand.Execute (null);

				vm.TypeRequested -= OnTypeRequested;
			}

			base.OnClosing (e);
		}

		private void OnDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is CollectionPropertyViewModel oldVm)
				oldVm.TypeRequested -= OnTypeRequested;

			if (e.NewValue is CollectionPropertyViewModel vm)
				vm.TypeRequested += OnTypeRequested;
		}

		private void OnTypeRequested (object sender, TypeRequestedEventArgs args)
		{
			args.SelectedType = Task.FromResult (TypeSelectorWindow.RequestType (this, ((CollectionPropertyViewModel)DataContext).AssignableTypes));
		}

		private void OnOkClick (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void OnCancelClick (object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
