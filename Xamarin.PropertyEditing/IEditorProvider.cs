using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IEditorProvider
	{
		Task<IObjectEditor> GetObjectEditorAsync (object item);

		/// <summary>
		/// Creates a representation value of the <paramref name="type"/> and returns it.
		/// </summary>
		Task<object> CreateObjectAsync (ITypeInfo type);
	}
}