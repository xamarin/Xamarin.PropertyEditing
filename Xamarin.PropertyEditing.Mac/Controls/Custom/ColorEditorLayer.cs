using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace Xamarin.PropertyEditing.Mac
{
	abstract class ColorEditorLayer : CALayer
	{
		public ColorEditorLayer ()
		{
		}

        public override NSObject ActionForKey(string eventKey)
        {
			return null;
        }

        public ColorEditorLayer (IntPtr handle) : base (handle)
		{
		}

		abstract public void Commit (EditorInteraction interaction);
		abstract public void UpdateFromModel (EditorInteraction interaction);
		abstract public void UpdateFromLocation (EditorInteraction interaction, CGPoint location);
	}

	class CAGradientLayerQuick : CAGradientLayer {
		public CAGradientLayerQuick ()
		{
		}

		public CAGradientLayerQuick (IntPtr handle) : base (handle)
		{
		}

		public override NSObject ActionForKey (string eventKey)
		{
			return null;
		}
	}

	class CALayerQuick : CALayer {
		public CALayerQuick ()
		{
		}

		public CALayerQuick (IntPtr handle) : base (handle)
		{
		}

		public override NSObject ActionForKey (string eventKey)
		{
			return null;
		}
	}
}
