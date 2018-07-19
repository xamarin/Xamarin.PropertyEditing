using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IEditorProvider
	{
		/// <summary>
		/// Gets a mapping of known types (such as CommonSolidBrush) to a real-type analog.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The "real-type analog" <see cref="ITypeInfo" /> does not need to be the real version of the type. It simply needs to be a type
		/// that <see cref="CreateObjectAsync"/> into <see cref="GetObjectEditorAsync"/> can understand for the purposes of creating an
		/// <see cref="IObjectEditor"/> of it.
		/// </para>
		/// <para>
		/// Expected known types are:
		///		- Applicable Common*Brush types
		///		- <see cref="PropertyBinding"/> if the platform supports binding.
		///		- <see cref="Xamarin.PropertyEditing.Common.CommonValueConverter"/> for XAML platforms.
		/// </para>
		/// </remarks>
		IReadOnlyDictionary<Type, ITypeInfo> KnownTypes { get; }

		/// <summary>
		/// Gets an object editor for the given target <paramref name="item"/>.
		/// </summary>
		/// <remarks>
		/// <param>
		/// It is not recommended that property value retrieval happens eagerly if the returned task waits for that to complete.
		/// Either do so in a separate-async fashion, or wait for the first <see cref="IObjectEditor.GetValueAsync{T}"/> call,
		/// as numerous <see cref="IObjectEditor"/>s may be requested that never retrieve their full value suite.
		/// </param>
		/// </remarks>
		Task<IObjectEditor> GetObjectEditorAsync (object item);

		Task<IReadOnlyCollection<IPropertyInfo>> GetPropertiesForTypeAsync (ITypeInfo type);

		/// <summary>
		/// Creates a representation value of the <paramref name="type"/> and returns it.
		/// </summary>
		Task<object> CreateObjectAsync (ITypeInfo type);

		/// <summary>
		/// Gets types assignable to the given base <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The base type to retrieve assignable types for.</param>
		/// <param name="childTypes">Whether or not to look for children of a collection <paramref name="type"/> or just <paramref name="type"/>.</param>
		Task<AssignableTypesResult> GetAssignableTypesAsync (ITypeInfo type, bool childTypes);

		/// <summary>
		/// Gets the children targets of the given target <paramref name="item"/>.
		/// </summary>
		Task<IReadOnlyList<object>> GetChildrenAsync (object item);
	}
}