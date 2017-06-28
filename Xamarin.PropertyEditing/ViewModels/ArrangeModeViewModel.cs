using System;
using System.ComponentModel;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ArrangeModeViewModel
		: NotifyingObject
	{
		public ArrangeModeViewModel (PropertyArrangeMode arrangeMode, PanelViewModel parent)
		{
			if (parent == null)
				throw new ArgumentNullException (nameof(parent));

			this.parent = parent;
			this.parent.PropertyChanged += OnParentPropertyChanged;
			ArrangeMode = arrangeMode;
		}

		public PropertyArrangeMode ArrangeMode
		{
			get;
		}

		public bool IsChecked
		{
			get { return this.parent.ArrangeMode == ArrangeMode; }
			set { this.parent.ArrangeMode = ArrangeMode; }
		}

		private readonly PanelViewModel parent;

		private void OnParentPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(PanelViewModel.ArrangeMode))
				OnPropertyChanged (nameof(IsChecked));
		}
	}
}