using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class TypePropertyViewModel
		: PropertyViewModel<ITypeInfo>
	{
		public TypePropertyViewModel (TargetPlatform platform, IPropertyInfo propertyInfo, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, propertyInfo, editors, variation)
		{
			SelectTypeCommand = new RelayCommand (SetType, () => Property.CanWrite);
		}

		public event EventHandler<TypeRequestedEventArgs> TypeRequested;

		public ICommand SelectTypeCommand
		{
			get;
		}

		public AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> AssignableTypes
		{
			get
			{
				if (this.assignableTypes == null)
					this.assignableTypes = new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (GetAssignableTypesAsync ());

				return this.assignableTypes;
			}
		}

		private AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes;

		private async Task<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> GetAssignableTypesAsync ()
		{
			AssignableTypesResult result = await Editors.GetCommonAssignableTypes (Property, childTypes: false).ConfigureAwait (false);
			return result.GetTypeTree ();
		}

		private async void SetType ()
		{
			using (await AsyncWork.RequestAsyncWork (this)) {
				ITypeInfo selectedType = null;
				var args = new TypeRequestedEventArgs ();
				TypeRequested?.Invoke (this, args);
				if (args.SelectedType == null)
					return;

				try {
					selectedType = await args.SelectedType;
					if (selectedType == null)
						return;
				} catch (OperationCanceledException) {
					return;
				}

				await SetValueAsync (new ValueInfo<ITypeInfo> {
					Value = selectedType,
					Source = ValueSource.Local
				});
			}
		}
	}
}
