namespace Xamarin.PropertyEditing
{
	public interface ICoerce<T>
	{
		T CoerceValue (T value);
	}
}