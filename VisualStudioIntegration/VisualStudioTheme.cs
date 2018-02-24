using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell.Interop;

namespace Xamarin.PropertyEditing.VisualStudioIntegration
{
	/// <summary>
	/// Create a ResourceDictionary containing all the theme resources, with values that match the
	/// current Visual Studio color theme. Clients that show the property editor inside
	/// Visual Studio (for Windows) should use this, so it has the right colors. The theme
	/// resource brush names here should be kept in sync with those in
	/// Xamarin.PropertyEditing/Xamarin.PropertyEditing.Windows/Themes.
	/// </summary>
	/// <example>
	/// To use, compile this source file as part of the client source (it's not included
	/// in the property editor assembly itself to avoid VS assembly dependencies there).
	/// Create the resource dictionary with:
	/// <code>
	/// themeResourceDictionary = new VisualStudioTheme ().CreateResourceDictionary (vsServiceProvider);
	/// </code>
	/// 
	/// Add the resource dictionary to the merged dictionaries for the control that hosts the property
	/// editor:
	/// <code>
	/// hostControl.Resources.MergedDictionaries.Add (themeResourceDictionary);
	/// </code>
	/// 
	/// Set the property editor to not supply theme colors itself:
	/// <code>
	/// propertyEditorPanel.ThemeManager.Theme = PropertyEditing.Themes.PropertyEditorTheme.None;
	/// </code>
	/// 
	/// And update the resource dictionary on VSColorTheme.ThemeChanged events:
	/// <code>
	/// void VSColorTheme_ThemeChanged (ThemeChangedEventArgs e)
	/// {
	///     hostControl.Resources.MergedDictionaries.Remove (themeResourceDictionary);
	///	    themeResourceDictionary = new VisualStudioTheme ().CreateResourceDictionary (this);
	///	    hostControl.Resources.MergedDictionaries.Add (themeResourceDictionary);
	/// }
	/// </code>
	/// </example>
	public class VisualStudioTheme
	{
		static readonly Guid categoryCider = new Guid ("92d153ee-57d7-431f-a739-0931ca3f7f70");
		static readonly Guid categoryEnvironment = new Guid ("624ed9c3-bdfd-41fa-96c3-7c824ea32e3d");
		static readonly Guid categorySearch = new Guid("f1095fad-881f-45f1-8580-589e10325eb8");
		static readonly Guid categoryTreeView = new Guid("92ecf08e-8b13-4cf4-99e9-ae2692382185");

		IVsUIShell5 vsUiShell5;
		ResourceDictionary resourceDictionary;

		public ResourceDictionary CreateResourceDictionary (IServiceProvider serviceProvider)
		{
			vsUiShell5 = serviceProvider.GetService (typeof (IVsUIShell)) as IVsUIShell5;
			if (vsUiShell5 == null)
				throw new Exception ("Couldn't get IVsUIShell5 service");

			resourceDictionary = new ResourceDictionary ();
			AddBrushes();
			return resourceDictionary;
		}

