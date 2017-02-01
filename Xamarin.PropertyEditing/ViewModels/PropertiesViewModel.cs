using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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
			IObjectEditor[] newEditors = null;
			IObjectEditor[] removedEditors = null;

			switch (e.Action) {
				case NotifyCollectionChangedAction.Add: {
					newEditors = await AddEditorsAsync (e);
					break;
				}

				case NotifyCollectionChangedAction.Remove:
					removedEditors = new IObjectEditor[e.OldItems.Count];
					for (int i = 0; i < e.OldItems.Count; i++) {
						removedEditors[i] = this.editors.First (oe => oe.Target == e.OldItems[i]);
						this.editors.Remove (removedEditors[i]);
					}
					break;

				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Reset: {
					removedEditors = this.editors.ToArray();
					this.editors.Clear ();

					Task<IObjectEditor>[] newEditorTasks = new Task<IObjectEditor>[SelectedObjects.Count];
					for (int i = 0; i < this.selectedObjects.Count; i++) {
						newEditorTasks[i] = EditorProvider.GetObjectEditorAsync (this.selectedObjects[i]);
					}

					newEditors = await Task.WhenAll (newEditorTasks);
					for (int i = 0; i < newEditors.Length; i++) {
						var notifier = newEditors[i].Properties as INotifyCollectionChanged;
						if (notifier != null)
							notifier.CollectionChanged -= OnObjectEditorPropertiesChanged;
					}

					this.editors.AddRange (newEditors);
					break;
				}
			}

			UpdateProperties (removedEditors, newEditors);
		}

		private readonly List<IObjectEditor> editors = new List<IObjectEditor> ();
		private readonly ObservableCollection<PropertyViewModel> properties = new ObservableCollection<PropertyViewModel> ();
		private readonly ObservableCollectionEx<object> selectedObjects = new ObservableCollectionEx<object> ();

		private void UpdateProperties (IObjectEditor[] removedEditors = null, IObjectEditor[] newEditors = null)
		{
			if (this.editors.Count == 0) {
				this.properties.Clear();
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
				this.properties.Add (GetViewModel (property));
			}
		}

		private async Task<IObjectEditor[]> AddEditorsAsync (NotifyCollectionChangedEventArgs e)
		{
			Task<IObjectEditor>[] newEditorTasks = new Task<IObjectEditor>[e.NewItems.Count];
			for (int i = 0; i < newEditorTasks.Length; i++) {
				newEditorTasks[i] = EditorProvider.GetObjectEditorAsync (e.NewItems[i]);
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

		private static readonly Dictionary<Type,Func<IPropertyInfo,IEnumerable<IObjectEditor>,PropertyViewModel>> ViewModelMap = new Dictionary<Type, Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel>> {
			{ typeof(string), (p,e) => new StringPropertyViewModel (p, e) },
			{ typeof(bool), (p,e) => new PropertyViewModel<bool> (p, e) },
			{ typeof(float), (p,e) => new FloatingPropertyViewModel (p, e) },
			{ typeof(double), (p,e) => new FloatingPropertyViewModel (p, e) },
			{ typeof(int), (p,e) => new IntegerPropertyViewModel (p, e) },
			{ typeof(long), (p,e) => new IntegerPropertyViewModel (p, e) }
		};
	}
}