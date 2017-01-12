using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

		}

		private readonly ObservableCollection<PropertyViewModel> properties;
	}
}