using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cadenza.Collections;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PanelGroupViewModel
		: NotifyingObject
	{
		public PanelGroupViewModel (TargetPlatform targetPlatform, string category, IEnumerable<EditorViewModel> editors, bool separateUncommon = true)
		{
			if (targetPlatform == null)
				throw new ArgumentNullException (nameof(targetPlatform));
			if (editors == null)
				throw new ArgumentNullException (nameof(editors));

			this.separateUncommon = separateUncommon;
			this.targetPlatform = targetPlatform;
			Category = category;
			AddCore (editors);
		}

		public string Category
		{
			get;
		}

		public IReadOnlyList<EditorViewModel> Editors => this.editors;

		public IReadOnlyList<EditorViewModel> UncommonEditors => this.uncommonEditors;

		public bool HasChildElements => Editors.Count > 0 || HasUncommonElements;

		public bool HasUncommonElements => UncommonEditors.Count > 0;

		public bool UncommonShown
		{
			get;
			set;
		}

		public void Add (IEnumerable<EditorViewModel> editors)
		{
			AddCore (editors);
		}

		public void Add (EditorViewModel editor)
		{
			AddCore (editor);
		}

		public void Replace (EditorViewModel oldEditor, EditorViewModel newEditor)
		{
			if (oldEditor == null)
				throw new ArgumentNullException (nameof(oldEditor));
			if (newEditor == null)
				throw new ArgumentNullException (nameof(newEditor));

			var list = GetListCore (oldEditor);
			if (list != GetListCore (newEditor)) {
				Remove (oldEditor);
				Add (newEditor);
				return;
			}

			int i = list.IndexOf (oldEditor);
			list[i] = newEditor;
		}

		public bool Remove (EditorViewModel editor)
		{
			if (editor == null)
				throw new ArgumentNullException (nameof(editor));

			var list = GetListCore (editor);
			if (editor is PropertyViewModel pvm && this.targetPlatform.GroupedTypes != null && this.targetPlatform.GroupedTypes.TryGetValue (pvm.Property.Type, out string groupName)) {
				var group = list.OfType<PropertyGroupViewModel> ().FirstOrDefault (gvm => gvm.Category == groupName);
				if (group != null) {
					bool found = group.Remove (pvm);
					if (!group.HasChildElements)
						list.Remove (group);

					return found;
				}
			}

			bool result = list.Remove (editor);
			if (result) {
				OnPropertyChanged (nameof(HasChildElements));
				OnPropertyChanged (nameof (HasUncommonElements));
			}

			return result;
		}

		public bool GetIsExpanded (PropertyArrangeMode mode)
		{
			if (this.isExpanded == null)
				return false;

			this.isExpanded.TryGetValue (mode, out bool expanded);
			return expanded;
		}

		public void SetIsExpanded (PropertyArrangeMode mode, bool expanded)
		{
			if (this.isExpanded == null) {
				if (!expanded)
					return;

				this.isExpanded = new Dictionary<PropertyArrangeMode, bool> ();
			}

			this.isExpanded[mode] = expanded;
		}

		public IReadOnlyList<EditorViewModel> GetList (EditorViewModel evm)
		{
			if (evm == null)
				throw new ArgumentNullException (nameof(evm));

			return (IReadOnlyList<EditorViewModel>)GetListCore (evm);
		}

		private Dictionary<PropertyArrangeMode, bool> isExpanded;
		private readonly ObservableCollectionEx<EditorViewModel> editors = new ObservableCollectionEx<EditorViewModel> ();
		private readonly ObservableCollectionEx<EditorViewModel> uncommonEditors = new ObservableCollectionEx<EditorViewModel> ();
		private readonly TargetPlatform targetPlatform;
		private readonly bool separateUncommon;

		private void AddCore (IEnumerable<EditorViewModel> editors)
		{
			if (editors == null)
				throw new ArgumentNullException (nameof (editors));

			foreach (EditorViewModel evm in editors)
				AddCore (evm);
		}

		private void AddCore (EditorViewModel editor)
		{
			if (editor == null)
				throw new ArgumentNullException (nameof (editor));

			var list = GetListCore (editor);
			if (editor is PropertyViewModel pvm && this.targetPlatform.GroupedTypes != null && this.targetPlatform.GroupedTypes.TryGetValue (pvm.Property.Type, out string groupName)) {
				var group = list.OfType<PropertyGroupViewModel> ().FirstOrDefault (gvm => gvm.Category == groupName);
				if (group != null)
					group.Add (pvm);
				else
					list.Add (editor);
			} else
				list.Add (editor);

			OnPropertyChanged (nameof(HasChildElements));
			OnPropertyChanged (nameof(HasUncommonElements));
		}

		private IList<EditorViewModel> GetListCore (EditorViewModel evm)
		{
			if (this.separateUncommon && evm is PropertyViewModel pvm)
				return pvm.Property.IsUncommon ? this.uncommonEditors : this.editors;
			else
				return this.editors;
		}
	}

	internal class PanelViewModel
		: PropertiesViewModel, IFilterable
	{
		public PanelViewModel (TargetPlatform targetPlatform)
			: base (targetPlatform)
		{
			if (targetPlatform == null)
				throw new ArgumentNullException (nameof(targetPlatform));

			var modes = new List<ArrangeModeViewModel> ();
			if (targetPlatform.ArrangeModes == null || targetPlatform.ArrangeModes.Count == 0)
				modes.Add (new ArrangeModeViewModel (PropertyArrangeMode.Name, this));
			else {
				for (int i = 0; i < targetPlatform.ArrangeModes.Count; i++)
					modes.Add (new ArrangeModeViewModel (targetPlatform.ArrangeModes[i], this));
			}

			ArrangeModes = modes;
			AutoExpand = targetPlatform.AutoExpandAll;
		}

		public event EventHandler ArrangedPropertiesChanged;

		public IReadOnlyList<PanelGroupViewModel> ArrangedEditors => (IReadOnlyList<PanelGroupViewModel>)this.arranged.Values;

		/// <summary>
		/// Gets or sets whether all categories should automatically expand.
		/// </summary>
		public bool AutoExpand
		{
			get { return this.autoExpand; }
			set
			{
				if (this.autoExpand == value)
					return;

				this.autoExpand = value;
				UpdateExpanded (value);
				OnPropertyChanged();
			}
		}

		public bool HasChildElements => (this.arranged.Count > 0);

		public bool IsFiltering => !String.IsNullOrWhiteSpace (FilterText);

		public string FilterText
		{
			get { return this.filterText; }
			set
			{
				if (this.filterText == value)
					return;

				string oldFilter = this.filterText;
				this.filterText = value;
				Filter (oldFilter);
				OnPropertyChanged ();
				if (String.IsNullOrWhiteSpace (oldFilter) != String.IsNullOrWhiteSpace (value))
					OnPropertyChanged (nameof(IsFiltering));
			}
		}

		public PropertyArrangeMode ArrangeMode
		{
			get { return this.arrangeMode; }
			set
			{
				if (this.arrangeMode == value)
					return;

				this.arrangeMode = value;
				Arrange ();
				OnPropertyChanged ();
			}
		}

		public IReadOnlyList<ArrangeModeViewModel> ArrangeModes
		{
			get;
		}

		public bool GetIsExpanded (string group)
		{
			if (group == null || !this.arranged.TryGetValue (group, out PanelGroupViewModel panelGroup))
				return false;

			return panelGroup.GetIsExpanded (ArrangeMode);
		}

		public void SetIsExpanded (string group, bool isExpanded)
		{
			SetIsExpanded (ArrangeMode, group, isExpanded);
		}

		public Task<Stream> GetIconAsync ()
		{
			if (TargetPlatform.IconProvider == null)
				return Task.FromResult<Stream> (null);

			return TargetPlatform.IconProvider.GetTypeIconAsync (ObjectEditors.Select (oe => oe.TargetType).ToArray ());
		}

		protected override void OnAddEditors (IEnumerable<EditorViewModel> editors)
		{
			IEnumerable<EditorViewModel> props = Properties;
			if (!String.IsNullOrWhiteSpace (FilterText))
				props = props.Where (MatchesFilter);

			props = props.OrderBy (vm => vm);

			Dictionary<string, List<PropertyViewModel>> groupedTypeProperties = null;

			bool isFlat = ArrangeMode == PropertyArrangeMode.Name;

			this.arranged.Clear ();
			foreach (var grouping in props.GroupBy (GetGroup).OrderBy (g => g.Key, CategoryComparer.Instance)) {
				HashSet<EditorViewModel> remainingItems = null;

				if (ArrangeMode == PropertyArrangeMode.Category) {
					foreach (EditorViewModel editorVm in grouping) {
						if (!(editorVm is PropertyViewModel vm)) {
							continue;
						}

						if (TargetPlatform.GroupedTypes != null && TargetPlatform.GroupedTypes.TryGetValue (vm.Property.Type, out string category)) {
							if (remainingItems == null)
								remainingItems = new HashSet<EditorViewModel> (grouping);

							remainingItems.Remove (vm);

							if (groupedTypeProperties == null)
								groupedTypeProperties = new Dictionary<string, List<PropertyViewModel>> ();
							if (!groupedTypeProperties.TryGetValue (category, out List<PropertyViewModel> group))
								groupedTypeProperties[category] = group = new List<PropertyViewModel> ();

							group.Add (vm);
						}
					}
				}

				string key = grouping.Key;
				if (remainingItems != null) {// TODO: pretty sure this was out of order before, add test
					if (remainingItems.Count > 0)
						this.arranged.Add (key, new PanelGroupViewModel (TargetPlatform, key, grouping.Where (evm => remainingItems.Contains (evm))));
				} else
					this.arranged.Add (key, new PanelGroupViewModel (TargetPlatform, key, grouping, separateUncommon: !isFlat));

				AutoExpandGroup (key);
			}

			if (groupedTypeProperties != null) { // Insert type-grouped properties back in sorted.
				int i = 0;
				foreach (var kvp in groupedTypeProperties.OrderBy (kvp => kvp.Key, CategoryComparer.Instance)) {
					var group = new PanelGroupViewModel (TargetPlatform, kvp.Key, new[] { new PropertyGroupViewModel (TargetPlatform, kvp.Key, kvp.Value, ObjectEditors) { Parent = this } });

					bool added = false;
					for (; i < this.arranged.Count; i++) {
						var g = this.arranged[i];

						// TODO: Are we translating categories? If so this needs to lookup the resource and be culture specific
						// nulls go on the bottom.
						if (String.IsNullOrEmpty (g.Category) || String.Compare (g.Category, kvp.Key, StringComparison.Ordinal) > 0) {
							added = true;
							this.arranged.Insert (i, group.Category, group);
							break;
						}
					}

					if (!added)
						this.arranged.Add (group.Category, group);

					AutoExpandGroup (group.Category);
				}
			}

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		protected override void OnRemoveEditors (IEnumerable<EditorViewModel> editors)
		{
			foreach (EditorViewModel vm in editors) {
				string g = GetGroup (vm);
				PanelGroupViewModel group = this.arranged[g];
				if (group == null)
					continue;

				group.Remove (vm);
				if (!group.HasChildElements)
					this.arranged.Remove (group.Category);
			}

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		protected override void OnClearProperties ()
		{
			this.arranged.Clear ();

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		internal override bool GetIsLastVariant (PropertyViewModel viewModel)
		{
			if (viewModel == null)
				throw new ArgumentNullException (nameof (viewModel));
			if (!viewModel.IsVariant)
				throw new ArgumentException ($"{nameof (viewModel)} is not a variant", nameof (viewModel));

			string groupKey = GetGroup (viewModel);
			PanelGroupViewModel group = this.arranged[groupKey];

			var list = group.GetList (viewModel);

			int index = list.IndexOf (viewModel);
			if (index == -1)
				throw new KeyNotFoundException ($"{nameof (viewModel)} was not found");

			if (++index == list.Count)
				return true;
			if (list[index] is PropertyViewModel pvm) {
				return !Equals (viewModel.Property, pvm.Property);
			} else
				return false;
		}

		private readonly OrderedDictionary<string, PanelGroupViewModel> arranged = new OrderedDictionary<string, PanelGroupViewModel> ();

		private PropertyArrangeMode arrangeMode;
		private string filterText;
		private bool autoExpand;

		private void AutoExpandGroup (string group)
		{
			if (group == null || !this.arranged.TryGetValue (group, out PanelGroupViewModel panelGroup))
				return;
			if (!AutoExpand && (TargetPlatform.AutoExpandGroups == null || !TargetPlatform.AutoExpandGroups.Contains (group)))
				return;

			UpdateExpanded (new[] { panelGroup }, true);
		}

		private void SetIsExpanded (PropertyArrangeMode mode, string group, bool isExpanded)
		{
			if (!this.arranged.TryGetValue (group, out PanelGroupViewModel panelGroup) || mode == PropertyArrangeMode.Name)
				return;

			panelGroup.SetIsExpanded (mode, isExpanded);
		}

		private void UpdateExpanded (bool expanded)
		{
			UpdateExpanded (this.arranged.Values, expanded);
		}

		private void UpdateExpanded (IEnumerable<PanelGroupViewModel> groups, bool expanded)
		{
			foreach (PanelGroupViewModel group in groups) {
				foreach (var mode in ArrangeModes) {
					if (mode.ArrangeMode == PropertyArrangeMode.Name)
						continue;

					group.SetIsExpanded (mode.ArrangeMode, expanded);
				}
			}
		}

		private void Arrange()
		{
			this.arranged.Clear ();

			OnAddEditors (Properties);
		}

		private void Filter (string oldFilter)
		{
			bool hadChildren = HasChildElements;

			if (FilterText != null && (String.IsNullOrWhiteSpace (oldFilter) || FilterText.StartsWith (oldFilter, StringComparison.OrdinalIgnoreCase))) {
				var toRemove = new List<EditorViewModel> ();
				foreach (PanelGroupViewModel g in this.arranged.Values) {
					foreach (EditorViewModel vm in g.Editors.Concat (g.UncommonEditors)) {
						if (!MatchesFilter (vm))
							toRemove.Add (vm);
						else if (vm is IFilterable) {
							var filterable = (IFilterable) vm;
							filterable.FilterText = FilterText;
							if (!filterable.HasChildElements)
								toRemove.Add (vm);
						}
					}
				}

				OnRemoveEditors (toRemove);
			} else {
				OnAddEditors (Properties);
			}

			if (hadChildren != HasChildElements)
				OnPropertyChanged (nameof(HasChildElements));
		}

		private string GetGroup (EditorViewModel vm)
		{
			if (ArrangeMode == PropertyArrangeMode.Name)
				return "0";
			if (vm is PropertyViewModel pvm) {
				if (TargetPlatform.GroupedTypes != null && TargetPlatform.GroupedTypes.TryGetValue (pvm.Property.Type, out string groupName))
					return groupName;
			}

			return vm.Category ?? String.Empty;
		}

		private bool MatchesFilter (EditorViewModel vm)
		{
			if (String.IsNullOrWhiteSpace (FilterText))
				return true;
			if (ArrangeMode == PropertyArrangeMode.Category && !String.IsNullOrEmpty (vm.Category) && vm.Category.Contains (FilterText, StringComparison.OrdinalIgnoreCase))
				return true;
			if (String.IsNullOrWhiteSpace (vm.Name))
				return false;

			return vm.Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase);
		}
	}
}
