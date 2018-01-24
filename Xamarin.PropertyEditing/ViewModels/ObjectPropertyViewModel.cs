using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cadenza.Collections;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class TypeRequestedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Gets or sets the type selected by the user from the UI
		/// </summary>
		public ITypeInfo SelectedType
		{
			get;
			set;
		}
	}

	internal class ObjectPropertyViewModel
		: PropertyViewModel
	{
		public ObjectPropertyViewModel (IEditorProvider provider, TargetPlatform targetPlatform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (property, editors)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));
			if (targetPlatform == null)
				throw new ArgumentNullException (nameof(targetPlatform));

			this.provider = provider;
			ValueModel = new ObjectViewModel (provider, targetPlatform);
			RequestCurrentValueUpdate();

			QueryTypes();
			CreateInstanceCommand = new RelayCommand (CreateInstance, () => IsAvailable && !IsCreateInstancePending);
			ClearValueCommand = new RelayCommand (OnClearValue, CanClearValue);
		}

		public event EventHandler<TypeRequestedEventArgs> TypeRequested;

		public AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> AssignableTypes
		{
			get { return this.assignableTypes; }
			private set
			{
				if (this.assignableTypes == value)
					return;

				this.assignableTypes = value;
				OnPropertyChanged();
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

		public ValueSource ValueSource
		{
			get { return this.valueSource; }
			private set
			{
				if (this.valueSource == value)
					return;

				this.valueSource = value;
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
				ValueModel.SelectedObjects.Clear();

				bool multipleTypes = false, multipleSources = false;
				ValueSource? source = null;
				ITypeInfo type = null;

				ValueInfo<object>[] values = await Task.WhenAll (Editors.Where (e => e != null).Select (ed => ed.GetValueAsync<object> (Property)));
				for (int i = 0; i < values.Length; i++) {
					ValueInfo<object> info = values[i];
					ValueModel.SelectedObjects.Add (info.Value);

					if (source == null)
						source = info.Source;
					else if (source.Value != info.Source)
						multipleSources = true;

					if (type == null)
						type = info.ValueDescriptor as ITypeInfo;
					else if (!multipleTypes && !Equals (type, info.ValueDescriptor as ITypeInfo))
						multipleTypes = true;
				}

				MultipleValues = multipleTypes;
				ValueType = (!multipleTypes) ? type : null;
				if (multipleSources)
					ValueSource = ValueSource.Unknown;
				else
					ValueSource = source ?? ValueSource.Default;

				SetCanDelve (values.Length > 0);
			}
		}

		private readonly IEditorProvider provider;
		private AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes;
		private bool createInstancePending;
		private ValueSource valueSource;
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

		private async void OnClearValue ()
		{
			await SetValueAsync (new ValueInfo<object> {
				Source = ValueSource.Default,
				Value = null
			});
		}

		private bool CanClearValue ()
		{
			return (Property.ValueSources.HasFlag (ValueSources.Local) && Property.ValueSources.HasFlag (ValueSources.Default) && ValueSource == ValueSource.Local);
		}

		private Task SetValueAsync (ValueInfo<object> valueInfo)
		{
			Task[] setValues = new Task[Editors.Count];
			int i = 0;
			foreach (IObjectEditor editors in Editors) {
				setValues[i++] = editors.SetValueAsync (Property, valueInfo);
			}

			return Task.WhenAll (setValues);
		}

		private void QueryTypes ()
		{
			AssignableTypes = new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (GetAssignableTypesAsync());
		}

		private async Task<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> GetAssignableTypesAsync ()
		{
			Task<IReadOnlyList<ITypeInfo>>[] typeTasks = Editors.Select (o => o.GetAssignableTypesAsync (Property)).ToArray();
			IReadOnlyList<ITypeInfo>[] lists = await Task.WhenAll (typeTasks).ConfigureAwait (false);

			var assemblies = new Dictionary<IAssemblyInfo, ILookup<string, ITypeInfo>> ();
			foreach (ITypeInfo type in lists.SelectMany (t => t)) {
				if (!assemblies.TryGetValue (type.Assembly, out ILookup<string, ITypeInfo> types)) {
					assemblies[type.Assembly] = types = new ObservableLookup<string, ITypeInfo> ();
				}

				((IMutableLookup<string, ITypeInfo>) types).Add (type.NameSpace, type);
			}

			return assemblies;
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

						selectedType = args.SelectedType;
					}

					await SetValueAsync (new ValueInfo<object> {
							Value = await this.provider.CreateObjectAsync (selectedType),
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
