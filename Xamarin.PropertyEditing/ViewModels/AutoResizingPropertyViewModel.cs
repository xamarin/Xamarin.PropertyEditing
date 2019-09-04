using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.PropertyEditing.Common;

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
