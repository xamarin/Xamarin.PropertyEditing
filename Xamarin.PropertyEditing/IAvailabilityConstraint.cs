namespace Xamarin.PropertyEditing
{
	public interface IAvailabilityConstraint
	{
		bool GetIsAvailable (IObjectEditor editor);
	}
}