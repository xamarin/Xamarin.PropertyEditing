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
		: NotifyingObject
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

		protected virtual void OnAddProperties (IEnumerable<PropertyViewModel> properties)
		{
		}

		protected virtual void OnRemoveProperties (IEnumerable<PropertyViewModel> properties)
		{
		}

		protected virtual void OnClearProperties()
		{
		}

		private INameableObject nameable;
		private bool objectNameable;
		private string typeName, objectName;
		private readonly List<IObjectEditor> editors = new List<IObjectEditor> ();
		private readonly ObservableCollectionEx<PropertyViewModel> properties = new ObservableCollectionEx<PropertyViewModel> ();
		private readonly ObservableCollectionEx<object> selectedObjects = new ObservableCollectionEx<object> ();

		private void AddProperties (IEnumerable<PropertyViewModel> properties)
		{
			this.properties.AddRange (properties);
			OnAddProperties (properties);
		}

		private void RemoveProperties (IEnumerable<PropertyViewModel> properties)
		{
			this.properties.RemoveRange (properties);
			OnRemoveProperties (properties);
		}

		private async void SetObjectName (string value)
		{
			if (this.nameable == null)
				return;

			// TODO: Errors, async work
			// not sure we have to worry about async here, the name shouldn't affect other elements
			// and it is not (currently) re-queried so sending a second name should be ok
			await this.nameable.SetNameAsync (value);
			SetCurrentObjectName (value);
		}

		private void SetNameable (INameableObject nameable)
		{
			this.nameable = nameable;
			OnPropertyChanged (nameof (IsObjectNameable));
		}

		private void SetCurrentObjectName (string value)
		{
			this.objectName = value;
			OnPropertyChanged (nameof (ObjectName));
		}

		private void ClearProperties()
		{
			TypeName = null;
			SetNameable (null);
			SetCurrentObjectName (null);
			this.properties.Clear ();
			OnClearProperties ();
		}

		private void UpdateProperties (IObjectEditor[] removedEditors = null, IObjectEditor[] newEditors = null)
		{
			if (this.editors.Count == 0) {
				ClearProperties ();
				return;
			}

			Task<string> nameQuery = null;
			if (this.editors.Count == 1) {
				SetNameable (this.editors[0] as INameableObject);
				nameQuery = this.nameable?.GetNameAsync ();
			} else {
				SetNameable (null);
			}

			string newTypeName = this.editors[0].TypeName;
			var newSet = new HashSet<IPropertyInfo> (this.editors[0].Properties);
			for (int i = 1; i < this.editors.Count; i++) {
				IObjectEditor editor = this.editors[i];
				newSet.IntersectWith (editor.Properties);

				if (newTypeName != editor.TypeName)
					newTypeName = String.Format (PropertyEditing.Properties.Resources.MultipleObjectsSelected, this.editors.Count);
			}

			TypeName = newTypeName;

			List<PropertyViewModel> toRemove = new List<PropertyViewModel> ();
			foreach (PropertyViewModel vm in this.properties.ToArray ()) {
				if (!newSet.Remove (vm.Property)) {
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
				RemoveProperties (toRemove);
			if (newSet.Count > 0)
				AddProperties (newSet.Select (GetViewModel));

			string name = (this.editors.Count > 1) ? String.Format (PropertyEditing.Properties.Resources.MultipleObjectsSelected, this.editors.Count) : PropertyEditing.Properties.Resources.NoName;
			if (this.editors.Count == 1) {
				string tname = nameQuery?.Result;
				if (tname != null)
					name = tname;
			}

			SetCurrentObjectName (name);
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
			Type[] interfaces = property.GetType ().GetInterfaces ();

			Type hasPredefinedValues = interfaces.FirstOrDefault (t => t.IsGenericType && t.GetGenericTypeDefinition () == typeof(IHavePredefinedValues<>));
			if (hasPredefinedValues != null) {
				Type type = typeof(PredefinedValuesViewModel<>).MakeGenericType (hasPredefinedValues.GenericTypeArguments[0]);
				return (PropertyViewModel) Activator.CreateInstance (type, property, this.editors);
			} else if (property.Type.IsEnum) {
				Type type = typeof(EnumPropertyViewModel<>).MakeGenericType (property.Type);
				return (PropertyViewModel) Activator.CreateInstance (type, property, this.editors);
			}

			Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel> vmFactory;
			if (ViewModelMap.TryGetValue (property.Type, out vmFactory))
				return vmFactory (property, this.editors);
			
			return new StringPropertyViewModel (property, this.editors);
		}

		private Task busyTask;

		public static readonly Dictionary<Type,Func<IPropertyInfo,IEnumerable<IObjectEditor>,PropertyViewModel>> ViewModelMap = new Dictionary<Type, Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel>> {
			{ typeof(string), (p,e) => new StringPropertyViewModel (p, e) },
			{ typeof(bool), (p,e) => new PropertyViewModel<bool> (p, e) },
			{ typeof(float), (p,e) => new FloatingPropertyViewModel (p, e) },
			{ typeof(double), (p,e) => new FloatingPropertyViewModel (p, e) },
			{ typeof(int), (p,e) => new IntegerPropertyViewModel (p, e) },
			{ typeof(long), (p,e) => new IntegerPropertyViewModel (p, e) },
			{ typeof(Point), (p,e) => new PropertyViewModel<Point> (p, e) },
			{ typeof(Size), (p,e) => new PropertyViewModel<Size> (p, e) },
			{ typeof(Rectangle), (p,e) => new PropertyViewModel<Rectangle> (p, e) },
		};
	}
}
