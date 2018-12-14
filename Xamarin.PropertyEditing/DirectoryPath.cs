using System;

namespace Xamarin.PropertyEditing
{
	public class DirectoryPath
	{
		public string Source { get; set; }

		public DirectoryPath () { }

		public DirectoryPath (string source) 
		{
			Source = source;
		}

		public override string ToString ()
		{
			return Source;
		}
	}
}