using System;
using AppKit;

namespace Xamarin.PropertyEditing.Mac
{
	internal class BaseOutlineViewDelegate
		: NSOutlineViewDelegate
	{
		internal IHostResourceProvider HostResources { get; }
        internal BaseOutlineViewDataSource DataSource { get; }

        public BaseOutlineViewDelegate (IHostResourceProvider hostResources, BaseOutlineViewDataSource dataSource)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));
			if (dataSource == null)
				throw new ArgumentNullException (nameof (dataSource));

			HostResources = hostResources;
			DataSource = dataSource;
		}
	}
}
