using System.Windows.Input;

namespace Xamarin.PropertyEditing.Windows
{
	internal class CategoryExpander
		: FilterExpander
	{
		protected override void OnExpanded ()
		{
			base.OnExpanded ();
			SetExpanded (true);
		}

		protected override void OnCollapsed ()
		{
			base.OnCollapsed ();
			SetExpanded (false);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.Key == Key.Down) {
				SetExpanded (true);
				UpdateValue();
				e.Handled = true;
			} else if (e.Key == Key.Up) {
				SetExpanded (false);
				UpdateValue();
				e.Handled = true;
			} else if (e.Key == Key.Space) { // Expander should have this built, not sure why it's not working
				SetExpanded (!IsExpanded);
				UpdateValue();
				e.Handled = true;
			}

			base.OnKeyDown (e);
		}

		protected override void UpdateValue ()
		{
			if (ViewModel == null)
				return;

			SetCurrentValue (IsExpandedProperty, IsFiltered || ViewModel.GetIsExpanded (Header as string));
		}

		protected override void OnIsFilteredChanged ()
		{
			this.fromFilter = true;
			base.OnIsFilteredChanged ();
			this.fromFilter = false;
		}

		private bool fromFilter;

		private void SetExpanded (bool isExpanded)
		{
			if (this.fromFilter)
				return;

			ViewModel.SetIsExpanded ((string) Header, isExpanded);
		}
    }
}
