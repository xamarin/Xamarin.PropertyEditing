using System;

namespace Xamarin.PropertyEditing.Mac
{
	public class MacSystemInformation
	{
		public static readonly Version Ventura = new Version (13, 0);
		public static readonly Version Monterey = new Version (12, 0);
		public static readonly Version BigSur = new Version (11, 0);
		public static readonly Version Catalina = new Version (10, 15);
		public static readonly Version Mojave = new Version (10, 14);
		public static readonly Version HighSierra = new Version (10, 13);
		public static readonly Version Sierra = new Version (10, 12);
		public static readonly Version ElCapitan = new Version (10, 11);
		public static readonly Version Yosemite = new Version (10, 10);
		public static readonly Version Mavericks = new Version (10, 9);
		public static readonly Version MountainLion = new Version (10, 8);
		public static readonly Version Lion = new Version (10, 7);
		public static readonly Version SnowLeopard = new Version (10, 6);
		static Version version;

		[System.Runtime.InteropServices.DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int Gestalt (int selector, out int result);

		//TODO: there are other gestalt selectors that return info we might want to display
		//mac API for obtaining info about the system
		static int Gestalt (string selector)
		{
			System.Diagnostics.Debug.Assert (selector != null && selector.Length == 4);
			int cc = selector[3] | (selector[2] << 8) | (selector[1] << 16) | (selector[0] << 24);
			int result;
			int ret = Gestalt (cc, out result);
			if (ret != 0)
				throw new Exception (string.Format ("Error reading gestalt for selector '{0}': {1}", selector, ret));
			return result;
		}

		static MacSystemInformation ()
		{
			version = new Version (Gestalt ("sys1"), Gestalt ("sys2"), Gestalt ("sys3"));
		}

		public static Version OsVersion
		{
			get { return version; }
		}
	}
}
