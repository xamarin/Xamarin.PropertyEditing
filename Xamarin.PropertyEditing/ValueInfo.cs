namespace Xamarin.PropertyEditing
{
	public class ValueInfo<T>
	{
		public T Value
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a descriptor of the value source, such as a resource reference or binding description.
		/// </summary>
		public object ValueDescriptor
		{
			get;
			set;
		}

		public ValueSource Source
		{
			get;
			set;
		}
	}
}