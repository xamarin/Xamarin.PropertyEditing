using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	abstract internal class ColorEditorLayer : CALayer
	{
		public ColorEditorLayer ()
		{
		}

		public override NSObject ActionForKey (string eventKey)
		{
			return null;
		}

		public ColorEditorLayer (NativeHandle handle) : base (handle)
		{
		}

		abstract public void Commit (EditorInteraction interaction);
		abstract public void UpdateFromModel (EditorInteraction interaction);
		abstract public void UpdateFromLocation (EditorInteraction interaction, CGPoint location);
	}

	internal class UnanimatedGradientLayer : CAGradientLayer
	{
		public UnanimatedGradientLayer ()
		{
		}

		public UnanimatedGradientLayer (NativeHandle handle) : base (handle)
		{
		}

		public override NSObject ActionForKey (string eventKey)
		{
			return null;
		}
	}

	internal class UnanimatedLayer : CALayer
	{
		public UnanimatedLayer ()
		{
		}

		public UnanimatedLayer (NativeHandle handle) : base (handle)
		{
		}

		public override NSObject ActionForKey (string eventKey)
		{
			return null;
		}
	}
}
