﻿using System;
using System.Collections.Generic;
 using System.IO;
 using System.Linq;
 using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PanelViewModel
		: PropertiesViewModel, IFilterable
	{
		public PanelViewModel (TargetPlatform targetPlatform)
			: base (targetPlatform)
		{
			ArrangeModes = new List<ArrangeModeViewModel> {
				new ArrangeModeViewModel (PropertyArrangeMode.Name, this),
				new ArrangeModeViewModel (PropertyArrangeMode.Category, this)
			};
		}

		public event EventHandler ArrangedPropertiesChanged;

		public IReadOnlyList<IGroupingList<string, EditorViewModel>> ArrangedEditors => this.arranged;

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
			HashSet<string> groups;
			if (!this.expandedGroups.TryGetValue (ArrangeMode, out groups))
				return false;

			return groups.Contains (group);
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

				AutoExpandGroup (grouping.Key);
				if (remainingItems != null)
					this.arranged.Add (grouping.Key, remainingItems);
				else
					this.arranged.Add (grouping);
			}

			if (groupedTypeProperties != null) { // Insert type-grouped properties back in sorted.
				int i = 0;
				foreach (var kvp in groupedTypeProperties.OrderBy (kvp => kvp.Key, CategoryComparer.Instance)) {
					var group = new ObservableGrouping<string, EditorViewModel> (kvp.Key) {
						new PropertyGroupViewModel (TargetPlatform, kvp.Key, kvp.Value, ObjectEditors)
					};

					AutoExpandGroup (group.Key);

					bool added = false;
					for (; i < this.arranged.Count; i++) {
						var g = (IGrouping<string, EditorViewModel>) this.arranged[i];

						// TODO: Are we translating categories? If so this needs to lookup the resource and be culture specific
						// nulls go on the bottom.
						if (g.Key == null || String.Compare (g.Key, kvp.Key, StringComparison.Ordinal) > 0) {
							added = true;
							this.arranged.Insert (i, group);
							break;
						}
					}

					if (!added)
						this.arranged.Add (group);
				}
			}

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		protected override void OnRemoveEditors (IEnumerable<EditorViewModel> editors)
		{
			foreach (EditorViewModel vm in editors) {
				string g = GetGroup (vm);
				var grouping = this.arranged[g] as ObservableGrouping<string, EditorViewModel>;
				if (grouping != null) {
					this.arranged.Remove (g, vm);
				}
			}

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		protected override void OnClearProperties ()
		{
			this.arranged.Clear ();

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		private readonly Dictionary<PropertyArrangeMode, HashSet<string>> expandedGroups = new Dictionary<PropertyArrangeMode, HashSet<string>> ();
		private readonly ObservableLookup<string, EditorViewModel> arranged = new ObservableLookup<string, EditorViewModel> {
			ReuseGroups = true
		};

		private PropertyArrangeMode arrangeMode;
		private string filterText;
		private bool autoExpand;

		private void AutoExpandGroup (string group)
		{
			if (AutoExpand || TargetPlatform.AutoExpandGroups.Contains (group))
				UpdateExpanded (new[] { group }, true);
		}

		private void SetIsExpanded (PropertyArrangeMode mode, string group, bool isExpanded)
		{
			if (!this.expandedGroups.TryGetValue (mode, out HashSet<string> groups)) {
				if (!isExpanded)
					return;

				this.expandedGroups[mode] = groups = new HashSet<string> ();
			}

			if (isExpanded)
				groups.Add (group);
			else
				groups.Remove (group);
		}

		private void UpdateExpanded (bool expanded)
		{
			UpdateExpanded (this.arranged.Select<IGroupingList<string, EditorViewModel>, string> (g => g.Key), expanded);
		}

		private void UpdateExpanded (IEnumerable<string> groups, bool expanded)
		{
			foreach (string group in groups) {
				foreach (var mode in ArrangeModes) {
					if (mode.ArrangeMode == PropertyArrangeMode.Name)
						continue;

					SetIsExpanded (mode.ArrangeMode, group, expanded);
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
				foreach (var g in this.arranged) {
					foreach (var vm in g) {
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
			return (ArrangeMode == PropertyArrangeMode.Name) ? "0" : vm.Category;
		}

		private bool MatchesFilter (EditorViewModel vm)
		{
			if (String.IsNullOrWhiteSpace (FilterText))
				return true;
			if (ArrangeMode == PropertyArrangeMode.Category && vm.Category != null && vm.Category.Contains (FilterText, StringComparison.OrdinalIgnoreCase))
				return true;
			if (String.IsNullOrWhiteSpace (vm.Name))
				return false;

			return vm.Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase);
		}
	}
}