namespace Xamarin.PropertyEditing
{
	public interface IValidator<in T>
	{
		bool IsValid (T value);
	}
}
