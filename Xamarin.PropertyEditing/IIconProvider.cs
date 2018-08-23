using System.IO;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IIconProvider
	{
		/// <summary>
		/// Gets an icon for the given <paramref name="types"/>.
		/// </summary>
		/// <param name="types">The types to get an icon for.</param>
		/// <remarks>
		/// The types provided in <paramref name="type"/> may not be the same.
		/// If they aren't, you should return a generic icon (VS uses an XML Tag).
		/// </remarks>
		Task<Stream> GetTypeIconAsync (ITypeInfo[] types);
	}
}