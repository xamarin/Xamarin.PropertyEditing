using System;

namespace Xamarin.PropertyEditing.Drawing
{
	/// <summary>
	/// Base class for brush descriptions.
	/// </summary>
	[Serializable]
	public abstract class CommonBrush : IEquatable<CommonBrush>
	{
		// TODO: add transforms

		public CommonBrush(double opacity = 1.0)
		{
			Opacity = opacity;
		}

		/// <summary>
		/// The opacity of the brush.
		/// </summary>
		public double Opacity { get; }

		public override bool Equals (object obj)
		{
			var brush = obj as CommonBrush;
			if (brush == null) return false;
			return Equals (brush);
		}

		public bool Equals (CommonBrush other)
		{
			return other != null &&
				   Opacity == other.Opacity;
		}

		public override int GetHashCode ()
		{
			return unchecked(1107944354 * -1521134295 + Opacity.GetHashCode ());
		}
	}
}
