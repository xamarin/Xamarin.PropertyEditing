using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Reflection;

namespace Xamarin.PropertyEditing.Tests
{
	internal class MockBindingEditor
		: IObjectEditor
	{
		public MockBindingEditor (MockBinding binding)
		{
			Target = binding;
			this.editor = new ReflectionObjectEditor (binding);
			this.editor.PropertyChanged += (sender, args) => {
				PropertyChanged?.Invoke (this, args);
			};

			KnownProperties = new Dictionary<IPropertyInfo, KnownProperty> {
				{ this.editor.Properties.Single (pi => pi.Name == nameof(MockBinding.Source)), PropertyBinding.SourceProperty },
				{ this.editor.Properties.Single (pi => pi.Name == nameof(MockBinding.SourceParameter)), PropertyBinding.SourceParameterProperty },
				{ this.editor.Properties.Single (pi => pi.Name == nameof(MockBinding.Path)), PropertyBinding.PathProperty },
				{ this.editor.Properties.Single (pi => pi.Name == nameof(MockBinding.Converter)), PropertyBinding.ConverterProperty },
				{ this.editor.Properties.Single (pi => pi.Name == nameof(MockBinding.TypeLevel)), PropertyBinding.TypeLevelProperty }
			};
		}

		public event EventHandler<EditorPropertyChangedEventArgs> PropertyChanged;

		public object Target
		{
			get;
		}

		public ITypeInfo TargetType => this.editor.TargetType;

		public IReadOnlyCollection<IPropertyInfo> Properties => this.editor.Properties;

		public IReadOnlyDictionary<IPropertyInfo, KnownProperty> KnownProperties
		{
			get;
		}

		public IObjectEditor Parent => null;

		public IReadOnlyList<IObjectEditor> DirectChildren => null;

		public Task<AssignableTypesResult> GetAssignableTypesAsync (IPropertyInfo property, bool childTypes)
		{
			return this.editor.GetAssignableTypesAsync (property, childTypes);
		}

		public Task<IReadOnlyCollection<PropertyVariationSet>> GetPropertyVariantsAsync (IPropertyInfo property)
		{
			throw new NotSupportedException();
		}

		public Task SetValueAsync<T> (IPropertyInfo property, ValueInfo<T> value, PropertyVariationSet variations = null)
		{
			return this.editor.SetValueAsync (property, value, variations);
		}

		public Task<ValueInfo<T>> GetValueAsync<T> (IPropertyInfo property, PropertyVariationSet variations = null)
		{
			return this.editor.GetValueAsync<T> (property, variations);
		}

		private readonly ReflectionObjectEditor editor;
	}
}