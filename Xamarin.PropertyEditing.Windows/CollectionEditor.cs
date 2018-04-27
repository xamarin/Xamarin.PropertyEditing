using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Xamarin.PropertyEditing.Windows
{
	[TemplatePart (Name = "launch", Type = typeof(ButtonBase))]
	internal class CollectionEditor
		: PropertyEditorControl
	{
		static CollectionEditor ()
		{
			DefaultStyleKeyProperty.OverrideMetadata (typeof(CollectionEditor), new FrameworkPropertyMetadata (typeof(CollectionEditor)));
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			if (!(GetTemplateChild ("launch") is ButtonBase button))
				throw new InvalidOperationException ("Missing 'launch' button in CollectionEditor template");

			var topLevel = this.FindPropertiesHost ();

			button.Click += (sender, args) => {
				var window = new CollectionEditorWindow (topLevel.Resources.MergedDictionaries) {
					DataContext = DataContext
				};

				window.ShowDialog ();
			};
		}
	}
}
