using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class PropertyTreeRoot
	{
		internal PropertyTreeRoot (IEditorProvider provider, ITypeInfo type, IReadOnlyCollection<IPropertyInfo> properties)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));
			if (type == null)
				throw new ArgumentNullException (nameof(type));
			if (properties == null)
				throw new ArgumentNullException (nameof(properties));

			TargetType = type;
			Children = properties.Select (pi => new PropertyTreeElement (provider, pi)).ToArray ();
		}

		public ITypeInfo TargetType
		{
			get;
		}

		public IReadOnlyCollection<PropertyTreeElement> Children
		{
			get;
		}
	}

	internal class PropertyTreeElement
	{
		internal PropertyTreeElement (IEditorProvider provider, IPropertyInfo property)
		{
			if (provider == null)
				throw new ArgumentNullException (nameof(provider));
			if (property == null)
				throw new ArgumentNullException (nameof(property));

			Property = property;
			this.provider = provider;

			this.properties = this.provider.GetPropertiesForTypeAsync (property.RealType);
		}

		private PropertyTreeElement (IEditorProvider provider, IPropertyInfo property, PropertyTreeElement parent)
			: this (provider, property)
		{
			if (parent == null)
				throw new ArgumentNullException (nameof(parent));

			Parent = parent;
		}

		public IPropertyInfo Property
		{
			get;
		}

		public bool IsCollection
		{
			get;
		}

		public PropertyTreeElement Parent
		{
			get;
		}

		public AsyncValue<IReadOnlyCollection<PropertyTreeElement>> Children
		{
			get
			{
				if (this.children == null) {
					this.children = new AsyncValue<IReadOnlyCollection<PropertyTreeElement>> (
						this.properties.ContinueWith<IReadOnlyCollection<PropertyTreeElement>> (t =>
							t.Result.Select (p => new PropertyTreeElement (this.provider, p, this)).ToArray (), TaskScheduler.Default));
				}


				return this.children;
			}
		}

		private readonly IEditorProvider provider;
		private readonly Task<IReadOnlyCollection<IPropertyInfo>> properties;
		private AsyncValue<IReadOnlyCollection<PropertyTreeElement>> children;
	}
}
