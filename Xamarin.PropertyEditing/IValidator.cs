namespace Xamarin.PropertyEditing
{
	public interface IValidator<T>
	{
		T ValidateValue (T value);
	}
}