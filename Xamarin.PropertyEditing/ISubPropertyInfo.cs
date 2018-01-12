namespace Xamarin.PropertyEditing
{
	public interface ISubPropertyInfo : IPropertyInfo
	{
		IComplexPropertyInfo ParentProperty { get; }
	}
}
