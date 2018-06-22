using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.PropertyEditing.Properties;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class CollectionPropertyItemViewModel
		: NotifyingObject
	{
		public CollectionPropertyItemViewModel (object item, ITypeInfo targetType)
		{
			if (item == null)
				throw new ArgumentNullException (nameof(item));
			if (targetType == null)
				throw new ArgumentNullException (nameof(targetType));

			Item = item;
			TypeName = targetType.Name;
		}

		public object Item
		{
			get;
		}

		public string TypeName
		{
			get;
		}

		public int Row
		{
			get { return this.row; }
			set
			{
				if (this.row == value)
					return;

				this.row = value;
				OnPropertyChanged();
			}
		}

		private int row;
	}

	// TODO: One thing this doesn't support currently is a way of previewing the changes

	internal class CollectionPropertyViewModel
		: PropertyViewModel<IList>
	{
		public CollectionPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			if (this.cachedProvider == null)
				this.cachedProvider = new CachedEditorProvider (platform.EditorProvider);

			RequestTypes ();

			Panel = new PanelViewModel (platform.WithProvider (this.cachedProvider)) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};

			AddTargetCommand = new RelayCommand (OnAddTarget);
			RemoveTargetCommand = new RelayCommand (OnRemoveTarget, CanAffectTarget);
			MoveUpCommand = new RelayCommand (() => MoveTarget (up: true), () => CanMoveTarget (up: true));
			MoveDownCommand = new RelayCommand (() => MoveTarget (up: false), () => CanMoveTarget (up: false));
			CommitCommand = new RelayCommand (OnCommitCommand);
			CancelCommand = new RelayCommand(RequestCurrentValueUpdate);

			this.collectionView.CollectionChanged += OnCollectionViewContentsChanged;
		}

		public event EventHandler<TypeRequestedEventArgs> TypeRequested;

		public IReadOnlyList<CollectionPropertyItemViewModel> Targets => this.collectionView;

		public IReadOnlyList<ITypeInfo> SuggestedTypes
		{
			get { return this.suggestedTypes; }
			private set
			{
				if (this.suggestedTypes == value)
					return;

				this.suggestedTypes = (ObservableCollectionEx<ITypeInfo>)value;
				OnPropertyChanged();
			}
		}

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

		public CollectionPropertyItemViewModel SelectedTarget
		{
			get { return this.selectedTarget; }
			set
			{
				if (this.selectedTarget == value)
					return;

				CollectionPropertyItemViewModel old = this.selectedTarget;
				this.selectedTarget = value;

				if (value != null) {
					if (old == null)
						Panel.SelectedObjects.Add (value.Item);
					else
						Panel.SelectedObjects.ReplaceOrAdd (old.Item, value.Item);
				} else
					Panel.SelectedObjects.Clear();

				OnPropertyChanged();
				UpdateTargetCommands ();
			}
		}

		public PanelViewModel Panel
		{
			get;
		}

		public ITypeInfo SelectedType
		{
			get { return this.selectedType; }
			set
			{
				if (this.selectedType == value)
					return;

				this.selectedType = value;
				OnPropertyChanged();

				if (value == OtherType)
					RequestOtherType ();
			}
		}

		public ICommand MoveUpCommand
		{
			get;
		}

		public ICommand MoveDownCommand
		{
			get;
		}

		public ICommand AddTargetCommand
		{
			get;
		}

		public ICommand RemoveTargetCommand
		{
			get;
		}

		public ICommand CommitCommand
		{
			get;
		}

		public ICommand CancelCommand
		{
			get;
		}

		protected override async Task UpdateCurrentValueAsync ()
		{
			await base.UpdateCurrentValueAsync ();

			if (this.cachedProvider == null)
				this.cachedProvider = new CachedEditorProvider (TargetPlatform.EditorProvider);

			this.cachedProvider.Clear();
			if (Value != null)
				this.collectionView.Reset (await GetViewsFromValueAsync());
			else
				this.collectionView.Clear();
		}

		protected override void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			base.OnEditorsChanged (sender, e);
			RequestTypes ();
		}

		// We need to grab the object editor for the children to get their "real" type name, but we don't want
		// the panel view model to have query the editor again each time it's selected, so we'll give it this
		// caching provider. Lists of this kind tend to be relatively small, but if becomes a problem we can
		// add a window to the cache.

		private class CachedEditorProvider
			: IEditorProvider
		{
			public CachedEditorProvider (IEditorProvider realProvider)
			{
				if (realProvider == null)
					throw new ArgumentNullException (nameof(realProvider));

				this.realProvider = realProvider;
			}

			public void Add (IObjectEditor editor)
			{
				this.editors.Add (editor.Target, editor);
			}

			public void Remove (object target)
			{
				this.editors.Remove (target);
			}

			public void Clear ()
			{
				this.editors.Clear();
			}

			public Task<IObjectEditor> GetObjectEditorAsync (object item)
			{
				if (this.editors.TryGetValue (item, out IObjectEditor editor))
					return Task.FromResult (editor);

				return this.realProvider.GetObjectEditorAsync (item);
			}

			public async Task<IObjectEditor> GetAndCacheEditorAsync (object item)
			{
				if (!this.editors.TryGetValue (item, out IObjectEditor editor)) {
					editor = await GetObjectEditorAsync (item);
					this.editors[item] = editor;
				}

				return editor;
			}

			public Task<IReadOnlyDictionary<Type, ITypeInfo>> GetKnownTypesAsync (IReadOnlyCollection<Type> knownTypes)
			{
				return this.realProvider.GetKnownTypesAsync (knownTypes);
			}

			public Task<object> CreateObjectAsync (ITypeInfo type)
			{
				return this.realProvider.CreateObjectAsync (type);
			}

			private readonly Dictionary<object, IObjectEditor> editors = new Dictionary<object, IObjectEditor> ();
			private readonly IEditorProvider realProvider;
		}

		private CollectionPropertyItemViewModel selectedTarget;
		private ITypeInfo selectedType;
		private AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes;
		private ObservableCollectionEx<ITypeInfo> suggestedTypes;
		private readonly ObservableCollectionEx<CollectionPropertyItemViewModel> collectionView = new ObservableCollectionEx<CollectionPropertyItemViewModel> ();
		private CachedEditorProvider cachedProvider;

		private static readonly ITypeInfo OtherType = new OtherTypeFake();

		private class OtherTypeFake
			: ITypeInfo
		{
			public IAssemblyInfo Assembly => null;
			public string NameSpace => null;
			public string Name => Resources.OtherTypeAction;
		}

		private async Task<IReadOnlyList<CollectionPropertyItemViewModel>> GetViewsFromValueAsync ()
		{
			var items = new List<CollectionPropertyItemViewModel> (Value.Count);
			for (int i = 0; i < Value.Count; i++) {
				object target = Value[i];
				IObjectEditor editor = await this.cachedProvider.GetAndCacheEditorAsync (target);
				items.Add (new CollectionPropertyItemViewModel (target, editor.TargetType) {
					Row = i
				});
			}

			return items;
		}

		private async void RequestTypes ()
		{
			if (Property == null)
				return;

			var types = Editors.GetCommonAssignableTypes (Property, childTypes: true);
			var assignableTypesTask = types.ContinueWith (t => t.Result.GetTypeTree (), TaskScheduler.Default);
			AssignableTypes = new AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> (assignableTypesTask);

			var results = await types;
			var suggested = new ObservableCollectionEx<ITypeInfo> (results.SuggestedTypes);
			if (results.AssignableTypes.Count > suggested.Count)
				suggested.Add (OtherType);

			SuggestedTypes = suggested;
			SelectedType = (results.SuggestedTypes.Count > 0) ? results.SuggestedTypes[0] : null;
		}

		private Task PushValueAsync ()
		{
			object[] snapshot = this.collectionView.Select (vm => vm.Item).ToArray ();
			return SetValueAsync (new ValueInfo<IList> {
				Value = snapshot,
				Source = ValueSource.Local
			});
		}

		private async void OnCommitCommand ()
		{
			await PushValueAsync ();
		}

		private void RequestOtherType ()
		{
			var args = new TypeRequestedEventArgs();
			TypeRequested?.Invoke (this, args);

			if (args.SelectedType == null) {
				// We know we have OtherType because we're in this method, and we know its at the bottom
				SelectedType = SuggestedTypes.Count > 1 ? SuggestedTypes[0] : null;
				return;
			}

			if (!this.suggestedTypes.Contains (args.SelectedType)) {
				this.suggestedTypes.Insert (0, args.SelectedType);
			}

			SelectedType = args.SelectedType;
		}

		private void OnCollectionViewContentsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTargetCommands ();
		}

		private bool CanAffectTarget ()
		{
			return (SelectedTarget != null);
		}

		private bool CanMoveTarget (bool up)
		{
			int index = this.collectionView.IndexOf (SelectedTarget);
			if (index == -1)
				return false;

			return (up) ? (index > 0) : (index < this.collectionView.Count - 1);
		}

		private void ReIndex ()
		{
			for (int i = 0; i < this.collectionView.Count; i++) {
				this.collectionView[i].Row = i;
			}
		}

		private void MoveTarget (bool up)
		{
			int index = this.collectionView.IndexOf (SelectedTarget);
			this.collectionView.Move (index, index + ((up) ? -1 : 1));

			ReIndex();
		}

		private async void OnAddTarget ()
		{
			object target = await TargetPlatform.EditorProvider.CreateObjectAsync (SelectedType);
			IObjectEditor editor = await TargetPlatform.EditorProvider.GetObjectEditorAsync (target);
			this.cachedProvider.Add (editor);

			var vm = new CollectionPropertyItemViewModel (target, editor.TargetType);

			if (SelectedTarget != null) {
				this.collectionView.Insert (SelectedTarget.Row + 1, vm);
			} else {
				this.collectionView.Add (vm);
			}

			ReIndex();
			SelectedTarget = vm;
		}

		private void OnRemoveTarget ()
		{
			int index = Math.Max (0, this.collectionView.IndexOf (SelectedTarget) - 1);
			this.cachedProvider.Remove (SelectedTarget.Item);
			this.collectionView.Remove (SelectedTarget);

			ReIndex();

			SelectedTarget = (this.collectionView.Count > 0) ? this.collectionView[index] : null;
		}

		private void UpdateTargetCommands ()
		{
			((RelayCommand)RemoveTargetCommand).ChangeCanExecute();
			((RelayCommand)MoveUpCommand).ChangeCanExecute();
			((RelayCommand)MoveDownCommand).ChangeCanExecute();
		}
	}
}
