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
		public ObjectPropertyViewModel (TargetPlatform targetPlatform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (targetPlatform, property, editors)
		{
			if (targetPlatform == null)
				throw new ArgumentNullException (nameof(targetPlatform));

			ValueModel = new ObjectViewModel (targetPlatform);
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

		public override Resource Resource
		{
			// TODO: WPF can support this
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		public ObjectViewModel ValueModel
		{
			get;
		}

		public string CustomExpression
		{
			get { return this.customExpression; }
			set
			{
				if (this.customExpression == value)
					return;

				SetValue (new ValueInfo<object> {
					CustomExpression = value
				});
			}
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

		public override ValueSource ValueSource
		{
			get { return this.valueSource; }
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

				bool multipleValues = false, multipleSources = false;
				ValueSource? source = null;
				ITypeInfo type = null;
				string expression = null;

				ValueInfo<object>[] values = await Task.WhenAll (Editors.Where (e => e != null).Select (ed => ed.GetValueAsync<object> (Property)));
				for (int i = 0; i < values.Length; i++) {
					ValueInfo<object> info = values[i];
					if (source == null)
						source = info.Source;
					else if (source.Value != info.Source)
						multipleSources = true;

					if (type == null)
						type = info.ValueDescriptor as ITypeInfo;
					else if (!multipleValues && !Equals (type, info.ValueDescriptor as ITypeInfo))
						multipleValues = true;

					if (i == 0)
						expression = info.CustomExpression;
					else if (info.CustomExpression != expression) {
						expression = null;
						multipleValues = true;
					}

					if (info.Value != null)
						ValueModel.SelectedObjects.Add (info.Value);
				}

				this.customExpression = expression;
				MultipleValues = multipleValues;
				ValueType = (!multipleValues) ? type : null;
				if (multipleSources)
					SetValueSource (ValueSource.Unknown);
				else
					SetValueSource (source ?? ValueSource.Default);

				SetCanDelve (values.Length > 0);
				OnPropertyChanged (nameof(CustomExpression));
			}
		}

		private AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes;
		private bool createInstancePending;
		private string customExpression;
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

		private async void SetValue (ValueInfo<object> valueInfo)
		{
			await SetValueAsync (valueInfo);
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

						selectedType = args.SelectedType;
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

		private void SetValueSource (ValueSource value)
		{
			if (this.valueSource == value)
				return;

			this.valueSource = value;
			OnPropertyChanged (nameof (ValueSource));
		}
	}
}
