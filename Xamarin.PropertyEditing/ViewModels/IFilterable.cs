namespace Xamarin.PropertyEditing.ViewModels
{
	internal interface IFilterable
	{
		string FilterText { get; set; }

		bool HasChildElements { get; }
	}
}