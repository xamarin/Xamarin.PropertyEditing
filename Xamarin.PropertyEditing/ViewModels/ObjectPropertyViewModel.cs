using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class TypeRequestedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Gets or sets a task for the type selected by the user from the UI
		/// </summary>
		public Task<ITypeInfo> SelectedType
		{
			get;
			set;
		}
	}

	internal class ObjectPropertyViewModel
		: PropertyViewModel<object>
	{
		public ObjectPropertyViewModel (TargetPlatform targetPlatform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (targetPlatform, property, editors, variation)
		{
			if (targetPlatform == null)
				throw new ArgumentNullException (nameof(targetPlatform));

			ValueModel = new ObjectViewModel (targetPlatform);
			RequestCurrentValueUpdate();
			CreateInstanceCommand = new RelayCommand (CreateInstance, () => IsAvailable && !IsCreateInstancePending);
		}

		public event EventHandler<TypeRequestedEventArgs> TypeRequested;

		public AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> AssignableTypes
		{
			get
			{
				if (this.assignableTypes == null)
					this.assignableTypes = new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (GetAssignableTypesAsync());

				return this.assignableTypes;
			}
		}

		public ICommand CreateInstanceCommand
		{
			get;
		}

		public override bool CanDelve
		{
			get { return this.canDelve; }
		}

		public ObjectViewModel ValueModel
		{
			get;
		}

		public ITypeInfo ValueType
		{
			get { return this.valueType; }
			private set
			{
				if (Equals (this.valueType, value))
					return;

				this.valueType = value;
				OnPropertyChanged();
			}
		}

		protected override void OnPropertyChanged ([CallerMemberName] string propertyName = null)
		{
			if (propertyName == nameof(IsAvailable)) {
				((RelayCommand)CreateInstanceCommand).ChangeCanExecute();
			}

			base.OnPropertyChanged (propertyName);
		}

		protected override async Task UpdateCurrentValueAsync ()
		{
			if (ValueModel == null)
				return;

			using (await AsyncWork.RequestAsyncWork (this)) {
				await base.UpdateCurrentValueAsync ();
				ValueType = CurrentValue?.ValueDescriptor as ITypeInfo;

				if (CurrentValue?.Value != null) {
					ValueModel.SelectedObjects.Reset (new[] { CurrentValue.Value });
				} else {
					ValueModel.SelectedObjects.Clear ();
				}

				SetCanDelve (ValueModel.SelectedObjects.Count > 0);
			}
		}

		private AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes;
		private bool createInstancePending;
		private ITypeInfo valueType;
		private bool canDelve;

		private bool IsCreateInstancePending
		{
			get { return this.createInstancePending; }
			set
			{
				if (this.createInstancePending == value)
					return;

				this.createInstancePending = value;
				((RelayCommand)CreateInstanceCommand).ChangeCanExecute();
			}
		}

		private void SetCanDelve (bool value)
		{
			if (this.canDelve == value)
				return;

			this.canDelve = value;
			OnPropertyChanged (nameof(CanDelve));
		}

		private async Task<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> GetAssignableTypesAsync ()
		{
			AssignableTypesResult result = await Editors.GetCommonAssignableTypes (Property, childTypes: false).ConfigureAwait (false);
			return result.GetTypeTree ();
		}

		private async void CreateInstance ()
		{
			IsCreateInstancePending = true;

			try {
				using (await AsyncWork.RequestAsyncWork (this)) {
					ITypeInfo selectedType = null;
				
					var types = await AssignableTypes.Task;
					// If there's only one assignable type, we'll skip selection
					if (types.Count == 1) {
						var kvp = types.First ();
						if (kvp.Value.Count == 1) {
							var group = kvp.Value.First ();
							if (!group.Skip (1).Any ()) {
								selectedType = group.First ();
							}
						}
					}

					if (selectedType == null) {
						var args = new TypeRequestedEventArgs ();
						TypeRequested?.Invoke (this, args);
						if (args.SelectedType == null)
							return;

						try {
							selectedType = await args.SelectedType;
 						} catch (OperationCanceledException) {
							return;
						}
					}

					await SetValueAsync (new ValueInfo<object> {
						Value = await TargetPlatform.EditorProvider.CreateObjectAsync (selectedType),
						ValueDescriptor = selectedType,
						Source = ValueSource.Local
					});
				}
			} finally {
				IsCreateInstancePending = false;
			}
		}
	}
}
