using System;

namespace Xamarin.PropertyEditing.Common
{
	[Flags]
	public enum AutoResizingFlags
	{
		None                 = 0,
		FlexibleLeftMargin   = 1 << 0,
		FlexibleWidth        = 1 << 1,
		FlexibleRightMargin  = 1 << 2,
		FlexibleTopMargin    = 1 << 3,
		FlexibleHeight       = 1 << 4,
		FlexibleBottomMargin = 1 << 5,
		FlexibleMargins      = FlexibleBottomMargin | FlexibleTopMargin | FlexibleLeftMargin | FlexibleRightMargin,
		FlexibleDimensions   = FlexibleHeight | FlexibleWidth,
		All = FlexibleMargins | FlexibleDimensions
	}
}
