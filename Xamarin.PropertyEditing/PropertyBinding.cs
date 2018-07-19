namespace Xamarin.PropertyEditing
{
	public static class PropertyBinding
	{
		public static readonly KnownProperty<string> PathProperty = new KnownProperty<string>();
		public static readonly KnownProperty<BindingSource> SourceProperty = new KnownProperty<BindingSource>();
		public static readonly KnownProperty<object> SourceParameterProperty = new KnownProperty<object> ();
		public static readonly KnownProperty<Resource> ConverterProperty = new KnownProperty<Resource>();
		public static readonly KnownProperty<int?> TypeLevelProperty = new KnownProperty<int?> ();
	}
}