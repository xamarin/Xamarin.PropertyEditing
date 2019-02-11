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
		public PanelGroupViewModel (string category, IEnumerable<EditorViewModel> editors, bool separateUncommon = true)
		{
			if (editors == null)
				throw new ArgumentNullException (nameof(editors));

			Category = category;
			AddCore (editors, separateUncommon);
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
			AddCore (editors, separate: true);
		}

		public void Add (EditorViewModel editor)
		{
			AddCore (editor, separate: true);
		}

		public bool Remove (EditorViewModel editor)
		{
			if (editor == null)
				throw new ArgumentNullException (nameof(editor));

			return GetList (editor, separate: true).Remove (editor);
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

		private Dictionary<PropertyArrangeMode, bool> isExpanded;
		private readonly ObservableCollectionEx<EditorViewModel> editors = new ObservableCollectionEx<EditorViewModel> ();
		private readonly ObservableCollectionEx<EditorViewModel> uncommonEditors = new ObservableCollectionEx<EditorViewModel> ();

		private void AddCore (IEnumerable<EditorViewModel> editors, bool separate)
		{
			if (editors == null)
				throw new ArgumentNullException (nameof (editors));

			foreach (EditorViewModel evm in editors)
				AddCore (evm, separate);
		}

		private void AddCore (EditorViewModel editor, bool separate)
		{
			if (editor == null)
				throw new ArgumentNullException (nameof (editor));

			GetList (editor, separate).Add (editor);
			OnPropertyChanged (nameof(HasChildElements));
			OnPropertyChanged (nameof(HasUncommonElements));
		}

		private IList<EditorViewModel> GetList (EditorViewModel evm, bool separate)
		{
			if (separate && evm is PropertyViewModel pvm)
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
						var vm = editorVm as PropertyViewModel;
						if (vm != null && TargetPlatform.GroupedTypes != null && TargetPlatform.GroupedTypes.TryGetValue (vm.Property.Type, out string category)) {
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

				string key = grouping.Key ?? String.Empty;
				if (remainingItems != null) // TODO: pretty sure this was out of order before, add test
					this.arranged.Add (key, new PanelGroupViewModel (key, grouping.Where (evm => remainingItems.Contains (evm))));
				else
					this.arranged.Add (key, new PanelGroupViewModel (key, grouping, separateUncommon: !isFlat));

				AutoExpandGroup (key);
			}

			if (groupedTypeProperties != null) { // Insert type-grouped properties back in sorted.
				int i = 0;
				foreach (var kvp in groupedTypeProperties.OrderBy (kvp => kvp.Key, CategoryComparer.Instance)) {
					var group = new PanelGroupViewModel (kvp.Key, new[] { new PropertyGroupViewModel (TargetPlatform, kvp.Key, kvp.Value, ObjectEditors) });

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
			return (ArrangeMode == PropertyArrangeMode.Name) ? "0" : (vm.Category ?? String.Empty);
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