using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class PropertiesViewModel
		: ViewModelBase
	{
		public PropertiesViewModel (IEditorProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof (provider));

			EditorProvider = provider;

			this.selectedObjects.CollectionChanged += OnSelectedObjectsChanged;
		}

		/// <remarks>Consumers should check for <see cref="INotifyCollectionChanged"/> and hook appropriately.</remarks>
		public IReadOnlyList<PropertyViewModel> Properties => this.properties;

		public ICollection<object> SelectedObjects => this.selectedObjects;

		protected IEditorProvider EditorProvider
		{
			get;
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
						IObjectEditor editor = this.editors.First (oe => oe.Target == e.OldItems[i]);
						INotifyCollectionChanged notifier = editor.Properties as INotifyCollectionChanged;
						if (notifier != null)
							notifier.CollectionChanged -= OnObjectEditorPropertiesChanged;

						removedEditors[i] = editor;
						this.editors.Remove (editor);
					}
					break;

				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Reset: {
					removedEditors = new IObjectEditor[this.editors.Count];
					for (int i = 0; i < removedEditors.Length; i++) {
						removedEditors[i] = this.editors[i];
						INotifyCollectionChanged notifier = removedEditors[i].Properties as INotifyCollectionChanged;
						if (notifier != null)
							notifier.CollectionChanged -= OnObjectEditorPropertiesChanged;
					}

					this.editors.Clear ();

					newEditors = await AddEditorsAsync (this.selectedObjects);
					this.editors.AddRange (newEditors);
					break;
				}
			}

			UpdateProperties (removedEditors, newEditors);
			tcs.SetResult (true);
		}

		private readonly List<IObjectEditor> editors = new List<IObjectEditor> ();
		private ObservableCollection<PropertyViewModel> properties = new ObservableCollection<PropertyViewModel> ();
		private readonly ObservableCollectionEx<object> selectedObjects = new ObservableCollectionEx<object> ();

		string filterText;
		PropertyArrangeMode filterMode;

		private void UpdateProperties (IObjectEditor[] removedEditors = null, IObjectEditor[] newEditors = null)
		{
			if (this.editors.Count == 0) {
				this.properties.Clear ();
				return;
			}

			var newSet = new HashSet<IPropertyInfo> (this.editors[0].Properties);
			for (int i = 1; i < this.editors.Count; i++) {
				newSet.IntersectWith (this.editors[i].Properties);
			}

			foreach (PropertyViewModel vm in this.properties.ToArray ()) {
				if (!newSet.Remove (vm.Property)) {
					this.properties.Remove (vm);
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

			foreach (IPropertyInfo property in newSet) {
				if (string.IsNullOrEmpty (filterText)) {
					this.properties.Add (GetViewModel (property));
				}
				else {
					switch (filterMode) {
						case PropertyArrangeMode.Name:
							if (property.Name.StartsWith (filterText, StringComparison.InvariantCultureIgnoreCase)) {
								this.properties.Add (GetViewModel (property));
							}
							break;
						case PropertyArrangeMode.Category:
							if (property.Name.StartsWith (filterText, StringComparison.InvariantCultureIgnoreCase)) {
								this.properties.Add (GetViewModel (property));
							}
							break;
						case PropertyArrangeMode.ValueSource:
							break;
					}
				}
			}
		}

		private async Task<IObjectEditor[]> AddEditorsAsync (IList newItems)
		{
			Task<IObjectEditor>[] newEditorTasks = new Task<IObjectEditor>[newItems.Count];
			for (int i = 0; i < newEditorTasks.Length; i++) {
				newEditorTasks[i] = EditorProvider.GetObjectEditorAsync (newItems[i]);
			}

			IObjectEditor[] newEditors = await Task.WhenAll (newEditorTasks);
			for (int i = 0; i < newEditors.Length; i++) {
				var notifier = newEditors[i].Properties as INotifyCollectionChanged;
				if (notifier != null)
					notifier.CollectionChanged += OnObjectEditorPropertiesChanged;
			}

			this.editors.AddRange (newEditors);
			return newEditors;
		}

		private void OnObjectEditorPropertiesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateProperties();
		}

		private PropertyViewModel GetViewModel (IPropertyInfo property)
		{
			if (property.Type.IsEnum) {
				Type type = typeof(EnumPropertyViewModel<>).MakeGenericType (property.Type);
				return (PropertyViewModel) Activator.CreateInstance (type, property, this.editors);
			}

			Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel> vmFactory;
			if (ViewModelMap.TryGetValue (property.Type, out vmFactory))
				return vmFactory (property, this.editors);
			
			return new StringPropertyViewModel (property, this.editors);
		}

		internal void FilterData (string filterText, PropertyArrangeMode filterMode)
		{
            this.filterText = filterText;
			this.filterMode = filterMode;
			this.properties.Clear ();

			UpdateProperties ();
		}

		private Task busyTask;

		public static readonly Dictionary<Type,Func<IPropertyInfo,IEnumerable<IObjectEditor>,PropertyViewModel>> ViewModelMap = new Dictionary<Type, Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel>> {
			{ typeof(string), (p,e) => new StringPropertyViewModel (p, e) },
			{ typeof(bool), (p,e) => new PropertyViewModel<bool> (p, e) },
			{ typeof(float), (p,e) => new FloatingPropertyViewModel (p, e) },
			{ typeof(double), (p,e) => new FloatingPropertyViewModel (p, e) },
			{ typeof(int), (p,e) => new IntegerPropertyViewModel (p, e) },
			{ typeof(long), (p,e) => new IntegerPropertyViewModel (p, e) },
		};
	}
}
