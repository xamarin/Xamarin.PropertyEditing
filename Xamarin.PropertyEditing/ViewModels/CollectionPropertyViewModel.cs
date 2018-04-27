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
	internal class CollectionPropertyViewModel
		: PropertyViewModel<IList>
	{
		public CollectionPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors)
			: base (platform, property, editors)
		{
			RequestTypes ();

			Panel = new PanelViewModel (platform) {
				ArrangeMode = PropertyArrangeMode.Category,
				AutoExpand = true
			};

			AddTargetCommand = new RelayCommand (OnAddTarget);
			RemoveTargetCommand = new RelayCommand (OnRemoveTarget, CanAffectTarget);
			MoveUpCommand = new RelayCommand (() => MoveTarget (up: true), () => CanMoveTarget (up: true));
			MoveDownCommand = new RelayCommand (() => MoveTarget (up: false), () => CanMoveTarget (up: false));
			CommitCommand = new RelayCommand (OnCommitCommand);

			this.collectionView.CollectionChanged += OnCollectionViewContentsChanged;
		}

		public event EventHandler<TypeRequestedEventArgs> TypeRequested;

		public IReadOnlyList<object> Targets => this.collectionView;

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

		public object SelectedTarget
		{
			get { return this.selectedTarget; }
			set
			{
				if (this.selectedTarget == value)
					return;

				object old = this.selectedTarget;
				this.selectedTarget = value;

				if (value != null)
					Panel.SelectedObjects.ReplaceOrAdd (old, value);
				else
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

		protected override async Task UpdateCurrentValueAsync ()
		{
			await base.UpdateCurrentValueAsync ();

			if (Value != null)
				this.collectionView.Reset (Value.Cast<object>());
			else
				this.collectionView.Clear();
		}

		protected override void OnEditorsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			base.OnEditorsChanged (sender, e);
			RequestTypes ();
		}

		private object selectedTarget;
		private ITypeInfo selectedType;
		private AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes;
		private ObservableCollectionEx<ITypeInfo> suggestedTypes;
		private readonly ObservableCollectionEx<object> collectionView = new ObservableCollectionEx<object> ();

		private static readonly ITypeInfo OtherType = new OtherTypeFake();

		private class OtherTypeFake
			: ITypeInfo
		{
			public IAssemblyInfo Assembly => null;
			public string NameSpace => null;
			public string Name => Resources.OtherTypeAction;
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
			object[] snapshot = this.collectionView.ToArray ();
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

		private void MoveTarget (bool up)
		{
			int index = this.collectionView.IndexOf (SelectedTarget);
			this.collectionView.Move (index, index + ((up) ? -1 : 1));
		}

		private async void OnAddTarget ()
		{
			object target = await TargetPlatform.EditorProvider.CreateObjectAsync (SelectedType);
			this.collectionView.Add (target);
			SelectedTarget = target;
		}

		private void OnRemoveTarget ()
		{
			int index = Math.Max (0, this.collectionView.IndexOf (SelectedTarget) - 1);
			this.collectionView.Remove (SelectedTarget);

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
