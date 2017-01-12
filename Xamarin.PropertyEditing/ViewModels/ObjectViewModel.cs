using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ObjectViewModel
		: PropertiesViewModel
	{
		private object value;

		public ObjectViewModel (IEditorProvider provider)
			: base (provider)
		{
		}

		public object Value
		{
			get { return this.value; }
			set
			{
				this.value = value;
				OnPropertyChanged ();
				OnPropertiesChanged ();
			}
		}

		protected override async Task<IReadOnlyList<IObjectEditor>> GetEditorsAsync ()
		{
			IObjectEditor editor = await EditorProvider.GetObjectEditorAsync (Value);
			return new[] { editor };
		}
	}
}
