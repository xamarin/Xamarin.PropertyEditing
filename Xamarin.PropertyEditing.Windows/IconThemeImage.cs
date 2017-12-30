using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Xamarin.PropertyEditing.Windows
{
	sealed class IconThemeImage : MultiSourceImage
	{
		static readonly List<WeakReference<IconThemeImage>> instances = new List<WeakReference<IconThemeImage>> ();

		static IconThemeImage ()
		{
			PropertyEditorPanel.ThemeManager.ThemeChanged += (s, e) => {
				for (var i = instances.Count - 1; i >= 0; i--) {
					if (instances[i].TryGetTarget (out IconThemeImage image))
						image.ReloadIcon ();
					else
						instances.RemoveAt (i);
				}
			};
		}

		static void OnPropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> ((IconThemeImage)d).ReloadIcon ();

		readonly WeakReference<IconThemeImage> weakReference;

		public IconThemeImage ()
		{
			this.weakReference = new WeakReference<IconThemeImage> (this, true);
			instances.Add (this.weakReference);
		}

		public static readonly DependencyProperty IconNameProperty = DependencyProperty.Register (
				nameof (IconName),
				typeof (string),
				typeof (IconThemeImage),
				new PropertyMetadata (OnPropertyChanged));

		public string IconName
		{
			get => (string)GetValue (IconNameProperty);
			set => SetValue (IconNameProperty, value);
		}

		public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register (
				nameof (IconSize),
				typeof (int),
				typeof (IconThemeImage),
				new PropertyMetadata (OnPropertyChanged));

		public int IconSize
		{
			get => (int)GetValue (IconSizeProperty);
			set => SetValue (IconSizeProperty, value);
		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register (
				nameof (IsSelected),
				typeof (bool),
				typeof (IconThemeImage),
				new PropertyMetadata (OnPropertyChanged));

		public bool IsSelected
		{
			get => (bool)GetValue (IsSelectedProperty);
			set => SetValue (IsSelectedProperty, value);
		}

		static readonly Dictionary<string, List<BitmapImage>> iconCache
			= new Dictionary<string, List<BitmapImage>> ();

		void ReloadIcon ()
		{
			if (string.IsNullOrEmpty (IconName) || IconSize <= 0) {
				Source = null;
				return;
			}

			var iconName = PropertyEditorPanel.ThemeManager.GetIconName (
				IconName,
				IconSize,
				IsSelected);

			Width = IconSize;
			Height = IconSize;

			if (iconCache.TryGetValue (iconName, out List<BitmapImage> images)) {
				Sources = images;
				return;
			}

			images = new List<BitmapImage> ();
			iconCache.Add (iconName, images);

			var iconsDirectory = "Icons";

			foreach (var suffix in new[] { "", "@2x" }) {
				var path = $"pack://application:,,,/Xamarin.PropertyEditing.Windows;component/{iconsDirectory}/{iconName}{suffix}.png";
				images.Add (new BitmapImage (new Uri (path)));
			}

			Sources = images;
		}
	}
}