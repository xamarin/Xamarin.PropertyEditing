namespace Xamarin.PropertyEditing.Tests
{
	interface IGetAndSet
	{
		TValue GetValue<TValue> (object target);
		void SetValue<TValue> (object target, TValue value);
	}
}
