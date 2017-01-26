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
				if (Equals (this.value, value))
					return;

				this.value = value;
				OnPropertyChanged ();

				SelectedObjects.Clear ();
				SelectedObjects.Add (value);
			}
		}
	}
}
