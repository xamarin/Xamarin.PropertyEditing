﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PanelViewModel
		: PropertiesViewModel
	{
		public PanelViewModel (IEditorProvider provider)
			: base (provider)
		{
		}

		public event EventHandler ArrangedPropertiesChanged;

		public IReadOnlyList<IGroupingList<string, PropertyViewModel>> ArrangedProperties => this.arranged;

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

		protected override void OnAddProperties (IEnumerable<PropertyViewModel> properties)
		{
			IEnumerable<PropertyViewModel> props = Properties;
			if (!String.IsNullOrWhiteSpace (FilterText))
				props = props.Where (MatchesFilter);

			props = props.OrderBy (vm => vm.Property.Name);

			this.arranged.Clear ();
			foreach (var grouping in props.GroupBy (GetGroup).OrderBy (g => g.Key)) {
				this.arranged.Add (grouping);
			}

			ArrangedPropertiesChanged?.Invoke (this, EventArgs.Empty);
		}

		protected override void OnRemoveProperties (IEnumerable<PropertyViewModel> properties)
		{
			foreach (PropertyViewModel vm in properties) {
				string g = GetGroup (vm);
				var grouping = this.arranged[g] as ObservableGrouping<string, PropertyViewModel>;
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
		private readonly ObservableLookup<string, PropertyViewModel> arranged = new ObservableLookup<string, PropertyViewModel> {
			ReuseGroups = true
		};

		private PropertyArrangeMode arrangeMode;
		private string filterText;

		private void Arrange()
		{
			this.arranged.Clear ();

			OnAddProperties (Properties);
		}

		private enum FilterState
		{
			Unknown,
			Shorter,
			Longer
		}

		private void Filter (string oldFilter)
		{
			FilterState state = FilterState.Unknown;
			if (String.IsNullOrWhiteSpace (oldFilter) || FilterText.StartsWith (oldFilter, StringComparison.OrdinalIgnoreCase))
				state = FilterState.Longer;
			else if (oldFilter.StartsWith (FilterText, StringComparison.OrdinalIgnoreCase))
				state = FilterState.Shorter;

			if (state != FilterState.Shorter) {
				var toRemove = new List<PropertyViewModel> ();
				foreach (var g in this.arranged) {
					foreach (var vm in g) {
						if (!MatchesFilter (vm))
							toRemove.Add (vm);
					}
				}

				OnRemoveProperties (toRemove);
			}

			if (state != FilterState.Longer) {
				OnAddProperties (Properties);
			}
		}

		private string GetGroup (PropertyViewModel vm)
		{
			return (ArrangeMode == PropertyArrangeMode.Name) ? "0" : vm.Category;
		}

		private bool MatchesFilter (PropertyViewModel vm)
		{
			if (String.IsNullOrWhiteSpace (FilterText))
				return true;

			if (ArrangeMode == PropertyArrangeMode.Category && vm.Category != null && vm.Category.Contains (FilterText, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}

			return vm.Property.Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase);
		}
	}
}