using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cadenza.Collections;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class PropertiesViewModel
		: NotifyingObject, INotifyDataErrorInfo
	{
		public PropertiesViewModel (TargetPlatform targetPlatform)
		{
			if (targetPlatform == null)
				throw new ArgumentNullException (nameof(targetPlatform));

			TargetPlatform = targetPlatform;

			this.selectedObjects.CollectionChanged += OnSelectedObjectsChanged;
		}

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		/// <remarks>Consumers should check for <see cref="INotifyCollectionChanged"/> and hook appropriately.</remarks>
		public IReadOnlyList<EditorViewModel> Properties => this.editors;

		public IReadOnlyList<EventViewModel> Events => this.events;

		public ICollection<object> SelectedObjects => this.selectedObjects;

		public string TypeName
		{
			get { return this.typeName; }
			private set
			{
				if (this.typeName == value)
					return;

				this.typeName = value;
				OnPropertyChanged ();
			}
		}

		public bool IsObjectNameable => this.nameable != null;

		public bool IsObjectNameReadOnly
		{
			get { return this.nameReadOnly; }
			private set
			{
				if (this.nameReadOnly == value)
					return;

				this.nameReadOnly = value;
				OnPropertyChanged ();
			}
		}

		public string ObjectName
		{
			get { return this.objectName; }
			set
			{
				if (this.objectName == value)
					return;

				SetObjectName (value);
			}
		}

		public bool EventsEnabled
		{
			get { return this.eventsEnabled; }
			private set
			{
				if (this.eventsEnabled == value)
					return;

				this.eventsEnabled = value;
				OnPropertyChanged ();
			}
		}

		public TargetPlatform TargetPlatform
		{
			get;
		}

		public bool HasErrors => this.errors.IsValueCreated && this.errors.Value.Count > 0;

		public IEnumerable GetErrors (string propertyName)
		{
			if (!this.errors.IsValueCreated)
				return Enumerable.Empty<string> ();

			string error;
			if (this.errors.Value.TryGetValue (propertyName, out error))
				return new[] { error };

			return Enumerable.Empty<string> ();
		}

		public PropertyViewModel<T> GetKnownPropertyViewModel<T> (KnownProperty<T> property)
		{
			if (property == null)
				throw new ArgumentNullException (nameof (property));
			if (this.knownEditors == null)
				throw new InvalidOperationException ("Querying for known properties before they've been setup");
			if (!this.knownEditors.TryGetValue (property, out EditorViewModel model))
				throw new KeyNotFoundException ();

			var vm = model as PropertyViewModel<T>;
			if (vm == null)
				throw new InvalidOperationException ("KnownProperty doesn't return expected property view model type");

			return vm;
		}

		protected IReadOnlyList<IObjectEditor> ObjectEditors => this.objEditors;

		/// <param name="newError">The error message or <c>null</c> to clear the error.</param>
		protected void SetError (string property, string newError)
		{
			if (this.errors.IsValueCreated) {
				string prevError;
				if (this.errors.Value.TryGetValue (property, out prevError)) {
					if (prevError == newError)
						return;
				}
			}

			if (newError == null)
				this.errors.Value.Remove (property);
			else
				this.errors.Value[property] = newError;

			OnErrorsChanged (new DataErrorsChangedEventArgs (property));
		}

		// TODO: Consider having the property hooks at the top level and a map of IPropertyInfo -> PropertyViewModel
		// the hash lookup would be likely faster than doing a property info compare in every property and would
		// reduce the number event attach/detatches

		protected virtual async void OnSelectedObjectsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			var tcs = new TaskCompletionSource<bool> ();
			var existingTask = Interlocked.Exchange (ref this.busyTask, tcs.Task);
			if (existingTask != null)
				await existingTask;

			IObjectEditor[] newEditors = null;
			IObjectEditor[] removedEditors = null;

			switch (e.Action) {
				case NotifyCollectionChangedAction.Add: {
					newEditors = await AddEditorsAsync (e.NewItems);
					break;
				}

				case NotifyCollectionChangedAction.Remove:
					removedEditors = new IObjectEditor[e.OldItems.Count];
					for (int i = 0; i < e.OldItems.Count; i++) {
						IObjectEditor editor = this.objEditors.First (oe => oe?.Target == e.OldItems[i]);
						INotifyCollectionChanged notifier = editor.Properties as INotifyCollectionChanged;
						if (notifier != null)
							notifier.CollectionChanged -= OnObjectEditorPropertiesChanged;

						removedEditors[i] = editor;
						this.objEditors.Remove (editor);
					}
					break;

				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Reset: {
					removedEditors = new IObjectEditor[this.objEditors.Count];
					for (int i = 0; i < removedEditors.Length; i++) {
						removedEditors[i] = this.objEditors[i];
						INotifyCollectionChanged notifier = removedEditors[i]?.Properties as INotifyCollectionChanged;
						if (notifier != null)
							notifier.CollectionChanged -= OnObjectEditorPropertiesChanged;
					}

					this.objEditors.Clear ();

					newEditors = await AddEditorsAsync (this.selectedObjects);
					break;
				}
			}

			await UpdateMembersAsync (removedEditors, newEditors);
			tcs.SetResult (true);
		}

		protected virtual void OnAddEditors (IEnumerable<EditorViewModel> editors)
		{
		}

		protected virtual void OnRemoveEditors (IEnumerable<EditorViewModel> editors)
		{
		}

		protected virtual void OnClearProperties()
		{
		}

		private INameableObject nameable;
		private bool nameReadOnly;
		private bool eventsEnabled;
		private string typeName, objectName;
		private BidirectionalDictionary<KnownProperty, EditorViewModel> knownEditors;
		private readonly List<IObjectEditor> objEditors = new List<IObjectEditor> ();
		private readonly ObservableCollectionEx<EditorViewModel> editors = new ObservableCollectionEx<EditorViewModel> ();
		private readonly ObservableCollectionEx<object> selectedObjects = new ObservableCollectionEx<object> ();
		private readonly ObservableCollectionEx<EventViewModel> events = new ObservableCollectionEx<EventViewModel> (); 
		private readonly Lazy<Dictionary<string, string>> errors = new Lazy<Dictionary<string, string>>();

		private void OnErrorsChanged (DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke (this, e);
		}

		private async void OnVariationsChanged (object sender, EventArgs e)
		{
			using (await AsyncWork.RequestAsyncWork (this)) {
				PropertyViewModel pvm = (PropertyViewModel) sender;
				var variations = (await GetVariationsAsync (pvm.Property)).SelectMany (vs => vs).Distinct ();
				var properties = this.editors
					.OfType<PropertyViewModel> ()
					.Where (evm => Equals (evm.Property, pvm.Property) && evm.Variation != null)
					.ToDictionary (evm => evm.Variation);

				List<PropertyViewModel> toAdd = new List<PropertyViewModel> ();
				foreach (PropertyVariation variation in variations) {
					if (!properties.Remove (variation)) {
						toAdd.Add (GetViewModel (pvm.Property, variation));
					}
				}

				if (properties.Count > 0) {
					var toRemove = new List<PropertyViewModel> ();
					foreach (var kvp in properties) {
						toRemove.Add (kvp.Value);
					}

					RemoveProperties (toRemove);
				}

				if (toAdd.Count > 0)
					AddProperties (toAdd);
			}
		}

		private void AddProperties (IReadOnlyList<EditorViewModel> newEditors)
		{
			if (this.knownEditors != null) {
				// Only properties common across obj editors will be listed, so knowns should also be common
				var knownProperties = newEditors[0].Editors.First().KnownProperties;
				if (knownProperties != null && knownProperties.Count > 0) {
					foreach (var editorvm in newEditors) {
						var prop = editorvm as PropertyViewModel;
						if (prop == null)
							continue;

						if (knownProperties.TryGetValue (prop.Property, out KnownProperty known)) {
							this.knownEditors[known] = editorvm;
						}
					}
				}
			}

			this.editors.AddRange (newEditors);
			OnAddEditors (newEditors);
		}

		private void RemoveProperties (IReadOnlyList<EditorViewModel> oldEditors)
		{
			if (this.knownEditors != null) {
				foreach (EditorViewModel old in oldEditors) {
					this.knownEditors.Inverse.Remove (old);
				}
			}

			this.editors.RemoveRange (oldEditors);
			OnRemoveEditors (oldEditors);
		}

		private async void SetObjectName (string value)
		{
			if (this.nameable == null)
				return;

			try {
				await this.nameable.SetNameAsync (value);
			} catch (Exception ex) {
				AggregateException aggregate = ex as AggregateException;
				if (aggregate != null) {
					aggregate = aggregate.Flatten ();
					ex = aggregate.InnerExceptions[0];
				}

				SetError (nameof(ObjectName), ex.ToString());
			} finally {
				SetCurrentObjectName (value, isReadonly: false);
			}
		}

		private void SetNameable (INameableObject nameable)
		{
			this.nameable = nameable;
			OnPropertyChanged (nameof (IsObjectNameable));
		}

		private void SetCurrentObjectName (string value, bool isReadonly)
		{
			IsObjectNameReadOnly = isReadonly;
			this.objectName = value;
			OnPropertyChanged (nameof (ObjectName));
		}

		private void ClearMembers()
		{
			TypeName = null;
			SetNameable (null);
			SetCurrentObjectName (null, isReadonly: true);
			this.editors.Clear ();
			this.events.Clear ();
			OnClearProperties ();
		}

		private async Task UpdateMembersAsync (IObjectEditor[] removedEditors = null, IObjectEditor[] newEditors = null)
		{
			if (this.objEditors.Count == 0) {
				ClearMembers ();
				return;
			}

			IObjectEditor editor = this.objEditors[0];

			Task<string> nameQuery = null;
			INameableObject firstNameable = editor as INameableObject;
			if (this.objEditors.Count == 1) {
				nameQuery = firstNameable?.GetNameAsync ();
			}

			IObjectEventEditor events = editor as IObjectEventEditor;
			var newEventSet = new HashSet<IEventInfo> (events?.Events ?? Enumerable.Empty<IEventInfo> ());

			bool knownProperties = (editor?.KnownProperties?.Count ?? 0) > 0;
			string newTypeName = editor?.TargetType.Name;
			var newPropertySet = new HashSet<IPropertyInfo> (editor?.Properties ?? Enumerable.Empty<IPropertyInfo>());
			for (int i = 1; i < this.objEditors.Count; i++) {
				editor = this.objEditors[i];
				if (editor == null)
					continue;

				newPropertySet.IntersectWith (editor.Properties);

				if (editor is IObjectEventEditor) {
					events = (IObjectEventEditor)editor;
					newEventSet.IntersectWith (events.Events);
				}

				if (firstNameable == null)
					firstNameable = editor as INameableObject;

				if (newTypeName != editor.TargetType.Name)
					newTypeName = String.Format (PropertyEditing.Properties.Resources.MultipleTypesSelected, this.objEditors.Count);

				if (!knownProperties)
					knownProperties = (editor.KnownProperties?.Count ?? 0) > 0;
			}

			TypeName = newTypeName;

			if (knownProperties && this.knownEditors == null)
				this.knownEditors = new BidirectionalDictionary<KnownProperty, EditorViewModel> ();

			await UpdatePropertiesAsync (newPropertySet, removedEditors, newEditors);

			EventsEnabled = events != null;
			UpdateEvents (newEventSet, removedEditors, newEditors);

			string name = (this.objEditors.Count > 1) ? String.Format (PropertyEditing.Properties.Resources.MultipleObjectsSelected, this.objEditors.Count) : PropertyEditing.Properties.Resources.NoName;
			if (this.objEditors.Count == 1) {
				string tname = nameQuery?.Result;
				if (tname != null)
					name = tname;
			}

			SetNameable (firstNameable);
			SetCurrentObjectName (name, this.objEditors.Count > 1);
		}

		private void UpdateEvents (HashSet<IEventInfo> newSet, IObjectEditor[] removedEditors = null, IObjectEditor[] newEditors = null)
		{
			if (this.objEditors.Count > 1) {
				this.events.Clear ();
				return;
			}

			var toRemove = new List<EventViewModel> ();
			foreach (EventViewModel vm in this.events.ToArray ()) {
				if (!newSet.Remove (vm.Event)) {
					toRemove.Add (vm);
					vm.Editors.Clear ();
					continue;
				}

				if (removedEditors != null) {
					for (int i = 0; i < removedEditors.Length; i++)
						vm.Editors.Remove (removedEditors[i]);
				}

				if (newEditors != null) {
					for (int i = 0; i < newEditors.Length; i++)
						vm.Editors.Add (newEditors[i]);
				}
			}

			if (toRemove.Count > 0)
				this.events.RemoveRange (toRemove);
			if (newSet.Count > 0) {
				this.events.Reset (this.events.Concat (newSet.Select (i => new EventViewModel (TargetPlatform, i, this.objEditors))).OrderBy (e => e.Event.Name).ToArray());
			}
		}

		private async Task UpdatePropertiesAsync (HashSet<IPropertyInfo> newSet, IObjectEditor[] removedEditors = null, IObjectEditor[] newEditors = null)
		{
			Dictionary<IPropertyInfo, Dictionary<PropertyVariation, PropertyViewModel>> variations = null;
			List<PropertyViewModel> toRemove = new List<PropertyViewModel> ();
			foreach (PropertyViewModel vm in this.editors.ToArray ()) {
				if (!newSet.Contains (vm.Property)) {
					toRemove.Add (vm);
					vm.Editors.Clear ();
					continue;
				}

				if (!vm.HasVariations) {
					newSet.Remove (vm.Property);
				} else if (vm.Variation != null) {
					if (variations == null)
						variations = new Dictionary<IPropertyInfo, Dictionary<PropertyVariation, PropertyViewModel>> ();
					if (!variations.TryGetValue (vm.Property, out Dictionary<PropertyVariation, PropertyViewModel> variantVms))
						variations[vm.Property] = variantVms = new Dictionary<PropertyVariation, PropertyViewModel> ();

					variantVms.Add (vm.Variation, vm);
				}

				if (removedEditors != null) {
					for (int i = 0; i < removedEditors.Length; i++)
						vm.Editors.Remove (removedEditors[i]);
				}

				if (newEditors != null) {
					for (int i = 0; i < newEditors.Length; i++)
						vm.Editors.Add (newEditors[i]);
				}
			}

			if (toRemove.Count > 0)
				RemoveProperties (toRemove);
			if (newSet.Count > 0) {
				toRemove = new List<PropertyViewModel> ();

				List<EditorViewModel> newVms = new List<EditorViewModel> ();
				foreach (IPropertyInfo property in newSet) {
					if (variations != null && variations.TryGetValue (property, out Dictionary<PropertyVariation, PropertyViewModel> propertyVariations)) {
						var setVariations = (await GetVariationsAsync (property)).SelectMany (vs => vs).Distinct ();
						foreach (PropertyVariation variation in setVariations) {
							if (propertyVariations.Remove (variation))
								continue;

							newVms.Add (GetViewModel (property, variation));
						}

						foreach (var kvp in propertyVariations) {
							toRemove.Add (kvp.Value);
						}
					} else if (property.HasVariations()) {
						newVms.AddRange (await GetViewModelsAsync (property));
					} else {
						newVms.Add (GetViewModel (property));
					}
				}

				if (toRemove.Count > 0) {
					RemoveProperties (toRemove);
				}

				AddProperties (newVms);
			}
		}

		private async Task<IObjectEditor[]> AddEditorsAsync (IList newItems)
		{
			Task<IObjectEditor>[] newEditorTasks = new Task<IObjectEditor>[newItems.Count];
			for (int i = 0; i < newEditorTasks.Length; i++) {
				newEditorTasks[i] = TargetPlatform.EditorProvider.GetObjectEditorAsync (newItems[i]);
			}

			IObjectEditor[] newEditors = await Task.WhenAll (newEditorTasks);
			for (int i = 0; i < newEditors.Length; i++) {
				IObjectEditor editor = newEditors[i];
				if (editor == null)
					continue;

				var notifier = editor.Properties as INotifyCollectionChanged;
				if (notifier != null)
					notifier.CollectionChanged += OnObjectEditorPropertiesChanged;
			}

			this.objEditors.AddRange (newEditors);
			return newEditors;
		}

		private async void OnObjectEditorPropertiesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			var tcs = new TaskCompletionSource<bool> ();
			var existingTask = Interlocked.Exchange (ref this.busyTask, tcs.Task);
			if (existingTask != null)
				await existingTask;

			await UpdateMembersAsync ();

			tcs.SetResult (true);
		}

		private Task<IReadOnlyCollection<PropertyVariation>[]> GetVariationsAsync (IPropertyInfo property)
		{
			var variantTasks = new List<Task<IReadOnlyCollection<PropertyVariation>>> (ObjectEditors.Count);
			for (int i = 0; i < ObjectEditors.Count; i++) {
				variantTasks.Add (ObjectEditors[i].GetPropertyVariantsAsync (property));
			}

			return Task.WhenAll (variantTasks);
		}

		private async Task<IReadOnlyList<PropertyViewModel>> GetViewModelsAsync (IPropertyInfo property, IEnumerable<PropertyVariation> variantsToSkip = null)
		{
			PropertyViewModel baseVm = GetViewModel (property);
			List<PropertyViewModel> vms = new List<PropertyViewModel> ();
			vms.Add (baseVm);

			HashSet<PropertyVariation> skipped =
				(variantsToSkip != null) ? new HashSet<PropertyVariation> (variantsToSkip) : null;

			if (baseVm.HasVariations) {
				using (await AsyncWork.RequestAsyncWork (this)) {
					var variants = await GetVariationsAsync (property);
					for (int i = 0; i < variants.Length; i++) {
						foreach (PropertyVariation variant in variants[i]) {
							if (skipped != null && skipped.Contains (variant))
								continue;

							vms.Add (GetViewModel (property, variant));
						}
					}
				}
			}

			return vms;
		}

		private PropertyViewModel GetViewModel (IPropertyInfo property, PropertyVariation variant = null)
		{
			PropertyViewModel vm;
			Type[] interfaces = property.GetType ().GetInterfaces ();

			Type hasPredefinedValues = interfaces.FirstOrDefault (t => t.IsGenericType && t.GetGenericTypeDefinition () == typeof(IHavePredefinedValues<>));
			if (hasPredefinedValues != null) {
				bool combinable = (bool) hasPredefinedValues.GetProperty (nameof(IHavePredefinedValues<bool>.IsValueCombinable)).GetValue (property);
				Type type = combinable
					? typeof(CombinablePropertyViewModel<>).MakeGenericType (hasPredefinedValues.GenericTypeArguments[0])
					: typeof(PredefinedValuesViewModel<>).MakeGenericType (hasPredefinedValues.GenericTypeArguments[0]);

				vm = (PropertyViewModel) Activator.CreateInstance (type, TargetPlatform, property, this.objEditors, variant);
			} else if (ViewModelMap.TryGetValue (property.Type, out var vmFactory))
				vm = vmFactory (TargetPlatform, property, this.objEditors, variant);
			else
				vm = new StringPropertyViewModel (TargetPlatform, property, this.objEditors, variant);

			vm.VariationsChanged += OnVariationsChanged;
			return vm;
		}

		private Task busyTask;

		protected internal static AsyncWorkQueue AsyncWork
		{
			get;
		} = new AsyncWorkQueue();

		private static readonly Dictionary<Type, Func<TargetPlatform, IPropertyInfo, IEnumerable<IObjectEditor>, PropertyVariation, PropertyViewModel>> ViewModelMap = new Dictionary<Type, Func<TargetPlatform, IPropertyInfo, IEnumerable<IObjectEditor>, PropertyVariation, PropertyViewModel>> {
			{ typeof(char), (tp,p,e,v) => new PropertyViewModel<char> (tp, p, e, v) },
			{ typeof(DateTime), (tp,p,e,v) => new PropertyViewModel<DateTime> (tp, p, e, v) },
			{ typeof(string), (tp,p,e,v) => new StringPropertyViewModel (tp, p, e, v) },
			{ typeof(bool), (tp,p,e,v) => new PropertyViewModel<bool?> (tp, p, e, v) },
			{ typeof(float), (tp,p,e,v) => new NumericPropertyViewModel<float?> (tp, p, e, v) },
			{ typeof(double), (tp,p,e,v) => new NumericPropertyViewModel<double?> (tp, p, e, v) },
			{ typeof(int), (tp,p,e,v) => new NumericPropertyViewModel<int?> (tp, p, e, v) },
			{ typeof(long), (tp,p,e,v) => new NumericPropertyViewModel<long?> (tp, p, e, v) },
			{ typeof(TimeSpan), (tp,p,e,v) => new PropertyViewModel<TimeSpan> (tp, p, e, v) },
			{ typeof(CommonSolidBrush), (tp,p,e,v) => new BrushPropertyViewModel (tp, p, e, v, new[] {CommonBrushType.NoBrush, CommonBrushType.Solid, CommonBrushType.MaterialDesign, CommonBrushType.Resource }) },
			{ typeof(CommonColor), (tp,p,e,v) => new BrushPropertyViewModel (tp, p, e, v, new[] {CommonBrushType.NoBrush, CommonBrushType.Solid, CommonBrushType.MaterialDesign, CommonBrushType.Resource }) },
			{ typeof(CommonBrush), (tp,p,e,v) => new BrushPropertyViewModel (tp, p, e, v) },
			{ typeof(CommonPoint), (tp,p,e,v) => new PointPropertyViewModel (tp, p, e, v) },
			{ typeof(CommonSize), (tp,p,e,v) => new SizePropertyViewModel (tp, p, e, v) },
			{ typeof(CommonRectangle), (tp,p,e,v) => new RectanglePropertyViewModel (tp, p, e, v) },
			{ typeof(CommonThickness), (tp,p,e,v) => new ThicknessPropertyViewModel (tp, p, e, v) },
			{ typeof(IList), (tp,p,e,v) => new CollectionPropertyViewModel (tp, p ,e, v) },
			{ typeof(BindingSource), (tp,p,e,v) => new PropertyViewModel<BindingSource> (tp, p, e, v) },
			{ typeof(Resource), (tp,p,e,v) => new PropertyViewModel<Resource> (tp, p, e, v) },
			{ typeof(FilePath), (tp,p,e,v) => new PropertyViewModel<FilePath> (tp, p, e, v) },
			{ typeof(object), (tp,p,e,v) => new ObjectPropertyViewModel (tp, p, e, v) },
			{ typeof(CommonRatio), (tp, p, e, v) => new RatioViewModel (tp, p, e, v) },
		};
	}
}
