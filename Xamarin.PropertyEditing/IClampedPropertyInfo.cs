namespace Xamarin.PropertyEditing
{
	public interface IClampedPropertyInfo
	{
		IPropertyInfo MaximumProperty { get; }
		IPropertyInfo MinimumProperty { get; }
	}
}