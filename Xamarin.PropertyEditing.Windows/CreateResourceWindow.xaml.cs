using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal partial class CreateResourceWindow
		: WindowEx
	{
		public CreateResourceWindow (IEnumerable<ResourceDictionary> merged, IResourceProvider provider, IEnumerable<object> targets, IPropertyInfo property)
		{
			Resources.MergedDictionaries.AddItems (merged);
			DataContext = new CreateResourceViewModel (provider, targets, property);
			InitializeComponent ();
			SetupResourceKey ();
		}

		private async void SetupResourceKey()
		{
			await ((CreateResourceViewModel) DataContext).LoadingTask;
			this.resourceKey.SelectAll();
			this.resourceKey.Focus ();
		}

		private void OnOkClicked (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void OnCancelClicked (object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		internal static Tuple<ResourceSource, string> CreateResource (FrameworkElement owner, IResourceProvider provider, IEnumerable<object> targets, IPropertyInfo property)
		{
			Window ownerWindow = Window.GetWindow (owner);
			var window = new CreateResourceWindow (owner.Resources.MergedDictionaries, provider, targets, property) {
				Owner = ownerWindow
			};
			window.Resources.MergedDictionaries.AddItems (owner.Resources.MergedDictionaries);
			bool? result = window.ShowDialog();

			var vm = (CreateResourceViewModel)window.DataContext;
			if (result.HasValue && result.Value) {
				return new Tuple<ResourceSource, string> (vm.SelectedResourceSource, vm.ResourceKey);
			} else {
				return new Tuple<ResourceSource, string> (null, null);
			}
		}
	}
}
