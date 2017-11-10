﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PanelViewModel
		: PropertiesViewModel, IFilterable
	{
		public PanelViewModel (IEditorProvider provider, TargetPlatform targetPlatform)
			: base (provider, targetPlatform)
		{
			ArrangeModes = new List<ArrangeModeViewModel> {
				new ArrangeModeViewModel (PropertyArrangeMode.Name, this),
				new ArrangeModeViewModel (PropertyArrangeMode.Category, this)
			};
		}

		public event EventHandler ArrangedPropertiesChanged;

		public IReadOnlyList<IGroupingList<string, EditorViewModel>> ArrangedEditors => this.arranged;

		public bool HasChildElements => (this.arranged.Count > 0);

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
			HashSet<string> groups;
			if (!this.expandedGroups.TryGetValue (ArrangeMode, out groups)) {
				if (!isExpanded)
					return;

				this.expandedGroups[ArrangeMode] = groups = new HashSet<string> ();
			}

			if (isExpanded)
				groups.Add (group);
			else
				groups.Remove (group);
		}

		protected override void OnAddEditors (IEnumerable<EditorViewModel> editors)
		{
			IEnumerable<EditorViewModel> props = Properties;
			if (!String.IsNullOrWhiteSpace (FilterText))
				props = props.Where (MatchesFilter);

			props = props.OrderBy (vm => vm.Name);

			this.arranged.Clear ();
			foreach (var grouping in props.GroupBy (GetGroup).OrderBy (g => g.Key)) {
				this.arranged.Add (grouping);
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

		private void Arrange()
		{
			this.arranged.Clear ();

			OnAddEditors (Properties);
		}

		private void Filter (string oldFilter)
		{
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

			if (ArrangeMode == PropertyArrangeMode.Category && vm.Category != null && vm.Category.Contains (FilterText, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}

			return vm.Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase);
		}
	}
}