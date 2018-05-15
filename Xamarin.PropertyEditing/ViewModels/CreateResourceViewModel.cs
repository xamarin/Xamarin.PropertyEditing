using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateResourceViewModel
		: NotifyingObject, INotifyDataErrorInfo
	{
		public CreateResourceViewModel (IResourceProvider provider, IEnumerable<object> targets, IPropertyInfo property)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));
			if (targets == null)
				throw new ArgumentNullException (nameof(targets));
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			this.provider = provider;
			this.targets = targets.ToArray();
			this.property = property;

			CreateResourceCommand = new RelayCommand (OnCreateResource, CanCreateResource);

			LoadingTask = RequestSources ();
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
		public event EventHandler CreateResource;

		public IPropertyInfo Property => this.property;

		public string ResourceKey
		{
			get { return this.resourceKey; }
			set
			{
				if (this.resourceKey == value)
					return;

				this.resourceKey = value;
				OnPropertyChanged();
				((RelayCommand)CreateResourceCommand).ChangeCanExecute();

				if (SelectedResourceSource != null)
					RequestErrorCheck();
			}
		}

		public bool IsKeyed
		{
			get { return this.isKeyed; }
			set
			{
				if (this.isKeyed == value)
					return;

				this.isKeyed = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(IsAppliedToAll));
			}
		}

		public bool IsAppliedToAll
		{
			get { return !this.isKeyed; } // Reverse on isKeyed for easier binding
			set
			{
				if (this.isKeyed == !value)
					return;

				this.isKeyed = !value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof (IsKeyed));
			}
		}

		public bool CanApplyToAll
		{
			get;
		} = false; // Support for this to come later (requires a different in-point)

		public ResourceSource SelectedResourceSource
		{
			get { return this.selectedSource; }
			set
			{
				if (this.selectedSource == value)
					return;

				this.selectedSource = value;
				OnPropertyChanged();
				RequestErrorCheck ();
				((RelayCommand)CreateResourceCommand).ChangeCanExecute();
			}
		}

		public bool DefineInApplication
		{
			get { return this.defineIn == ResourceSourceType.Application; }
			set
			{
				if (DefineInApplication == value)
					return;

				if (!value) {
					DefineInDocument = true;
					return;
				}

				this.defineIn = ResourceSourceType.Application;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(DefineInDocument));
				OnPropertyChanged (nameof(DefineInApplicationSource));
				SelectedResourceSource = this.applicationSource;
			}
		}

		public bool DefineInDocument
		{
			get { return this.defineIn == ResourceSourceType.Document; }
			set
			{
				if (DefineInDocument == value)
					return;

				if (value) {
					this.defineIn = ResourceSourceType.Document;
					SelectedResourceSource = SelectedDocumentSource;
				} else {
					DefineInApplication = true;
					return;
				}

				OnPropertyChanged ();
				OnPropertyChanged (nameof(DefineInApplication));
				OnPropertyChanged (nameof(DefineInApplicationSource));
			}
		}

		public ResourceSource SelectedDocumentSource
		{
			get { return this.selectedDocSource; }
			set
			{
				if (this.selectedDocSource == value)
					return;

				this.selectedDocSource = value;
				OnPropertyChanged();
				SelectedResourceSource = value;
			}
		}

		public bool DefineInApplicationSource
		{
			get { return this.defineIn == ResourceSourceType.ResourceDictionary; }
			set {
				if (DefineInApplicationSource == value)
					return;

				if (value) {
					this.defineIn = ResourceSourceType.ResourceDictionary;
					SelectedResourceSource = SelectedApplicationSource;
				} else {
					DefineInApplication = true;
					return;
				}

				OnPropertyChanged ();
				OnPropertyChanged (nameof (DefineInApplication));
				OnPropertyChanged (nameof (DefineInDocument));
			}
		}

		public ResourceSource SelectedApplicationSource
		{
			get { return this.selectedDictSource; }
			set
			{
				if (this.selectedDictSource == value)
					return;

				this.selectedDictSource = value;
				OnPropertyChanged();
				SelectedResourceSource = value;
			}
		}

		public bool HasApplicationSources => (ApplicationSources?.Count ?? 0) > 0;

		public IReadOnlyList<ResourceSource> ApplicationSources
		{
			get { return this.applicationSources; }
			private set
			{
				if (this.applicationSources == value)
					return;

				this.applicationSources = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(HasApplicationSources));
			}
		}

		public bool HasDocumentSources => (DocumentSources?.Count ?? 0) > 0;

		public IReadOnlyList<ResourceSource> DocumentSources
		{
			get { return this.documentSources; }
			private set
			{
				if (this.documentSources == value)
					return;

				this.documentSources = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(HasDocumentSources));
			}
		}

		public ICommand CreateResourceCommand
		{
			get;
		}

		public Task LoadingTask
		{
			get;
		}

		public bool IsLoading
		{
			get { return this.isLoading; }
			private set
			{
				if (this.isLoading == value)
					return;

				this.isLoading = value;
				OnPropertyChanged();
			}
		}

		public bool HasErrors
		{
			get { return this.errors.Count > 0; }
		}

		public bool HasFatalError
		{
			get { return FatalError != null; }
		}

		public string FatalError
		{
			get { return this.fatalError; }
			private set
			{
				if (this.fatalError == value)
					return;

				this.fatalError = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(HasFatalError));
				((RelayCommand)CreateResourceCommand).ChangeCanExecute();
			}
		}

		public IEnumerable GetErrors (string propertyName)
		{
			if (this.errors.TryGetValue (propertyName, out Tuple<string, bool> error) && error != null)
				return new string[] { error.Item1 };
			else
				return Enumerable.Empty<string> ();
		}

		private readonly IPropertyInfo property;

		private ResourceSource selectedSource, selectedDictSource, selectedDocSource;
		private ResourceSource applicationSource;
		private IReadOnlyList<ResourceSource> applicationSources;
		private IReadOnlyList<ResourceSource> documentSources;
		private ResourceSourceType defineIn;
		private bool isKeyed = true;
		private string resourceKey;
		private bool isLoading = true;
		private string fatalError;

		private readonly Dictionary<string, Tuple<string, bool>> errors = new Dictionary<string, Tuple<string, bool>> ();
		private readonly IResourceProvider provider;
		private readonly object[] targets;

		private void SetError (string propertyName, Tuple<string, bool> incoming)
		{
			bool signal = false;
			if (incoming == null)
				signal = this.errors.Remove (propertyName);
			else {
				if (signal = !this.errors.TryGetValue (propertyName, out Tuple<string, bool> current) || !Equals (current, incoming))
					this.errors[propertyName] = incoming;
			}

			if (signal) {
				OnPropertyChanged (nameof(HasErrors));
				ErrorsChanged?.Invoke (this, new DataErrorsChangedEventArgs (propertyName));
				((RelayCommand)CreateResourceCommand).ChangeCanExecute();
			}
		}

		private async Task RequestSources ()
		{
			var appSources = new List<ResourceSource> ();
			var docSources = new List<ResourceSource> ();

			Task suggestedNameTask = RequestSuggestedName ();

			// Not sorted on purpose; we want implementers to be able to importance by order (ex. document hierarchy).
			try {
				var allSources = await Task.WhenAll (this.targets.Select (t => this.provider.GetResourceSourcesAsync (t, this.property)));
				IEnumerable<ResourceSource> sources = allSources[0];
				if (allSources.Length > 1) {
					HashSet<ResourceSource> commonSources = new HashSet<ResourceSource> (allSources[0]);
					for (int i = 1; i < allSources.Length; i++) {
						commonSources.IntersectWith (allSources[i]);
					}

					sources = allSources[0].Where (s => commonSources.Contains (s));
				}

				foreach (ResourceSource source in sources) {
					if (source.Type == ResourceSourceType.Application) {
						this.applicationSource = source;
					} else if (source.Type == ResourceSourceType.ResourceDictionary)
						appSources.Add (source);
					else if (source.Type == ResourceSourceType.Document)
						docSources.Add (source);
				}

				if (this.applicationSource == null)
					throw new NotSupportedException ("You must define an application level resource source.");

				ApplicationSources = appSources;
				SelectedApplicationSource = appSources.FirstOrDefault ();
				DocumentSources = docSources;
				SelectedDocumentSource = docSources.FirstOrDefault ();
				DefineInApplication = true;
				await suggestedNameTask;
			} catch (Exception ex) {
				FatalError = ex.Message;
			} finally {
				IsLoading = false;
			}
		}

		private async void RequestErrorCheck ()
		{
			if (!String.IsNullOrEmpty (ResourceKey)) {
				try {
					foreach (object target in this.targets) {
						ResourceCreateError error = await this.provider.CheckNameErrorsAsync (target, SelectedResourceSource, ResourceKey);
						SetError (nameof(ResourceKey), error != null ? new Tuple<string, bool> (error.Message, error.IsWarning) : null);
						if (error != null)
							break;
					}
				} catch (Exception ex) {
					FatalError = ex.Message;
				}
			} else {
				SetError (nameof(ResourceKey), null);
			}
		}

		private async Task RequestSuggestedName ()
		{
			string suggested = await this.provider.SuggestResourceNameAsync (this.targets, this.property);
			if (String.IsNullOrWhiteSpace (ResourceKey))
				ResourceKey = suggested;
		}

		private bool CanCreateResource ()
		{
			return !String.IsNullOrWhiteSpace (ResourceKey) && SelectedResourceSource != null && !(this.errors.Any (kvp => !kvp.Value.Item2)) && !HasFatalError;
		}

		private void OnCreateResource ()
		{
			CreateResource?.Invoke (this, EventArgs.Empty);
		}
	}
}