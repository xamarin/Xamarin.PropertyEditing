using System;

namespace Xamarin.PropertyEditing
{
	public class FilePath
	{
		public string Source { get; set; }

		public FilePath () { }

		public FilePath (string source) 
		{
			Source = source;
		}

		public override string ToString ()
		{
			return Source;
		}
	}
}