		void AddBrushes ()
		{
			AddBrush ("FocusVisualBorderBrush", "VS.Cider.ToolWindowTextColor");

			AddBrush ("ListBackgroundBrush", "VS.Cider.ListBackgroundColor");
			AddBrush ("PanelBackgroundBrush", "VS.Cider.ToolWindowColor");
			AddBrush ("PanelForegroundBrush", "VS.Cider.ToolWindowTextColor");
			AddBrush ("PanelGroupBackgroundBrush", "VS.Cider.ToolWindowGroupColor");
			AddBrush ("PanelGroupForegroundBrush", "VS.Cider.ToolWindowGroupTextColor");
			AddBrush ("PanelGroupSecondaryBackgroundBrush", "VS.Cider.ToolWindowGroupSecondaryColor");
			AddBrush ("PanelGroupSecondaryBorderBrush", "VS.Cider.ToolWindowColor");
			AddBrush ("PanelGroupSecondaryDimmedForegroundBrush", "VS.Cider.ToolWindowGroupSecondaryTextColor");
			AddBrush ("PanelHeaderBackgroundBrush", "VS.Cider.ToolWindowGroupColor");
			AddBrush ("PropertiesPanelIconBackgroundBrush", "VS.Cider.ToolWindowColor");
			//<SolidColorBrush x:Key="PopupDropShadowColor">#FFFFFFFF</SolidColorBrush>

			AddBrush ("AdvancedExpanderCollapsedForegroundBrush", "VS.TreeView.GlyphColor");
			AddBrush ("AdvancedExpanderMouseOverForegroundBrush", "VS.TreeView.SelectedItemInactiveGlyphMouseOverColor");
			AddBrush ("AdvancedExpanderMouseOverBorderBrush", "VS.Cider.ToggleMouseOverBorderColor");
			AddBrush ("AdvancedExpanderMouseOverBackgroundBrush", "VS.Cider.ToggleMouseOverColor");

			AddBrush ("ToggleItemOuterBorderBrush", "VS.Cider.ToggleOuterBorderColor");
			AddBrush ("ToggleItemMouseOverBackgroundBrush", "VS.Cider.ToggleMouseOverColor");
			AddBrush ("ToggleItemMouseOverBorderBrush", "VS.Cider.ToggleMouseOverBorderColor");
			AddBrush ("ToggleItemMouseOverForegroundBrush", "VS.Cider.ToggleMouseOverTextColor");
			AddBrush ("ToggleItemSelectedBackgroundBrush", "VS.Cider.ToggleSelectedColor");
			AddBrush ("ToggleItemSelectedBorderBrush", "VS.Cider.ToggleSelectedBorderColor");
			AddBrush ("ToggleItemSelectedForegroundBrush", "VS.Cider.ToggleSelectedTextColor");
			AddBrush ("ToggleItemBackgroundBrush", "VS.Cider.ToggleColor");
			AddBrush ("ToggleItemBorderBrush", "VS.Cider.ToggleBorderColor");
			AddBrush ("ToggleItemForegroundBrush", "VS.Cider.ToggleTextColor");
			AddBrush ("ToggleItemPressedBackgroundBrush", "VS.Environment.MainWindowButtonDownColor");
			AddBrush ("ToggleItemPressedBorderBrush", "VS.Environment.MainWindowButtonDownBorderColor");

			AddBrush ("ControlBorderBrush", "VS.Cider.ControlBorderColor");

			AddBrush ("MenuPopupBackgroundBrush", "VS.Cider.MenuColor");
			AddBrush ("MenuPopupBorderBrush", "VS.Cider.MenuBorderColor");
			AddBrush ("ListItemForegroundBrush", "VS.Cider.ListItemTextColor");
			//<SolidColorBrush x:Key="ListItemHighlightForegroundBrush">#F1F1F1</SolidColorBrush>
			AddBrush ("ListItemHighlightBorderBrush", "VS.Cider.ListItemMouseOverBorderColor");
			AddBrush ("ListItemHighlightBackgroundBrush", "VS.Cider.ListItemMouseOverColor");
			AddBrush ("ListItemDisabledForegroundBrush", "VS.Cider.ListItemDisabledTextColor");
			AddBrush ("ListItemSelectedBackgroundBrush", "VS.Cider.ListItemSelectedColor");
			AddBrush ("ListItemSelectedBorderBrush", "VS.Cider.ListItemSelectedBorderColor");
			AddBrush ("ListItemSelectedForegroundBrush", "VS.Cider.ListItemSelectedTextColor");
			AddBrush ("ListItemMouseOverBackgroundBrush", "VS.Cider.ListItemMouseOverColor");
			AddBrush ("ListItemMouseOverBorderBrush", "VS.Cider.ListItemMouseOverBorderColor");
			AddBrush ("ListItemMouseOverForegroundBrush", "VS.Cider.ListItemMouseOverTextColor");
			AddBrush ("MenuSeparatorBrush", "VS.Cider.MenuSeparatorColor");

			AddBrush ("PropertyMenuDotIconBorderBrush", "VS.Cider.PropertyDotBorderColor");
			//<SolidColorBrush x:Key="LiteralMarkerBrush">#55000000</SolidColorBrush>
			//<SolidColorBrush x:Key="ResourceMarkerBrush">#FF8BD44A</SolidColorBrush>

			//<SolidColorBrush x:Key="PropertyButtonBackgroundBrush">#F2F2F6</SolidColorBrush>
			//<SolidColorBrush x:Key="PropertyButtonBorderBrush">#717171</SolidColorBrush>
			//<SolidColorBrush x:Key="PropertyLocalValueBrush">#F1F1F1</SolidColorBrush>
			//<SolidColorBrush x:Key="PropertyBoundValueBrush">#FFCF00</SolidColorBrush>
			//<SolidColorBrush x:Key="PropertyResourceBrush">#00FF00</SolidColorBrush>

			AddBrush ("SearchControlBackgroundBrush", "VS.SearchControl.UnfocusedColor");
			AddBrush ("SearchControlBorderBrush", "VS.SearchControl.UnfocusedBorderColor");
			AddBrush ("SearchControlForegroundBrush", "VS.SearchControl.UnfocusedTextColor");
			AddBrush ("SearchControlActiveBorderBrush", "VS.SearchControl.MouseOverBorderColor");
			AddBrush ("SearchControlActiveBackgroundBrush", "VS.SearchControl.MouseOverBackgroundColor");
			AddBrush ("SearchControlActiveForegroundBrush", "VS.SearchControl.MouseOverBackgroundTextColor");
			AddBrush ("SearchControlButtonMouseOverBackgroundBrush", "VS.SearchControl.ActionButtonMouseOverColor");
			AddBrush ("SearchControlButtonMouseOverBorderBrush", "VS.SearchControl.ActionButtonMouseOverBorderColor");
			AddBrush ("SearchControlButtonPressedBackgroundBrush", "VS.SearchControl.ActionButtonMouseDownColor");
			AddBrush ("SearchControlButtonPressedBorderBrush", "VS.SearchControl.ActionButtonMouseDownBorderColor");
			AddBrush ("SearchControlWatermarkBrush", "VS.SearchControl.UnfocusedWatermarkTextColor");

			AddBrush ("InputBackgroundBrush", "VS.Cider.InputColor");
			AddBrush ("InputBorderBrush", "VS.Cider.InputBorderColor");
			AddBrush ("InputForegroundBrush", "VS.Cider.InputTextColor");

			AddBrush ("PopupBackgroundBrush", "VS.Cider.MenuColor");
			AddBrush ("PopupForegroundBrush", "VS.Cider.MenuTextColor");
			AddBrush ("PopupBorderBrush", "VS.Cider.MenuBorderColor");

			AddBrush ("ThicknessIconBackgroundBrush", "VS.Cider.ToolWindowGroupSecondaryColor");
			AddBrush ("ThicknessIconBorderBrush", "VS.Cider.ToolWindowGroupSecondaryColor");

			AddBrush ("DialogBackgroundBrush", "VS.Cider.DialogColor");
			AddBrush ("DialogForegroundBrush", "VS.Cider.DialogTextColor");
			AddBrush ("DialogBorderBrush", "VS.Cider.DialogBorderColor");

			//<SolidColorBrush x:Key="IconButtonSimpleBackgroundBrush" Color="Transparent" />
			//<SolidColorBrush x:Key="IconButtonSimpleBorderBrush" Color="Transparent" />
			AddBrush ("IconButtonForegroundBrush", "VS.Cider.CommandTextColor");
			AddBrush ("IconButtonMouseOverForegroundBrush", "VS.Cider.CommandMouseOverTextColor");
			AddBrush ("IconButtonPressedForegroundBrush", "VS.Cider.CommandPressedTextColor");

			AddBrush ("CategoryExpanderBorderBrush", "VS.Cider.ToolWindowGroupBottomBorderColor");

			AddBrush ("ComboBoxBackgroundBrush", "VS.Cider.ComboBoxColor");
			AddBrush ("ComboBoxBorderBrush", "VS.Cider.ComboBoxBorderColor");
			AddBrush ("ComboBoxForegroundBrush", "VS.Cider.ComboBoxTextColor");
			//<SolidColorBrush x:Key="ComboBoxSeparatorBrush" ComboBoxSeparatorBrush">#FF333337</SolidColorBrush
			AddBrush ("ComboBoxButtonBackgroundBrush", "VS.Cider.ComboBoxButtonColor");
			AddBrush ("ComboBoxButtonSeparatorBrush", "VS.Cider.ComboBoxButtonBorderColor");
			AddBrush ("ComboBoxMouseOverBackgroundBrush", "VS.Cider.ComboBoxMouseOverColor");
			AddBrush ("ComboBoxMouseOverBorderBrush", "VS.Cider.ComboBoxMouseOverBorderColor");
			//<SolidColorBrush x:Key="ComboBoxPressedBackgroundBrush">#FF3F3F46</SolidColorBrush>
			//<SolidColorBrush x:Key="ComboBoxPressedBorderBrush">#FF434346</SolidColorBrush>
			AddBrush ("ComboBoxButtonMouseOverBackgroundBrush", "VS.Cider.ComboBoxButtonMouseOverColor");
			AddBrush ("ComboBoxButtonMouseOverSeparatorBrush", "VS.Cider.ComboBoxButtonMouseOverBorderColor");
			AddBrush ("ComboBoxButtonPressedBackgroundBrush", "VS.Cider.ComboBoxButtonPressedColor");
			AddBrush ("ComboBoxButtonPressedSeparatorBrush", "VS.Cider.ComboBoxButtonPressedBorderColor");
			AddBrush ("ComboBoxPopupBackgroundBrush", "VS.Cider.ComboBoxPopUpColor");
			AddBrush ("ComboBoxPopupBorderBrush", "VS.Cider.ComboBoxPopUpBorderColor");

			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Fill" Color="Transparent"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Stroke" Color="#F1F1F1"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Checked.Fill" Color="#F1F1F1"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Checked.Stroke" Color="#F1F1F1"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Stroke" Color="#007ACC"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Fill" Color="Transparent"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Checked.Stroke" Color="#007ACC"/>
			//<SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Checked.Fill" Color="#007ACC"/>

			AddBrush ("VS.Environment.ScrollBarBackgroundBrush", "VS.Environment.ScrollBarBackgroundColor");
			AddBrush ("VS.Environment.ScrollBarBorderBrush", "VS.Environment.ScrollBarBorderColor");
			AddBrush ("VS.Environment.ScrollBarThumbBackgroundBrush", "VS.Environment.ScrollBarThumbBackgroundColor");
			AddBrush ("VS.Environment.ScrollBarThumbMouseOverBackgroundBrush",
				"VS.Environment.ScrollBarThumbMouseOverBackgroundColor");
			AddBrush ("VS.Environment.ScrollBarThumbPressedBackgroundBrush",
				"VS.Environment.ScrollBarThumbPressedBackgroundColor");
			AddBrush ("VS.Environment.ScrollBarArrowBackgroundBrush", "VS.Environment.ScrollBarArrowBackgroundColor");
			AddBrush ("VS.Environment.ScrollBarArrowGlyphBrush", "VS.Environment.ScrollBarArrowGlyphColor");
			AddBrush ("VS.Environment.ScrollBarArrowGlyphMouseOverBrush", "VS.Environment.ScrollBarArrowGlyphMouseOverColor");
			AddBrush ("VS.Environment.ScrollBarArrowGlyphPressedBrush", "VS.Environment.ScrollBarArrowGlyphPressedColor");
			AddBrush ("VS.Environment.ScrollBarArrowDisabledBackgroundBrush",
				"VS.Environment.ScrollBarArrowDisabledBackgroundColor");
		}

