using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing
{
	public enum BindingSourceType
	{
		/// <summary>
		/// The binding source is that of an existing object that can be targeted, Ex. {Binding ElementName=..}
		/// </summary>
		Object = 0,

		/// <summary>
		/// The binding source is that of a type and the property path comes from the selected type. Ex. {RelativeSource AncestorType=..}
		/// </summary>
		Type = 1,
		Resource = 2,

		/// <summary>
		/// SingleObject behaves the same as <see cref="Object"/>, but <see cref="BindingSource.Description"/> is treated as a long description
		/// replacing any object selection UI. Ex. {RelativeSource Self}
		/// </summary>
		SingleObject = 3
	}

	public class BindingSource
	{
		public BindingSource (string name, BindingSourceType type)
		{
			if (name == null)
				throw new ArgumentNullException (nameof(name));

			Name = name;
			Type = type;
		}

		/// <param name="description">The localized description provided to the end user when the binding source is singular (such as self binding).</param>
		public BindingSource (string name, BindingSourceType type, string description)
			: this (name, type)
		{
			if (description == null)
				throw new ArgumentNullException (nameof(description));

			Description = description;
		}

		public string Name
		{
			get;
		}

		public BindingSourceType Type
		{
			get;
		}

		public string Description
		{
			get;
		}
	}

	public interface IBindingProvider
	{
		/// <summary>
		/// Gets a list of the binding sources that can be applied to a binding for this property.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		Task<IReadOnlyList<BindingSource>> GetBindingSourcesAsync (object target, IPropertyInfo property);

		/// <exception cref="ArgumentException"><paramref name="source"/>'s <see cref="BindingSource.Type"/> is not <see cref="BindingSourceType.Resource"/>.</exception>
		Task<ILookup<ResourceSource, Resource>> GetResourcesAsync (BindingSource source, object target);

		/// <summary>
		/// Gets a list of the types selectable as a source in the binding <paramref name="source"/>.
		/// </summary>
		Task<AssignableTypesResult> GetSourceTypesAsync (BindingSource source, object target);

		/// <summary>
		/// Gets the root element objects for a binding <paramref name="source"/> of <see cref="BindingSourceType.Object"/> type.
		/// </summary>
		/// <exception cref="ArgumentException"><paramref name="source"/>'s <see cref="BindingSource.Type"/> is not <see cref="BindingSourceType.Object"/>.</exception>
		Task<IReadOnlyList<object>> GetRootElementsAsync (BindingSource source, object target);

		Task<IReadOnlyList<Resource>> GetValueConverterResourcesAsync (object target);
	}
}
