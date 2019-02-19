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
				this.typeOptions = new SimpleCollectionViewOptions {
					DisplaySelector = (o) => ((ITypeInfo)o).Name
				};

				this.assemblyView = new SimpleCollectionView (t.Result, this.assemblyOptions = new SimpleCollectionViewOptions {
					Filter = AssemblyFilter,
					DisplaySelector = (o) => ((KeyValuePair<IAssemblyInfo, ILookup<string, ITypeInfo>>)o).Key.Name,
					ChildrenSelector = (o) => ((KeyValuePair<IAssemblyInfo, ILookup<string, ITypeInfo>>)o).Value,
					ChildOptions = new SimpleCollectionViewOptions {
						DisplaySelector = (o) => ((IGrouping<string, ITypeInfo>)o).Key,
						ChildrenSelector = (o) => ((IGrouping<string, ITypeInfo>)o),
						ChildOptions = this.typeOptions
					}
				});
				OnPropertyChanged (nameof(Types));
				IsLoading = false;
			}, TaskScheduler.FromCurrentSynchronizationContext());
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
			}
		}

		public int TypeLevel
		{
			get { return this.typeLevel; }
			set
			{
				if (this.typeLevel == value)
					return;

				this.typeLevel = value;
				OnPropertyChanged();
			}
		}

		public IList Types
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
				this.assemblyOptions.Filter = (!value) ? AssemblyFilter : (Predicate<object>)null;
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
				this.typeOptions.Filter = (!String.IsNullOrWhiteSpace (FilterText))
					? (o => ((ITypeInfo)o).Name.Contains (FilterText, StringComparison.OrdinalIgnoreCase))
					: (Predicate<object>)null;
			}
		}

		private SimpleCollectionViewOptions assemblyOptions, typeOptions;
		private string filterText;
		private bool isLoading;
		private bool showAllAssemblies;
		private SimpleCollectionView assemblyView;
		private ITypeInfo selectedType;
		private int typeLevel = 1;

		private bool AssemblyFilter (object item)
		{
			return ((KeyValuePair<IAssemblyInfo, ILookup<string, ITypeInfo>>)item).Key.IsRelevant;
		}
	}
}