		void AddBrush (string ourKey, string vsKey)
		{
			int lastPeriod = vsKey.LastIndexOf ('.');
			if (lastPeriod == -1)
				throw new Exception ($"Invalid format for color key (no periods): {vsKey}");

			string categoryName = vsKey.Substring (0, lastPeriod);

			Guid categoryGuid;
			if (categoryName == "VS.Cider")
				categoryGuid = categoryCider;
			else if (categoryName == "VS.Environment")
				categoryGuid = categoryEnvironment;
			else if (categoryName == "VS.SearchControl")
				categoryGuid = categorySearch;
			else if (categoryName == "VS.TreeView")
				categoryGuid = categoryTreeView;
			else throw new Exception ($"Unknown theme brush category: {categoryName}");

			string longBrushName = vsKey.Substring (lastPeriod + 1);
			string shortBrushName;
			__THEMEDCOLORTYPE colorType;
			if (longBrushName.EndsWith ("TextColor")) {
				shortBrushName = longBrushName.Substring (0, longBrushName.Length - "TextColor".Length);
				colorType = __THEMEDCOLORTYPE.TCT_Foreground;
			} else if (longBrushName.EndsWith ("Color")) {
				shortBrushName = longBrushName.Substring (0, longBrushName.Length - "Color".Length);
				colorType = __THEMEDCOLORTYPE.TCT_Background;
			} else throw new Exception ($"Unknown color type (doesn't have TextColor or Color as suffix): {longBrushName}");

			uint win32Color = vsUiShell5.GetThemedColor (categoryGuid, shortBrushName, (uint)colorType);
			//Debug.Write ($"Color {ourKey} ({vsKey}) is: #{win32Color:X6}\n");

			byte[] bytes = BitConverter.GetBytes (win32Color);
			Color color = Color.FromArgb (bytes[3], bytes[2], bytes[1], bytes[0]);
			SolidColorBrush brush = new SolidColorBrush (color);

			resourceDictionary.Add (ourKey, brush);
		}
	}
}

