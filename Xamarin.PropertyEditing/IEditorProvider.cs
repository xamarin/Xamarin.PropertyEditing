using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IEditorProvider
	{
		Task<IObjectEditor> GetObjectEditorAsync (object item);

		Task<IReadOnlyCollection<IPropertyInfo>> GetPropertiesForTypeAsync (ITypeInfo type);

		/// <summary>
		/// Creates a representation value of the <paramref name="type"/> and returns it.
		/// </summary>
		Task<object> CreateObjectAsync (ITypeInfo type);

		/// <summary>
		/// Gets a mapping of known types (such as CommonSolidBrush) to a real-type analog.
		/// </summary>
		/// <remarks>
		/// The "real-type analog" <see cref="ITypeInfo" /> does not need to be the real version of the type. It simply needs to be a type
		/// that <see cref="CreateObjectAsync"/> into <see cref="GetObjectEditorAsync"/> can understand for the purposes of creating an
		/// <see cref="IObjectEditor"/> of it.
		/// </remarks>
		Task<IReadOnlyDictionary<Type, ITypeInfo>> GetKnownTypesAsync (IReadOnlyCollection<Type> knownTypes);
	}
}