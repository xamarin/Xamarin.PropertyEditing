using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.PropertyEditing.Properties;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CreateValueConverterEventArgs
		: EventArgs
	{
		public string Name
		{
			get;
			set;
		}

		public ITypeInfo ConverterType
		{
			get;
			set;
		}

		public ResourceSource Source
		{
			get;
			set;
		}
	}

	internal class CreateBindingViewModel
		: PropertiesViewModel, IProvidePath
	{
		public CreateBindingViewModel (TargetPlatform platform, IObjectEditor targetEditor, IPropertyInfo property, PropertyVariation variations = null, bool includeAddValueConverter = true)
			: base (platform)
		{
			if (platform == null)
				throw new ArgumentNullException (nameof(platform));
			if (targetEditor == null)
				throw new ArgumentNullException (nameof(targetEditor));
			if (platform.BindingProvider == null)
				throw new ArgumentException ("Null BindingProvider on TargetPlatform", nameof(platform));
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			this.editorProvider = platform.EditorProvider;
			this.targetEditor = targetEditor;
			this.property = property;
			this.provider = platform.BindingProvider;
			this.variations = variations;
			IncludeAddValueConverter = includeAddValueConverter;

			PropertyDisplay = String.Format (Resources.CreateDataBindingTitle, $"[{this.targetEditor.TargetType.Name}].{property.Name}");
			RequestNamedDisplay ();

			BindingSources = new AsyncValue<IReadOnlyList<BindingSource>> (
					platform.BindingProvider.GetBindingSourcesAsync (targetEditor.Target, property));

			this.requestAddValueConverterCommand = new RelayCommand (OnRequestAddValueConverter, CanRequestAddValueConverter);

			RequestBindingObject ();
		}

		private async void RequestBindingObject ()
		{
			if (!TargetPlatform.EditorProvider.KnownTypes.TryGetValue (typeof (PropertyBinding), out ITypeInfo bindingType))
				throw new InvalidOperationException ("IEditorProvider does not have a known type for PropertyBinding");

			object binding = await TargetPlatform.EditorProvider.CreateObjectAsync (bindingType);
			SelectedObjects.Add (binding);
		}

		private async void RequestNamedDisplay ()
		{
			if (!(this.targetEditor is INameableObject nameable))
				return;

			string name = await nameable.GetNameAsync ();
			if (String.IsNullOrWhiteSpace (name))
				return;

			PropertyDisplay = String.Format (Resources.CreateDataBindingTitle, $"{name}.{this.property.Name}");
		}

		public event EventHandler<CreateValueConverterEventArgs> CreateValueConverterRequested;

		public object Target => this.targetEditor.Target;

		public string PropertyDisplay
		{
			get { return this.propertyDisplay; }
			private set
			{
				if (this.propertyDisplay == value)
					return;

				this.propertyDisplay = value;
				OnPropertyChanged();
			}
		}

		public bool CanCreateBinding
		{
			get
			{
				if (SelectedBindingSource == null)
					return false;

				switch (SelectedBindingSource.Type) {
				case BindingSourceType.SingleObject:
				case BindingSourceType.Object:
					return SelectedObjectTreeElement != null;
				case BindingSourceType.Resource:
					return SelectedResource != null;
				case BindingSourceType.Type:
					return TypeSelector.SelectedType != null;
				}

				return false;
			}
		}

		public bool ShowLongDescription
		{
			get
			{
				if (SelectedBindingSource == null)
					return false;

				return SelectedBindingSource.Type == BindingSourceType.SingleObject;
			}
		}

		public bool ShowTypeSelector
		{
			get
			{
				if (SelectedBindingSource == null || ShowLongDescription)
					return false;

				return SelectedBindingSource.Type == BindingSourceType.Type;
			}
		}

		public bool ShowTypeLevel
		{
			get
			{
				if (ObjectEditors.Count == 0)
					return false;

				IObjectEditor bindingEditor = ObjectEditors[0];
				return bindingEditor.KnownProperties.Values.Contains (PropertyBinding.TypeLevelProperty);
			}
		}

		public bool ShowObjectSelector
		{
			get
			{
				if (SelectedBindingSource == null || ShowLongDescription)
					return false;

				return SelectedBindingSource.Type == BindingSourceType.Object;
			}
		}

		public bool ShowResourceSelector
		{
			get
			{
				if (SelectedBindingSource == null || ShowLongDescription)
					return false;

				return SelectedBindingSource.Type == BindingSourceType.Resource;
			}
		}

		public TypeSelectorViewModel TypeSelector
		{
			get { return this.typeSelector; }
			private set
			{
				if (this.typeSelector == value)
					return;

				if (this.typeSelector != null)
					this.typeSelector.PropertyChanged -= OnTypeSelectorPropertyChanged;

				this.typeSelector = value;
				if (this.typeSelector != null)
					this.typeSelector.PropertyChanged += OnTypeSelectorPropertyChanged;

				OnPropertyChanged ();
			}
		}

		public AsyncValue<IReadOnlyList<BindingSource>> BindingSources
		{
			get;
		}

		public BindingSource SelectedBindingSource
		{
			get
			{
				if (!KnownPropertiesSetup)
					return null;

				return GetKnownPropertyViewModel (PropertyBinding.SourceProperty).Value;
			}

			set
			{
				var vm = GetKnownPropertyViewModel (PropertyBinding.SourceProperty);
				if (vm.Value == value)
					return;

				if (vm.Value != null) {
					switch (vm.Value.Type) {
					case BindingSourceType.SingleObject:
					case BindingSourceType.Object:
						SelectedObjectTreeElement = null;
						break;
					case BindingSourceType.Resource:
						SelectedResource = null;
						break;
					case BindingSourceType.Type:
						TypeSelector.SelectedType = null;
						break;
					}
				}

				vm.Value = value;
				OnPropertyChanged();
				UpdateShowProperties();
				RequestUpdateSources();

				((RelayCommand)RequestAddValueConverterCommand)?.ChangeCanExecute ();
			}
		}

		public AsyncValue<ILookup<ResourceSource, Resource>> SourceResources
		{
			get { return this.resources; }
			private set
			{
				this.resources = value;
				OnPropertyChanged();
			}
		}

		public Resource SelectedResource
		{
			get { return this.selectedResource; }
			set
			{
				if (this.selectedResource == value)
					return;

				this.selectedResource = value;
				BindingSourceTarget = value;
				OnPropertyChanged ();
				OnPropertyChanged (nameof(CanCreateBinding));

				RequestUpdateProperties ();
			}
		}

		public AsyncValue<IReadOnlyList<ObjectTreeElement>> ObjectElementRoots
		{
			get { return this.objectElementRoots; }
			private set
			{
				if (this.objectElementRoots == value)
					return;

				this.objectElementRoots = value;
				OnPropertyChanged ();

				RequestUpdateProperties ();
			}
		}

		public ObjectTreeElement SelectedObjectTreeElement
		{
			get { return this.selectedObjectTreeElement; }
			set
			{
				if (this.selectedObjectTreeElement == value)
					return;

				this.selectedObjectTreeElement = value;
				BindingSourceTarget = value?.Editor.Target;
				OnPropertyChanged ();
				OnPropertyChanged (nameof (CanCreateBinding));

				RequestUpdateProperties ();
			}
		}

		public string Path
		{
			get
			{
				if (!KnownPropertiesSetup)
					return null;

				return GetKnownPropertyViewModel<string> (PropertyBinding.PathProperty).Value;
			}

			set
			{
				var vm = GetKnownPropertyViewModel<string> (PropertyBinding.PathProperty);
				if (vm.Value == value)
					return;

				vm.Value = value;
				OnPropertyChanged();
			}
		}

		public AsyncValue<PropertyTreeRoot> PropertyRoot
		{
			get { return this.propertyRoot; }
			private set
			{
				if (this.propertyRoot == value)
					return;

				this.propertyRoot = value;
				OnPropertyChanged();
			}
		}

		public PropertyTreeElement SelectedPropertyElement
		{
			get { return this.selectedPropertyElement; }
			set
			{
				if (this.selectedPropertyElement == value)
					return;

				this.selectedPropertyElement = value;
				OnPropertyChanged();

				UpdatePath ();
			}
		}

		public AsyncValue<IReadOnlyList<Resource>> ValueConverters
		{
			get { return this.valueConvertersAsync; }
			private set
			{
				if (this.valueConvertersAsync == value)
					return;

				this.valueConvertersAsync = value;
				OnPropertyChanged();
			}
		}

		public Resource SelectedValueConverter
		{
			get
			{
				if (!KnownPropertiesSetup)
					return null;

				var vm = GetKnownPropertyViewModel (PropertyBinding.ConverterProperty);
				if (vm.Value == null)
					return NoValueConverter;

				return vm.Value;
			}

			set
			{
				var vm = GetKnownPropertyViewModel (PropertyBinding.ConverterProperty);
				var previous = vm.Value;
				if (previous == value)
					return;

				if (value == NoValueConverter)
					value = null;

				vm.Value = value;
				OnPropertyChanged();

				if (value == AddValueConverter) {
					RequestCreateValueConverter (previous);
				}
			}
		}

		public IReadOnlyList<EditorViewModel> FlagsProperties
		{
			get { return this.bindingFlagsProperties; }
			private set
			{
				if (this.bindingFlagsProperties == value)
					return;

				this.bindingFlagsProperties = value;
				OnPropertyChanged();
			}
		}

		public IReadOnlyList<EditorViewModel> BindingProperties
		{
			get { return this.bindingProperties; }
			private set
			{
				if (this.bindingProperties == value)
					return;

				this.bindingProperties = value;
				OnPropertyChanged();
			}
		}

		protected override void OnAddEditors (IEnumerable<EditorViewModel> editors)
		{
			base.OnAddEditors (editors);
			ValueConverters = new AsyncValue<IReadOnlyList<Resource>> (GetValueConvertersAsync());

			var flags = new List<EditorViewModel> ();
			var regular = new List<EditorViewModel> ();
			// We do not support grouped properties here
			foreach (EditorViewModel vm in Properties) {
				var propvm = vm as PropertyViewModel;
				if (propvm == null)
					continue;
				if (BindingEditor.KnownProperties.ContainsKey (propvm.Property))
					continue;

				if (propvm.Property.Type == typeof(bool))
					flags.Add (vm);
				else
					regular.Add (vm);
			}

			BindingProperties = regular;
			FlagsProperties = flags;

			BindingSources.Task.ContinueWith (t => {
				SelectedBindingSource = t.Result.FirstOrDefault ();
			}, TaskScheduler.FromCurrentSynchronizationContext ());

			OnPropertyChanged (nameof(Path));
			OnPropertyChanged (nameof(SelectedValueConverter));
			OnPropertyChanged (nameof(ShowTypeLevel));
		}

		private static readonly Resource NoValueConverter = new Resource (Resources.NoValueConverter);
		private static readonly Resource AddValueConverter = new Resource ("<" + Resources.AddValueConverterEllipsis + ">");

		private readonly PropertyVariation variations;
		private readonly IObjectEditor targetEditor;
		private readonly IPropertyInfo property;
		private readonly IBindingProvider provider;
		private readonly IEditorProvider editorProvider;
		private readonly ObservableCollectionEx<Resource> valueConverters = new ObservableCollectionEx<Resource> ();

		private TypeSelectorViewModel typeSelector;
		private ObjectTreeElement selectedObjectTreeElement;
		private PropertyTreeElement selectedPropertyElement;
		private AsyncValue<PropertyTreeRoot> propertyRoot;
		private AsyncValue<IReadOnlyList<ObjectTreeElement>> objectElementRoots;
		private AsyncValue<IReadOnlyList<Resource>> valueConvertersAsync;
		private IReadOnlyList<EditorViewModel> bindingFlagsProperties;
		private IReadOnlyList<EditorViewModel> bindingProperties;
		private string propertyDisplay;
		private AsyncValue<ILookup<ResourceSource, Resource>> resources;
		private Resource selectedResource;

		private bool KnownPropertiesSetup => ValueConverters != null;

		private IObjectEditor BindingEditor => ObjectEditors[0];

		private object BindingSourceTarget
		{
			get { return GetKnownPropertyViewModel<object> (PropertyBinding.SourceParameterProperty).Value; }
			set { GetKnownPropertyViewModel<object> (PropertyBinding.SourceParameterProperty).Value = value; }
		}

		public bool IncludeAddValueConverter { get; private set; }

		private void UpdateShowProperties ()
		{
			OnPropertyChanged (nameof (ShowResourceSelector));
			OnPropertyChanged (nameof (ShowObjectSelector));
			OnPropertyChanged (nameof (ShowTypeSelector));
		}

		private async Task<IReadOnlyList<Resource>> GetValueConvertersAsync ()
		{
			this.valueConverters.Clear ();
			this.valueConverters.Add (NoValueConverter);

			var converters = await TargetPlatform.BindingProvider.GetValueConverterResourcesAsync (this.targetEditor.Target);
			this.valueConverters.AddRange (converters);

			// Don't add the AddValueConverter resource if we are on Mac
			if (IncludeAddValueConverter) {
				this.valueConverters.Add (AddValueConverter);
			}

			return this.valueConverters;
		}

		private async void RequestUpdateSources ()
		{
			Task task = null;
			switch (SelectedBindingSource.Type) {
			case BindingSourceType.SingleObject:
			case BindingSourceType.Object:
				SelectedObjectTreeElement = null;
				ObjectElementRoots = new AsyncValue<IReadOnlyList<ObjectTreeElement>> (GetRootElementsAsync ());
				task = ObjectElementRoots.Task;
				break;
			case BindingSourceType.Type:
				TypeSelector = new TypeSelectorViewModel (new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (GetBindingSourceTypesAsync()));
				break;
			case BindingSourceType.Resource:
				SelectedResource = null;
				SourceResources = new AsyncValue<ILookup<ResourceSource, Resource>> (GetSourceResourcesAsync ());
				break;
			}

			if (task != null)
				await task;

			switch (SelectedBindingSource.Type) {
				case BindingSourceType.SingleObject:
				case BindingSourceType.Object:
					if (ObjectElementRoots.Value.Count == 1) {
						var root = ObjectElementRoots.Value[0];
						var children = await root.Children.Task;
						if (children.Count == 0)
							SelectedObjectTreeElement = root;
					}
					break;
			}

			UpdateShowProperties ();
			OnPropertyChanged (nameof (ShowLongDescription));
		}

		private void OnTypeSelectorPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
			case nameof(TypeSelector.SelectedType):
				OnPropertyChanged (nameof (CanCreateBinding));
				BindingSourceTarget = TypeSelector.SelectedType;
				RequestUpdateProperties ();
				break;
			case nameof(TypeSelector.TypeLevel):
				var vm = GetKnownPropertyViewModel (PropertyBinding.TypeLevelProperty);
				if (vm != null)
					vm.Value = TypeSelector.TypeLevel;
				break;
			}
		}

		private Task<ILookup<ResourceSource, Resource>> GetSourceResourcesAsync ()
		{
			return this.provider.GetResourcesAsync (SelectedBindingSource, Target);
		}

		private async Task<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> GetBindingSourceTypesAsync ()
		{
			var results = await this.provider.GetSourceTypesAsync (SelectedBindingSource, this.targetEditor.Target).ConfigureAwait (false);
			return results.GetTypeTree();
		}

		private async Task<IReadOnlyList<ObjectTreeElement>> GetRootElementsAsync ()
		{
			var root = await this.provider.GetRootElementsAsync (SelectedBindingSource, this.targetEditor.Target).ConfigureAwait (false);
			IObjectEditor[] editors = await Task.WhenAll (root.Select (o => this.editorProvider.GetObjectEditorAsync (o))).ConfigureAwait (false);
			return editors.Select (oe => new ObjectTreeElement (this.editorProvider, oe)).ToArray ();
		}

		private void RequestUpdateProperties ()
		{
			SelectedPropertyElement = null;

			ITypeInfo type = null;
			switch (SelectedBindingSource.Type) {
			case BindingSourceType.Type:
				type = TypeSelector.SelectedType;
				break;
			case BindingSourceType.SingleObject:
			case BindingSourceType.Object:
				type = SelectedObjectTreeElement?.Editor.TargetType;
				break;
			case BindingSourceType.Resource:
				if (SelectedResource != null)
					type = GetRealType (SelectedResource);
				break;
			default:
				throw new InvalidOperationException();
			}

			if (type == null) {
				PropertyRoot = null;
				return;
			}

			Task<PropertyTreeRoot> task = this.editorProvider.GetPropertiesForTypeAsync (type)
				.ContinueWith (t => new PropertyTreeRoot (this.editorProvider, type, t.Result), TaskScheduler.Default);

			PropertyRoot = new AsyncValue<PropertyTreeRoot> (task);
		}

		private ITypeInfo GetRealType (Resource resource)
		{
			if (resource.RepresentationType.IsPrimitive)
				return resource.RepresentationType.ToTypeInfo (isRelevant: false);
			if (this.editorProvider.KnownTypes.TryGetValue (resource.RepresentationType, out ITypeInfo type))
				return type;

			throw new InvalidOperationException();
		}

		private void UpdatePath ()
		{
			if (SelectedPropertyElement == null) {
				Path = null;
				return;
			}

			string newPath = String.Empty;
			PropertyTreeElement element = SelectedPropertyElement;
			while (element != null) {
				string sep = (newPath != String.Empty) ? ((!element.IsCollection) ? "." : "/") : String.Empty;
				newPath = element.Property.Name + sep + newPath;
				element = element.Parent;
			}

			Path = newPath;
		}

		private async void RequestCreateValueConverter (Resource previous)
		{
			var request = new CreateValueConverterEventArgs ();
			CreateValueConverterRequested?.Invoke (this, request);

			if (request.ConverterType == null || request.Name == null) {
				SynchronizationContext.Current.Post (p => SelectedValueConverter = (Resource)p, previous);
				return;
			}

			object converter = await TargetPlatform.EditorProvider.CreateObjectAsync (request.ConverterType);
			if (request.Source == null) {
				// TODO: Set directly outside of a resource
				return;
			}

			Resource resource = await TargetPlatform.ResourceProvider.CreateResourceAsync (request.Source, request.Name, converter);
			this.valueConverters.Insert (this.valueConverters.Count - 1, resource);
			SelectedValueConverter = resource;
		}

		IReadOnlyList<object> IProvidePath.GetItemParents (object item)
		{
			switch (item) {
			case PropertyTreeElement prop:
				List<object> path = new List<object> ();
				var current = prop;
				while (current != null) {
					path.Insert (0, current);
					current = current.Parent;
				}

				path.Insert (0, PropertyRoot.Value);
				return path;
			case Resource resource:
				return new object[] { resource.Source, resource };
			default:
				throw new ArgumentException();
			}
		}

		private readonly RelayCommand requestAddValueConverterCommand;
		public ICommand RequestAddValueConverterCommand => this.requestAddValueConverterCommand;

		private bool CanRequestAddValueConverter ()
		{
			return TargetPlatform.BindingProvider != null;
		}

		private void OnRequestAddValueConverter ()
		{
			SelectedValueConverter = AddValueConverter;
		}
	}
}
