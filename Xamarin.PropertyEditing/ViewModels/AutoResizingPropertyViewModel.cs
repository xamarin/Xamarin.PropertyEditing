using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.PropertyEditing.Common;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class AutoResizingPropertyViewModel
		: PropertyViewModel<AutoResizingFlags>
	{
		public AutoResizingPropertyViewModel (TargetPlatform platform, IPropertyInfo property, IEnumerable<IObjectEditor> editors, PropertyVariation variation = null)
			: base (platform, property, editors, variation)
		{
			ToggleMaskCommand = new RelayCommand<AutoResizingFlags> (ToggleFlag);
			CycleSizingCommand = new RelayCommand (OnCycleSizing);
		}

		public ICommand ToggleMaskCommand
		{
			get;
		}

		public ICommand CycleSizingCommand
		{
			get;
		}

		public bool LeftMarginFixed
		{
			get => (Value & AutoResizingFlags.FlexibleLeftMargin) != AutoResizingFlags.FlexibleLeftMargin;
			set => SetFlag (AutoResizingFlags.FlexibleLeftMargin, !value);
		}

		public bool RightMarginFixed
		{
			get => (Value & AutoResizingFlags.FlexibleRightMargin) != AutoResizingFlags.FlexibleRightMargin;
			set => SetFlag (AutoResizingFlags.FlexibleRightMargin, !value);
		}

		public bool TopMarginFixed
		{
			get => (Value & AutoResizingFlags.FlexibleTopMargin) != AutoResizingFlags.FlexibleTopMargin;
			set => SetFlag (AutoResizingFlags.FlexibleTopMargin, !value);
		}

		public bool BottomMarginFixed
		{
			get => (Value & AutoResizingFlags.FlexibleBottomMargin) != AutoResizingFlags.FlexibleBottomMargin;
			set => SetFlag (AutoResizingFlags.FlexibleBottomMargin, !value);
		}

		public bool HeightSizable
		{
			get => (Value & AutoResizingFlags.FlexibleHeight) == AutoResizingFlags.FlexibleHeight;
			set => SetFlag (AutoResizingFlags.FlexibleHeight, value);
		}

		public bool WidthSizable
		{
			get => (Value & AutoResizingFlags.FlexibleWidth) == AutoResizingFlags.FlexibleWidth;
			set => SetFlag (AutoResizingFlags.FlexibleWidth, value);
		}

		public CommonRectangle GetPreviewElementRectangle (CommonSize window, AutoResizingFlags flags)
		{
			const int Offset = 5;
			const double multiplier = .75;
			double width = 10, height = 10;
			double left;
			bool fixedLeft = !flags.HasFlag (AutoResizingFlags.FlexibleLeftMargin),
				fixedRight = !flags.HasFlag (AutoResizingFlags.FlexibleRightMargin);

			if (fixedLeft)
				left = Offset;
			else if (fixedRight)
				left = window.Width - Offset - width;
			else
				left = (window.Width / 2) - (width / 2);

			if (flags.HasFlag (AutoResizingFlags.FlexibleWidth)) {
				if (fixedRight && fixedLeft) {
					width = window.Width - (Offset * 2);
				} else if (fixedRight) {
					left = window.Width - (window.Width * multiplier);
					width = window.Width - left - Offset;
				} else if (fixedLeft) {
					width = (window.Width * multiplier) - Offset;
				} else {
					width = window.Width * multiplier;
					left = (window.Width - width) / 2;
				}
			}

			double top;
			bool fixedTop = !flags.HasFlag (AutoResizingFlags.FlexibleTopMargin),
				fixedBottom = !flags.HasFlag (AutoResizingFlags.FlexibleBottomMargin);

			if (fixedTop)
				top = Offset;
			else if (fixedBottom)
				top = window.Height - Offset - height;
			else
				top = (window.Height / 2) - (height / 2);

			if (flags.HasFlag (AutoResizingFlags.FlexibleHeight)) {
				if (fixedBottom && fixedTop) {
					height = window.Height - (Offset * 2);
				} else if (fixedBottom) {
					top = window.Height - (window.Height * multiplier);
					height = window.Height - top - Offset;
				} else if (fixedTop) {
					height = (window.Height * multiplier) - Offset;
				} else {
					height = window.Height * multiplier;
					top = (window.Height - height) / 2;
				}
			}

			return new CommonRectangle (left, top, width, height);
		}

		protected override void OnValueChanged ()
		{
			base.OnValueChanged ();

			OnPropertyChanged (nameof(LeftMarginFixed));
			OnPropertyChanged (nameof(RightMarginFixed));
			OnPropertyChanged (nameof(TopMarginFixed));
			OnPropertyChanged (nameof(BottomMarginFixed));
			OnPropertyChanged (nameof(HeightSizable));
			OnPropertyChanged (nameof(WidthSizable));
		}

		private void SetFlag (AutoResizingFlags flag, bool set)
		{
			Value = (set) ? Value | flag : Value & ~flag;
		}

		private void ToggleFlag (AutoResizingFlags flag)
		{
			Value = Value ^ flag;
		}

		private void OnCycleSizing ()
		{
			if (!WidthSizable && !HeightSizable) {
				WidthSizable = true;
			} else if (WidthSizable && !HeightSizable) {
				HeightSizable = true;
			} else if (WidthSizable && HeightSizable) {
				WidthSizable = false;
			} else if (!WidthSizable && HeightSizable) {
				HeightSizable = false;
			}
		}
	}
}