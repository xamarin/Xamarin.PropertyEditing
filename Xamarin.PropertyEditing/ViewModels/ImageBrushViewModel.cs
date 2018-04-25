using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.PropertyEditing.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.PropertyEditing.ViewModels
{
	internal class ImageBrushViewModel : NotifyingObject
	{
		public ImageBrushViewModel (BrushPropertyViewModel parent)
		{
			Parent = parent ?? throw new ArgumentNullException (nameof (parent));
			parent.PropertyChanged += Parent_PropertyChanged;
		}

		public CommonImageBrush PreviousImageBrush { get; set; }

		public CommonTileMode TileMode {
			get => Parent.Value is CommonImageBrush imageBrush ? imageBrush.TileMode : CommonTileMode.None;
			set {
				if (TileMode != value) {
					Parent.Value = (Parent.Value as CommonImageBrush ?? new CommonImageBrush()).CopyWith (tileMode: value);
					OnPropertyChanged ();
				}
			}
		}

		public IEnumerable<KeyValuePair<CommonTileMode, string>> TileModeValues
			=> typeof (CommonTileMode).GetEnumValues ().Cast<CommonTileMode> ()
				.Select (v => new KeyValuePair<CommonTileMode, string> (v, typeof (CommonTileMode).GetEnumName (v)))
				.OrderBy (kvp => kvp.Key);

		public CommonStretch Stretch
		{
			get => Parent.Value is CommonImageBrush imageBrush ? imageBrush.Stretch : CommonStretch.None;
			set {
				if (Stretch != value) {
					Parent.Value = (Parent.Value as CommonImageBrush ?? new CommonImageBrush ()).CopyWith (stretch: value);
					OnPropertyChanged ();
				}
			}
		}

		public IEnumerable<KeyValuePair<CommonStretch, string>> StretchValues
			=> typeof (CommonStretch).GetEnumValues ().Cast<CommonStretch> ()
				.Select (v => new KeyValuePair<CommonStretch, string> (v, typeof (CommonStretch).GetEnumName (v)))
				.OrderBy (kvp => kvp.Key);

		public CommonImageSource ImageSource
		{
			get => Parent.Value is CommonImageBrush imageBrush ? imageBrush.ImageSource : null;
			set {
				if (ImageSource != value) {
					Parent.Value = (Parent.Value as CommonImageBrush ?? new CommonImageBrush ())
						.CopyWith (imageSource: value);
					OnPropertyChanged ();
				}
			}
		}

		public IReadOnlyList<KeyValuePair<CommonImageSource, string>> ImageSources
		{
			get => GetImageResourcesAsync ().Result
				.Where (r => r.Source.IsLocal)
				.Cast<Resource<CommonImageSource>>()
				.Select (r => new KeyValuePair<CommonImageSource, string>(r.Value, r.Name))
				.ToList();
		}

		public async Task<IReadOnlyList<Resource>> GetImageResourcesAsync()
		{
			if (Parent.ResourceProvider == null)
				return null;

			IReadOnlyList<Resource> resources = await Parent.ResourceProvider.GetResourcesAsync (this, ImageSourceProperty, CancellationToken.None);
			return resources;
		}

		private BrushPropertyViewModel Parent { get; }
		private static readonly ImageSourcePropertyInfo ImageSourceProperty = new ImageSourcePropertyInfo ();

		private void Parent_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (BrushPropertyViewModel.Value)) {
				OnPropertyChanged (nameof (ImageSource));
				OnPropertyChanged (nameof (TileMode));
				OnPropertyChanged (nameof (Stretch));
			}
		}

		private class ImageSourcePropertyInfo : IPropertyInfo
		{
			public string Name => "ImageSource";

			public string Description => "";

			public Type Type => typeof(CommonImageSource);

			public string Category => "";

			public bool CanWrite => true;

			public ValueSources ValueSources => throw new NotImplementedException ();

			public IReadOnlyList<PropertyVariation> Variations => throw new NotImplementedException ();

			public IReadOnlyList<IAvailabilityConstraint> AvailabilityConstraints => throw new NotImplementedException ();
		}
	}
}
