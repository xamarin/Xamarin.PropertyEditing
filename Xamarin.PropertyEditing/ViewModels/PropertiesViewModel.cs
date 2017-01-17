using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal abstract class PropertiesViewModel
		: ViewModelBase
	{
		public PropertiesViewModel (IEditorProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof (provider));

			EditorProvider = provider;
		}

		public IReadOnlyList<PropertyViewModel> Properties => this.properties;

		protected IEditorProvider EditorProvider
		{
			get;
		}

		protected abstract Task<IReadOnlyList<IObjectEditor>> GetEditorsAsync ();

		protected async void OnPropertiesChanged ()
		{
			IReadOnlyList<IObjectEditor> editors = await GetEditorsAsync ();
			if (editors.Count == 0) {
				this.properties.Clear();
				return;
			}

			var newSet = new HashSet<IPropertyInfo> (editors[0].Properties);
			for (int i = 1; i < editors.Count; i++) {
				newSet.IntersectWith (editors[i].Properties);
			}

			foreach (PropertyViewModel vm in this.properties.ToArray()) {
				if (!newSet.Remove (vm.Property)) {
					this.properties.Remove (vm);
					continue;
				}

				foreach (IObjectEditor editor in editors) {
					if (!vm.Editors.Contains (editor))
						vm.Editors.Add (editor);
				}
			}

			foreach (IPropertyInfo property in newSet) {
				this.properties.Add (GetViewModel (property, editors));
			}
		}

		private readonly ObservableCollection<PropertyViewModel> properties = new ObservableCollection<PropertyViewModel> ();

		private PropertyViewModel GetViewModel (IPropertyInfo property, IEnumerable<IObjectEditor> editors)
		{
			Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel> vmFactory;
			if (ViewModelMap.TryGetValue (property.Type, out vmFactory))
				return vmFactory (property, editors);
			
			return new StringPropertyViewModel (property, editors);
		}

		private static readonly Dictionary<Type,Func<IPropertyInfo,IEnumerable<IObjectEditor>,PropertyViewModel>> ViewModelMap = new Dictionary<Type, Func<IPropertyInfo, IEnumerable<IObjectEditor>, PropertyViewModel>> {
			{ typeof(string), (p,e) => new StringPropertyViewModel (p, e) },
			{ typeof(bool), (p,e) => new PropertyViewModel<bool> (p, e) }
		};
	}
}