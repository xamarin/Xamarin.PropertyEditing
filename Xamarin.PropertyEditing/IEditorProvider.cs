using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IEditorProvider
	{
		Task<IObjectEditor> GetObjectEditorAsync (object item);
	}
}