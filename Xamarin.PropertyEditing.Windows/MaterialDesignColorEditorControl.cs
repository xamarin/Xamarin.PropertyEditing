using System;
using System.Windows;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Windows
{
	internal class MaterialDesignColorEditorControl
		: PropertyEditorControl
	{
		static MaterialDesignColorEditorControl ()
		{
			FocusableProperty.OverrideMetadata (typeof (MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (false));
			DefaultStyleKeyProperty.OverrideMetadata (typeof (MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (typeof (MaterialDesignColorEditorControl)));
		}

		public static readonly DependencyProperty NormalColorProperty = DependencyProperty.Register (
			"NormalColor", typeof(CommonColor?), typeof(MaterialDesignColorEditorControl), new FrameworkPropertyMetadata (default(CommonColor?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((MaterialDesignColorEditorControl)o).OnValueChanged()));

		public CommonColor? NormalColor
		{
			get { return (CommonColor?) GetValue (NormalColorProperty); }
			set { SetValue (NormalColorProperty, value); }
		}

		public static readonly DependencyProperty AccentColorProperty = DependencyProperty.Register (
			"AccentColor", typeof(CommonColor?), typeof(MaterialDesignColorEditorControl), new PropertyMetadata (default(CommonColor?)));

		public CommonColor? AccentColor
		{
			get { return (CommonColor?) GetValue (AccentColorProperty); }
			set { SetValue (AccentColorProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.normalColorPicker = GetTemplateChild ("normalColorPicker") as ChoiceControl;
			this.accentColorPicker = GetTemplateChild ("accentColorPicker") as ChoiceControl;

			if (this.normalColorPicker == null)
				throw new InvalidOperationException ($"{nameof (MaterialDesignColorEditorControl)} is missing a child ChoiceControl named \"normalColorPicker\"");
			if (this.accentColorPicker == null)
				throw new InvalidOperationException ($"{nameof (MaterialDesignColorEditorControl)} is missing a child ChoiceControl named \"accentColorPicker\"");

			this.normalColorPicker.SelectedItemChanged += (s, e) => {
				SetCurrentValue (NormalColorProperty, this.normalColorPicker.SelectedItem as CommonColor?);
				EnsureNormalAndAccentState ();
			};

			this.accentColorPicker.SelectedItemChanged += (s, e) => {
				SetCurrentValue (AccentColorProperty, this.accentColorPicker.SelectedItem as CommonColor?);
				EnsureNormalAndAccentState ();
			};
		}

		private ChoiceControl normalColorPicker;
		private ChoiceControl accentColorPicker;

		private void OnValueChanged ()
		{
			EnsureNormalAndAccentState ();
		}

		private void EnsureNormalAndAccentState ()
		{
			if (this.normalColorPicker == null)
				return;

			this.normalColorPicker.SelectedItem = NormalColor;
			this.accentColorPicker.SelectedItem = AccentColor;
		}
	}
}
