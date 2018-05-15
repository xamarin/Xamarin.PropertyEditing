using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public interface IResourceProvider
	{
		/// <summary>
		/// Gets whether or not the resource provider can create resources.
		/// </summary>
		bool CanCreateResources { get; }

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

		/// <summary>
		/// Gets an unused type-appropriate resource key for a value of the <paramref name="property"/> being turned into a resource.
		/// </summary>
		Task<string> SuggestResourceNameAsync (IReadOnlyCollection<object> targets, IPropertyInfo property);

		/// <summary>
		/// Checks for issues creating a resource in the given <paramref name="source"/> with <paramref name="name"/> such as name in use, or would be overriden.
		/// </summary>
		Task<ResourceCreateError> CheckNameErrorsAsync (object target, ResourceSource source, string name);

		/// <typeparam name="T">The representation type.</typeparam>
		/// <param name="value">The value of the resource in it's representative form.</param>
		/// <exception cref="NotSupportedException"><see cref="CanCreateResources"/> is <c>false</c>.</exception>
		/// <exception cref="DuplicateNameException"><paramref name="name"/> is already present in the given <paramref name="source"/>.</exception>
		Task<Resource> CreateResourceAsync<T> (ResourceSource source, string name, T value);
	}

	public class ResourceCreateError
	{
		public ResourceCreateError (string message, bool isWarning)
		{
			if (message == null)
				throw new ArgumentNullException (nameof(message));

			IsWarning = isWarning;
			Message = message;
		}

		/// <summary>
		/// Gets or sets the localized description of the error
		/// </summary>
		public string Message
		{
			get;
		}

		/// <summary>
		/// Gets or sets whether the error message is just a warning, thereby not preventing creation
		/// </summary>
		public bool IsWarning
		{
			get;
		}
	}
}