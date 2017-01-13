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

		protected override async Task<IReadOnlyList<IObjectEditor>> GetEditorsAsync ()
		{
			int i = 0;
			Task<IObjectEditor>[] editorTasks = new Task<IObjectEditor>[SelectedObjects.Count];
			foreach (object item in SelectedObjects) {
				editorTasks[i++] = EditorProvider.GetObjectEditorAsync (item);
			}

			return await Task.WhenAll (editorTasks).ConfigureAwait (false);
		}

		private async void OnSelectedObjectsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			// this needs to be more connected with the object changes to allow for better reuse
			OnPropertiesChanged ();
		}
	}
}