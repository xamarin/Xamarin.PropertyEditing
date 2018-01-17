using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class TypeSelectorViewModel
		: NotifyingObject
	{
		public TypeSelectorViewModel (AsyncValue<IReadOnlyDictionary<IAssemblyInfo, ILookup<string, ITypeInfo>>> assignableTypes)
		{
			if (assignableTypes == null)
				throw new ArgumentNullException (nameof (assignableTypes));

			assignableTypes.Task.ContinueWith (t => {
				this.assemblyView = new SimpleCollectionView (t.Result, new SimpleCollectionViewOptions {
					DisplaySelector = (o) => ((KeyValuePair<IAssemblyInfo, ILookup<string, ITypeInfo>>)o).Key.Name,
					ChildrenSelector = (o) => ((KeyValuePair<IAssemblyInfo, ILookup<string, ITypeInfo>>)o).Value,
					ChildOptions = new SimpleCollectionViewOptions {
						DisplaySelector = (o) => ((IGrouping<string, ITypeInfo>)o).Key,
						ChildrenSelector = (o) => ((IGrouping<string, ITypeInfo>)o),
						ChildOptions = new SimpleCollectionViewOptions {
							DisplaySelector = (o) => ((ITypeInfo)o).Name
						}
					}
				});
				OnPropertyChanged (nameof(Types));
				IsLoading = false;
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public IEnumerable Types
		{
			get { return this.assemblyView; }
		}

		public bool IsLoading
		{
			get { return this.isLoading; }
			set
			{
				if (this.isLoading == value)
					return;

				this.isLoading = value;
				OnPropertyChanged();
			}
		}

		public bool ShowAllAssemblies
		{
			get {  return this.showAllAssemblies; }
			set
			{
				if (this.showAllAssemblies == value)
					return;

				bool oldValue = this.showAllAssemblies;
				this.showAllAssemblies = value;
				OnPropertyChanged();
				Filter (filterText, oldValue);
			}
		}

		public string FilterText
		{
			get { return this.filterText; }
			set
			{
				if (this.filterText == value)
					return;

				string oldFilter = this.filterText;
				this.filterText = value;
				OnPropertyChanged();
				Filter (oldFilter, ShowAllAssemblies);
			}
		}

		private string filterText;
		private bool isLoading;
		private bool showAllAssemblies;
		private SimpleCollectionView assemblyView;

		private void Filter (string oldFilter, bool wasShowingAll)
		{
			bool isSuperset = (wasShowingAll == ShowAllAssemblies) || ShowAllAssemblies;
			isSuperset &= !(String.IsNullOrWhiteSpace (oldFilter) || FilterText.StartsWith (oldFilter, StringComparison.OrdinalIgnoreCase));

			this.assemblyView.Filter (o => {
				if (o is KeyValuePair<IAssemblyInfo, ILookup<string, ITypeInfo>> kvp) {
					return ShowAllAssemblies || kvp.Key.IsRelevant;
				} else if (o is IGrouping<string, ITypeInfo> g) {
					return g.Key.StartsWith (FilterText, StringComparison.OrdinalIgnoreCase);
				} else if (o is ITypeInfo t) {
					return t.Name.StartsWith (FilterText, StringComparison.OrdinalIgnoreCase);
				}

				return false;
			}, isSuperset);
		}
	}
}
