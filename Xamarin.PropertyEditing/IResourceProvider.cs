using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IResourceProvider
	{
		/// <summary>
		/// Gets the resources available to the given <paramref name="target"/> and <paramref name="property"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> or <paramref name="target"/> is <c>null</c>.</exception>
		/// <remarks>
		/// <para>
		/// Returned resources can either be of base class <see cref="Resource"/> or, whenever possible, of <see cref="Resource{T}"/>
		/// including their relative representative value. This mean things like dynamic resources should, if possible, do a lookup
		/// of its value relative to the <paramref name="target" /> and provide that value.
		/// </para>
		/// </remarks>
		Task<IReadOnlyList<Resource>> GetResourcesAsync (object target, IPropertyInfo property, CancellationToken cancelToken);

		/// <summary>
		/// Gets resource sources relative to the provided <paramref name="target"/>.
		/// </summary>
		Task<IReadOnlyList<ResourceSource>> GetResourceSourcesAsync (object target, IPropertyInfo property);

		/// <typeparam name="T">The representation type.</typeparam>
		/// <param name="value">The value of the resource in it's representative form.</param>
		Task<Resource> CreateResourceAsync<T> (ResourceSource source, string name, T value);
	}
}