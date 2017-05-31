namespace Xamarin.PropertyEditing.Windows
{
	internal class HostEnvironment
		: NotifyingObject
	{
		public static HostEnvironment Current
		{
			get;
		} = new HostEnvironment();

		public bool AreAnimationsAllowed
		{
			get { return this.areAnimationsAllowed; }
			set
			{
				if (this.areAnimationsAllowed == value)
					return;

				this.areAnimationsAllowed = value;
				OnPropertyChanged();
			}
		}

		public bool AreGradientsAllowed
		{
			get { return this.areGradientsAllowed; }
			set
			{
				if (this.areGradientsAllowed == value)
					return;

				this.areGradientsAllowed = value;
				OnPropertyChanged();
			}
		}

		private bool areAnimationsAllowed = true;
		private bool areGradientsAllowed = true;
	}
}