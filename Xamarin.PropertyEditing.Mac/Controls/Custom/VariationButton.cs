using System;

namespace Xamarin.PropertyEditing.Mac
{
	internal class VariationButton
		: FocusableCommandButton
	{
		private readonly IHostResourceProvider hostResources;

		private readonly string imageFocused;
		private readonly string imageUnfocused;

		public VariationButton (IHostResourceProvider hostResources, string imageFocused, string imageUnfocused)
		{
			if (hostResources == null)
				throw new ArgumentNullException (nameof (hostResources));

			this.hostResources = hostResources;

			if (string.IsNullOrEmpty(imageFocused))
				throw new ArgumentNullException (nameof (imageFocused));

			this.imageFocused = imageFocused;

			if (string.IsNullOrEmpty (imageUnfocused))
				throw new ArgumentNullException (nameof (imageUnfocused));

			this.imageUnfocused = imageUnfocused;

			Bordered = false;

			OnMouseEntered += (sender, e) => {
				ToggleFocusImage (true);
			};

			OnMouseExited += (sender, e) => {
				ToggleFocusImage ();
			};

			AppearanceChanged ();
		}

		private void ToggleFocusImage (bool focused = false)
		{
			Image = focused ? this.hostResources.GetNamedImage (this.imageFocused) : this.hostResources.GetNamedImage (this.imageUnfocused);
		}

		public sealed override void ViewDidChangeEffectiveAppearance ()
		{
			base.ViewDidChangeEffectiveAppearance ();

			AppearanceChanged ();
		}

		protected void AppearanceChanged ()
		{
			ToggleFocusImage ();
		}
	}
}
