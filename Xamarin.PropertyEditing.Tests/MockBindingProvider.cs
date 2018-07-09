using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Reflection;
using Xamarin.PropertyEditing.Tests.MockControls;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockBindingProvider
		: IBindingProvider
	{
		public Task<IReadOnlyList<BindingSource>> GetBindingSourcesAsync (object target, IPropertyInfo property)
		{
			return Task.FromResult<IReadOnlyList<BindingSource>> (new[] {
				new BindingSourceInstance (RelativeSelf, $"Bind [{target.GetType().Name}] to itself."), 
				StaticResource,
				Ancestor
			});
		}

		public Task<AssignableTypesResult> GetSourceTypesAsync (BindingSource source, object target)
		{
			return ReflectionObjectEditor.GetAssignableTypes (typeof(MockControl).ToTypeInfo (), childTypes: false);
		}

		public Task<IReadOnlyList<object>> GetRootElementsAsync (BindingSource source, object target)
		{
			if (source == null)
				throw new ArgumentNullException (nameof(source));
			if (source.Type != BindingSourceType.Object && source.Type != BindingSourceType.SingleObject)
				throw new ArgumentException ("source.Type was not Object", nameof(source));

			if (source is BindingSourceInstance instance)
				source = instance.Original;

			if (source == RelativeSelf)
				return Task.FromResult<IReadOnlyList<object>> (new [] { target });
			if (source == StaticResource)
				return MockResourceProvider.GetResourceSourcesAsync (target).ContinueWith (t => (IReadOnlyList<object>) t.Result);

			throw new NotImplementedException();
		}

		public async Task<ILookup<ResourceSource, Resource>> GetResourcesAsync (BindingSource source, object target)
		{
			var results = await this.resources.GetResourcesAsync (target, CancellationToken.None).ConfigureAwait (false);
			return results.ToLookup (r => r.Source);
		}

		public Task<IReadOnlyList<Resource>> GetValueConverterResourcesAsync (object target)
		{
			return Task.FromResult<IReadOnlyList<Resource>> (Array.Empty<Resource> ());
		}

		private readonly MockResourceProvider resources = new MockResourceProvider();

		private static readonly BindingSource Ancestor = new BindingSource ("RelativeSource FindAncestor", BindingSourceType.Type);
		private static readonly BindingSource RelativeSelf = new BindingSource ("RelativeSource Self", BindingSourceType.SingleObject);
		private static readonly BindingSource StaticResource = new BindingSource ("StaticResource", BindingSourceType.Resource);

		private class BindingSourceInstance
			: BindingSource
		{
			public BindingSourceInstance (BindingSource original, string description)
				: base (original.Name, original.Type, description)
			{
				Original = original;
			}

			public BindingSource Original
			{
				get;
			}
		}
	}

	public class MockBinding
	{
		public string Path
		{
			get;
			set;
		}

		public BindingSource Source
		{
			get;
			set;
		}

		public object SourceParameter
		{
			get;
			set;
		}

		public Resource Converter
		{
			get;
			set;
		}

		public string StringFormat
		{
			get;
			set;
		}

		public bool IsAsync
		{
			get;
			set;
		}

		public int TypeLevel
		{
			get;
			set;
		}
	}
}