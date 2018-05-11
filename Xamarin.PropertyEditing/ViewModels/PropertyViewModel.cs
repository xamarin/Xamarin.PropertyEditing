using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PropertyViewModel<TValue>
		: PropertyViewModel
	{
		static PropertyViewModel ()
		{
			Type t = typeof(TValue);
			if (t.Name == NullableName)
				DefaultValue = (TValue) Activator.CreateInstance (Nullable.GetUnderlyingType (t));
			else
				DefaultValue = default(TValue);
		}

		public PropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			this.coerce = property as ICoerce<TValue>;
			this.validator = property as IValidator<TValue>;
			this.valueNavigator = property as ICanNavigateToSource;
			this.isNullable = (!property.ValueSources.HasFlag (ValueSources.Default) || property.Type.Name == NullableName);

			RequestCreateResourceCommand = new RelayCommand (OnCreateResource, CanCreateResource);
			NavigateToValueSourceCommand = new RelayCommand (OnNavigateToSource, CanNavigateToSource);
			SetValueResourceCommand = new RelayCommand<Resource> (OnSetValueToResource, CanSetValueToResource);
			ClearValueCommand = new RelayCommand (OnClearValue, CanClearValue);
			ConvertToLocalValueCommand = new RelayCommand (OnConvertToLocalValue, CanClearToLocalValue);

			RequestCurrentValueUpdate();
		}

		public override ValueSource ValueSource => this.value != null ? this.value.Source : ValueSource.Default;

		/// <remarks>
		/// This is only meant for use from the UI. Other value setting mechanisms (like, converting resources to local, or setting to a resource) should
		/// use bespoke value setting mechanisms as this property switches the value source to local and only if the value itself has changed.
		/// </remarks>
		public TValue Value
		{
			get { return (this.value != null) ? this.value.Value : default(TValue); }
			set
			{
				value = CoerceValue (value);

				if (Equals (value, Value))
					return;

				SetValue (new ValueInfo<TValue> {
					Source = ValueSource.Local,
					Value = value
				});
			}
		}

		public override Resource Resource
		{
			get => this.value?.ValueDescriptor as Resource;
			set {
				if (Resource == value)
					return;

				if (value == null)
					return;

				if (SetValueResourceCommand.CanExecute (value))
					SetValueResourceCommand.Execute (value);
			}
		}

		public string CustomExpression
		{
			get { return this.value?.CustomExpression; }
			set
			{
				if (value == null) {
					SetValue (new ValueInfo<TValue> {
						Source = ValueSource.Unset
					});
				} else {
					SetValue (new ValueInfo<TValue> {
						CustomExpression = value
					});
				}
			}
		}

		public override bool SupportsValueSourceNavigation => this.valueNavigator != null;

		protected virtual TValue CoerceValue (TValue validationValue)
		{
			if (!this.isNullable && validationValue == null) {
				validationValue = DefaultValue;
			}

			if (this.coerce != null)
				validationValue = this.coerce.CoerceValue (validationValue);

			return validationValue;
		}

		/// <remarks>
		/// For updating property-type-specific value properties, override this. <see cref="PropertyViewModel.MultipleValues"/> <see cref="Value"/> is up to date.
		/// </remarks>
		protected virtual void OnValueChanged ()
		{
			((RelayCommand)RequestCreateResourceCommand)?.ChangeCanExecute();
		}

		/// <summary>
		/// Obtains the current value from the editors and updates the value properties.
		/// </summary>
		protected override async Task UpdateCurrentValueAsync ()
		{
			if (Property == null)
				return;

			ValueInfo<TValue> currentValue = null;

			using (await AsyncWork.RequestAsyncWork (this)) {
				bool disagree = false;
				ValueInfo<TValue>[] values = await Task.WhenAll (Editors.Select (ed => ed.GetValueAsync<TValue> (Property, Variation)).ToArray ());
				foreach (ValueInfo<TValue> valueInfo in values) {
					if (currentValue == null)
						currentValue = valueInfo;
					else {
						if (valueInfo == null) {
							currentValue.Value = default (TValue);
							disagree = true;
							continue;
						}

						if (currentValue.Source != valueInfo.Source) {
							currentValue.Source = ValueSource.Default;
							disagree = true;
						}

						if (!Equals (currentValue.Value, valueInfo.Value)) {
							currentValue.Value = default(TValue);
							disagree = true;
						}

						if (!Equals (currentValue.ValueDescriptor, valueInfo.ValueDescriptor)) {
							currentValue.ValueDescriptor = null;
							disagree = true;
						}
					}
				}

				// The public setter for Value is a local set for binding
				SetCurrentValue (currentValue, disagree);
			}
		}

		protected override ResourceRequestedEventArgs CreateRequestResourceArgs ()
		{
			var args = base.CreateRequestResourceArgs ();
			args.Resource = Resource;
			return args;
		}

		protected async Task SetValueAsync (ValueInfo<TValue> newValue)
		{
			if (this.value == newValue)
				return;

			SetError (null);

			// We may need to be more careful about value sources here
			if (this.validator != null && !this.validator.IsValid (newValue.Value)) {
				SignalValueChange(); // Ensure UI refresh its own value
				return;
			}

			using (await AsyncWork.RequestAsyncWork (this)) {
				try {
					Task[] setValues = new Task[Editors.Count];
					int i = 0;
					foreach (IObjectEditor editor in Editors) {
						setValues[i++] = editor.SetValueAsync (Property, newValue);
					}

					await Task.WhenAll (setValues);
					// Implementers should raise PropertyChanged during the set task
					// which will bring the update under the async work request.
				} catch (Exception ex) {
					AggregateException aggregate = ex as AggregateException;
					if (aggregate != null) {
						aggregate = aggregate.Flatten ();
						ex = aggregate.InnerExceptions[0];
					}

					SetError (ex.ToString ());
				}
			}
		}

		private readonly ICoerce<TValue> coerce;
		private readonly IValidator<TValue> validator;
		private readonly ICanNavigateToSource valueNavigator;
		internal const string NullableName = "Nullable`1";
		private bool isNullable;
		private ValueInfo<TValue> value;

		private void SignalValueChange ()
		{
			OnPropertyChanged (nameof (Value));
			OnPropertyChanged (nameof (ValueSource));
			OnPropertyChanged (nameof (CustomExpression));
			OnPropertyChanged (nameof (Resource));
		}

		private bool SetCurrentValue (ValueInfo<TValue> newValue, bool multipleValues)
		{
			if (!this.isNullable && !multipleValues && newValue != null && newValue.Value == null)
				newValue.Value = DefaultValue;

			MultipleValues = multipleValues;

			if (this.value == newValue)
				return false;

			this.value = newValue;
			OnValueChanged ();
			SignalValueChange();

			((RelayCommand) ConvertToLocalValueCommand)?.ChangeCanExecute ();
			((RelayCommand) ClearValueCommand)?.ChangeCanExecute ();
			((RelayCommand) NavigateToValueSourceCommand)?.ChangeCanExecute ();

			return true;
		}

		private async void SetValue (ValueInfo<TValue> newValue)
		{
			await SetValueAsync (newValue);
		}

		private bool CanSetValueToResource (Resource resource)
		{
			return (ResourceProvider != null && resource != null && SupportsResources);
		}

		private void OnSetValueToResource (Resource resource)
		{
			if (resource == null)
				throw new ArgumentNullException (nameof (resource));
			
			SetValue (new ValueInfo<TValue> {
				Source = ValueSource.Resource,
				ValueDescriptor = resource
			});
		}

		private void OnClearValue ()
		{
			SetValue (new ValueInfo<TValue> {
				Source = ValueSource.Unset
			});
		}

		private bool CanClearValue ()
		{
			return (ValueSource != ValueSource.Default && ValueSource != ValueSource.Unset && ValueSource != ValueSource.Unknown);
		}

		private bool CanClearToLocalValue ()
		{
			return (ValueSource != ValueSource.Local && ValueSource != ValueSource.Unset);
		}

		private void OnConvertToLocalValue ()
		{
			SetValue (new ValueInfo<TValue> {
				Value = Value,
				Source = ValueSource.Local
			});
		}

		private bool CanNavigateToSource ()
		{
			if (this.valueNavigator == null)
				return false;
			if (Editors.Count != 1)
				return false;
			if (ValueSource == ValueSource.Default || ValueSource == ValueSource.Unset)
				return false;

			return this.valueNavigator.CanNavigateToSource (Editors.Single());
		}

		private void OnNavigateToSource ()
		{
			this.valueNavigator?.NavigateToSource (Editors.FirstOrDefault ());
		}

		private bool CanCreateResource ()
		{
			return SupportsResources && ResourceProvider != null && !MultipleValues;
		}

		private async void OnCreateResource ()
		{
			var e = RequestCreateResource ();
			if (e.Source == null)
				return;

			Resource resource =  await ResourceProvider.CreateResourceAsync (e.Source, e.Name, Value);
			OnSetValueToResource (resource);
		}

		private static TValue DefaultValue;
	}

	internal abstract class PropertyViewModel
		: EditorViewModel, INotifyDataErrorInfo
	{
		protected PropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, editors)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));

			Property = property;
			SetupConstraints ();

			this.requestResourceCommand = new RelayCommand (OnRequestResource, CanRequestResource);
		}

		public event EventHandler<ResourceRequestedEventArgs> ResourceRequested;
		public event EventHandler<CreateResourceRequestedEventArgs> CreateResourceRequested;
		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		public IPropertyInfo Property
		{
			get;
		}

		public override string Name => Property.Name;
		public override string Category => Property.Category;

		public bool IsAvailable
		{
			get { return this.isAvailable?.Result ?? true; }
			private set
			{
				if (this.isAvailable.Result == value)
					return;

				this.isAvailable = Task.FromResult (value);
				OnPropertyChanged();
			}
		}

		public virtual bool CanDelve => false;

		public bool SupportsResources
		{
			get { return Property.CanWrite && Property.ValueSources.HasFlag (ValueSources.Resource); }
		}

		public bool CanCreateResources
		{
			get { return SupportsResources && (ResourceProvider?.CanCreateResources ?? false); }
		}

		public abstract Resource Resource
		{
			get;
			set;
		}

		public IResourceProvider ResourceProvider
		{
			get { return this.resourceProvider; }
			set
			{
				if (this.resourceProvider == value)
					return;

				this.resourceProvider = value;
				OnPropertyChanged ();

				if (SetValueResourceCommand is RelayCommand<Resource> r)
					r.ChangeCanExecute();
			}
		}

		public ICommand SetValueResourceCommand
		{
			get { return this.setValueResourceCommand; }
			protected set
			{
				if (this.setValueResourceCommand == value)
					return;

				if (this.setValueResourceCommand != null)
					this.setValueResourceCommand.CanExecuteChanged -= OnSetValueResourceCommandCanExecuteChanged;

				this.setValueResourceCommand = value;
				if (this.setValueResourceCommand != null)
					this.setValueResourceCommand.CanExecuteChanged += OnSetValueResourceCommandCanExecuteChanged;

				((RelayCommand)RequestResourceCommand).ChangeCanExecute();
			}
		}

		public ICommand RequestResourceCommand => this.requestResourceCommand;

		public ICommand RequestCreateResourceCommand
		{
			get;
			protected set;
		}

		public ICommand ClearValueCommand
		{
			get;
			protected set;
		}

		public ICommand ConvertToLocalValueCommand
		{
			get;
			protected set;
		}

		public ICommand NavigateToValueSourceCommand
		{
			get;
			protected set;
		}

		public virtual bool SupportsValueSourceNavigation => false;

		public abstract ValueSource ValueSource
		{
			get;
		}

		public bool HasErrors => this.error != null;

		public IEnumerable GetErrors (string propertyName)
		{
			return (this.error != null) ? new [] { this.error } : Enumerable.Empty<string> ();
		}

		/// <summary>
		/// Gets or sets the current <see cref="PropertyVariation"/> that the value is currently looking at.
		/// </summary>
		public PropertyVariation Variation
		{
			get { return this.variation; }
			set
			{
				if (this.variation == value)
					return;

				this.variation = value;
				OnPropertyChanged ();
			}
		}

		/// <param name="newError">The error message or <c>null</c> to clear the error.</param>
		protected void SetError (string newError)
		{
			if (this.error == newError)
				return;

			this.error = newError;
			OnErrorsChanged (new DataErrorsChangedEventArgs (nameof (Property)));
		}

		protected virtual async void OnEditorPropertyChanged (object sender, EditorPropertyChangedEventArgs e)
		{
			IDisposable work = null;
			if (this.constraintProperties != null && this.constraintProperties.Contains (e.Property)) {
				work = await AsyncWork.RequestAsyncWork (this);
				IsAvailable = await RequeryAvailabilityAsync ();
			}

			try {
				if (e.Property != null && !Equals (e.Property, Property))
					return;
		
				// TODO: Smarter querying, can query the single editor and check against MultipleValues
				await UpdateCurrentValueAsync ();
			} finally {
				work?.Dispose ();
			}
		}

		protected override void SetupEditor (IObjectEditor editor)
		{
			if (editor == null)
				return;

			base.SetupEditor (editor);
			editor.PropertyChanged += OnEditorPropertyChanged;
		}

		protected override void TeardownEditor (IObjectEditor editor)
		{
			if (editor == null)
				return;

			base.TeardownEditor (editor);
			editor.PropertyChanged -= OnEditorPropertyChanged;
		}

		protected CreateResourceRequestedEventArgs RequestCreateResource ()
		{
			var e = new CreateResourceRequestedEventArgs ();
			CreateResourceRequested?.Invoke (this, e);
			return e;
		}

		private readonly RelayCommand requestResourceCommand;
		private IResourceProvider resourceProvider;
		private ICommand setValueResourceCommand;
		private HashSet<IPropertyInfo> constraintProperties;
		private PropertyVariation variation;
		private string error;
		private Task<bool> isAvailable;

		private void OnErrorsChanged (DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke (this, e);
		}

		private void SetupConstraints ()
		{
			IReadOnlyList<IAvailabilityConstraint> constraints = Property.AvailabilityConstraints;
			if (constraints == null || constraints.Count == 0)
				return;

			for (int i = 0; i < constraints.Count; i++) {
				IAvailabilityConstraint constraint = constraints[i];
				IReadOnlyList<IPropertyInfo> properties = constraint.ConstrainingProperties;
				if (properties != null) {
					if (this.constraintProperties == null)
						this.constraintProperties = new HashSet<IPropertyInfo> ();

					foreach (IPropertyInfo property in properties)
						this.constraintProperties.Add (property);
				}
			}

			this.isAvailable = RequeryAvailabilityAsync ();
		}

		private async Task<bool> RequeryAvailabilityAsync()
		{
			var constraints = Property.AvailabilityConstraints;
			if (constraints == null || constraints.Count == 0)
				return true;

			using (await AsyncWork.RequestAsyncWork (this)) {
				HashSet<Task<bool>> tasks = new HashSet<Task<bool>> ();
				foreach (IObjectEditor editor in Editors) {
					foreach (IAvailabilityConstraint constraint in constraints) {
						tasks.Add (constraint.GetIsAvailableAsync (editor));
					}
				}

				while (tasks.Count > 0) {
					Task<bool> task = await Task.WhenAny (tasks);
					tasks.Remove (task);

					if (!task.Result)
						return false;
				}

				return true;
			}
		}

		protected virtual ResourceRequestedEventArgs CreateRequestResourceArgs ()
		{
			return new ResourceRequestedEventArgs ();
		}

		private bool CanRequestResource ()
		{
			return SupportsResources && ResourceProvider != null && SetValueResourceCommand != null;
		}

		private void OnRequestResource ()
		{
			var e = CreateRequestResourceArgs ();
			ResourceRequested?.Invoke (this, e);
			if (e.Resource == null)
				return;

			if (SetValueResourceCommand.CanExecute (e.Resource))
				SetValueResourceCommand.Execute (e.Resource);
		}

		private void OnSetValueResourceCommandCanExecuteChanged (object sender, EventArgs e)
		{
			((RelayCommand)RequestResourceCommand)?.ChangeCanExecute();
		}
	}
}
