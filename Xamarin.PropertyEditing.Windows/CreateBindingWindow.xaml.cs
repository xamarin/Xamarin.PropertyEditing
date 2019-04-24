using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Xamarin.PropertyEditing.Common;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal partial class CreateBindingWindow
		: WindowEx
	{
		public CreateBindingWindow (IEnumerable<ResourceDictionary> merged, TargetPlatform platform, IObjectEditor editor, IPropertyInfo property, PropertyVariation variation = null,  object bindingObject = null)
		{
			var vm = new CreateBindingViewModel (platform, editor, property, variation, bindingObject);
			vm.CreateValueConverterRequested += OnCreateValueConverterRequested;

			DataContext = vm;
			InitializeComponent ();
			Resources.MergedDictionaries.AddItems (merged);
		}

		private void OnCreateValueConverterRequested (object sender, CreateValueConverterEventArgs e)
		{
			var vm = (CreateBindingViewModel) DataContext;
			ITypeInfo valueConverter = vm.TargetPlatform.EditorProvider.KnownTypes[typeof(CommonValueConverter)];

			var typesTask = vm.TargetPlatform.EditorProvider.GetAssignableTypesAsync (valueConverter, childTypes: false)
				.ContinueWith (t => t.Result.GetTypeTree(), TaskScheduler.Default);

			var result = CreateValueConverterWindow.RequestConverter (this, vm.TargetPlatform, vm.Target, new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (typesTask));
			if (result == null)
				return;

			e.Name = result.Item1;
			e.ConverterType = result.Item2;
			e.Source = result.Item3;
		}

		private void OnOkClicked (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void OnCancelClicked (object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		internal static object CreateBinding (FrameworkElement owner, TargetPlatform platform, IObjectEditor editor, IPropertyInfo property, PropertyVariation variation = null, object bindingObject = null)
		{
			Window ownerWindow = Window.GetWindow (owner);
			var window = new CreateBindingWindow (owner.Resources.MergedDictionaries, platform, editor, property, variation, bindingObject) {
				Owner = ownerWindow
			};
			bool? result = window.ShowDialog ();
			if (!result.HasValue || !result.Value)
				return null;

			var vm = (CreateBindingViewModel)window.DataContext;
			return vm.SelectedObjects.Single();
		}

		private void OnMoreSettingsExpanded (object sender, RoutedEventArgs e)
		{
			var fe = this.moreSettings.Content as FrameworkElement;
			if (fe == null)
				return;

			fe.Measure (new Size (Double.PositiveInfinity, Double.PositiveInfinity));
			Height += fe.DesiredSize.Height;
		}

		private void OnMoreSettingsCollapsed (object sender, RoutedEventArgs e)
		{
			var fe = this.moreSettings.Content as FrameworkElement;
			if (fe == null)
				return;

			Height -= fe.DesiredSize.Height;
		}
	}
}