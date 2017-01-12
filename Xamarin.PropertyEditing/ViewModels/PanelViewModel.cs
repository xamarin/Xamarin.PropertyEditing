using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PanelViewModel
		: PropertiesViewModel
	{
		public PanelViewModel (IEditorProvider provider)
			: base (provider)
		{
			var selected = new ObservableCollection<object> ();
			selected.CollectionChanged += OnSelectedObjectsChanged;
			SelectedObjects = selected;
		}

		public ICollection<object> SelectedObjects
		{
			get;
		}

		protected override Task<IReadOnlyList<IObjectEditor>> GetEditorsAsync ()
		{
			throw new NotImplementedException();
		}

		private async void OnSelectedObjectsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{

		}
	}
}