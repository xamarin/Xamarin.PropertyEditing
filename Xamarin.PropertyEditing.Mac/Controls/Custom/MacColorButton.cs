using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AppKit;
using CoreGraphics;
using CoreImage;
using Foundation;
using ObjCRuntime;

namespace Xamarin.PropertyEditing.Mac
{
	class MacColorButton : NSPopUpButton
	{
		NSColor UndefinedDefaultColor = NSColor.Black;
		static HashSet<NSColor> recentColorList = new HashSet<NSColor> ();
		static Dictionary<string, NSImage> colorPreviews = new Dictionary<string, NSImage> ();

		NSColor lastColor = NSColor.White;
		bool IsAttributedStringChooser;

		public enum Mode
		{
			WithText,
			IconOnly,
			IconWithLetter
		}

		enum ColorMode
		{
			Default,
			Predefined,
			Custom,
			NoColor
		}

		public enum IconSize
		{
			Small,
			Medium,
			Large
		}

		NSMenu recentMenu;

		Mode mode;
		PredefinedColor[] predefinedColors;
		NSColor defaultColor;
		string defaultColorName;
		const int predefinedColorStartingIndex = 4;

		// Raised when the color of the button is commited
		public event Action<NSColor> CommitEvent;
		// Raised when the color of the button is changed but not committed
		public event Action ChangedEvent;

		// ImagePosition == ImageOnly has a flawed baseline offset. Since they are all the same we hardcode the value here.
		public override nfloat BaselineOffsetFromBottom {
			get {
				return 5;
			}
		}

		public MacColorButton (Mode mode, PredefinedColor[] predefinedColors, NSColor defaultColor = null, string defaultColorName = null, bool isAttributedStringChooser = false)
		{
			this.IsAttributedStringChooser = isAttributedStringChooser;
			this.mode = mode;
			this.predefinedColors = predefinedColors;
			this.defaultColor = defaultColor;
			if (defaultColor != null)
				lastColor = defaultColor;
			this.defaultColorName = defaultColorName;
			TranslatesAutoresizingMaskIntoConstraints = false;

			Cell.UsesItemFromMenu = false;
			var menu = BuildColorMenu ();
			Menu = menu;
			ImagePosition = mode != Mode.WithText ? NSCellImagePosition.ImageOnly : NSCellImagePosition.ImageLeft;
			var firstItem = menu.ItemAt (0);
			SelectItem (-1);
			Cell.MenuItem = firstItem;
		}

		public void ShowIntederminateColor ()
		{
			SelectItem (-1);
			var newItem = new NSMenuItem ();
			if (mode == Mode.WithText)
				newItem.Title = string.Empty;
			newItem.Image = PrepareIconForColor (null, mode == Mode.WithText ? IconSize.Large : IconSize.Small);
			Cell.MenuItem = newItem;
			if (ChangedEvent != null)
				ChangedEvent ();
		}

		public static string ColorToString (NSColor color, bool forceAlpha = false)
		{
			CIColor ci = new CIColor (color);
			var r = (byte)(255d * ci.Red);
			var g = (byte)(255d * ci.Green);
			var b = (byte)(255d * ci.Blue);
			var a = (byte)(255d * ci.Alpha);
			int val = (a << 24) + (r << 16) + (g << 8) + b;
			if (a > 0 || forceAlpha)
				return "#" + val.ToString ("x8").ToUpper ();
			else
				return "#" + val.ToString ("x6").ToUpper ();
		}

		public static bool TryParseColor (string scolor, out NSColor color)
		{
			color = NSColor.Black;
			if (string.IsNullOrEmpty (scolor) || !scolor.StartsWith ("#"))
				return false;
			scolor = scolor.Substring (1);
			if (scolor.Length == 4 || scolor.Length == 3) {
				string r = "";
				foreach (var c in scolor)
					r += "" + c + c;
				scolor = r;
			}
			if (scolor.Length == 8 || scolor.Length == 6) {
				uint val;
				if (!uint.TryParse (scolor, NumberStyles.HexNumber, null, out val))
					return false;
				if (scolor.Length == 6)
					val |= 0xff000000;

				color = NSColor.FromRgba ((byte)((val >> 16) & 0xff), (byte)((val >> 8) & 0xff), (byte)(val & 0xff), (byte)((val >> 24) & 0xff));
				return true;
			}
			else
				return false;
		}

		string ToPredefinedColorString (object newColor)
		{
			if (!(newColor is NSColor))
				return (string)newColor;
			CIColor color = new CIColor ((NSColor)newColor);
			if (color.Red == color.Green && color.Green == color.Blue)
				return color.ToString ();
			return ColorToString ((NSColor)newColor, forceAlpha: true);
		}

		void ApplyColorChange (NSMenuItem item, ColorMode newMode, object newColor, bool commitValue = true, bool changedEvent = true)
		{
			string result = null;

			switch (newMode) {
				case ColorMode.Default:
					result = defaultColorName;
					break;
				case ColorMode.Predefined:
					result = ToPredefinedColorString (newColor);
					break;
				case ColorMode.Custom:
					result = ColorToString ((NSColor)newColor, forceAlpha: true);
					break;
			}

			if (newColor is NSColor)
				lastColor = (NSColor)newColor;

			var newItem = new NSMenuItem ();
			ImagePosition = NSCellImagePosition.ImageLeft;
			if (newMode == ColorMode.Custom) {
				newItem.Title = result;
				newItem.Image = PrepareIconForColor ((NSColor)newColor, IconSize.Medium );
			}
			else {
				//SynchronizeTitleAndSelectedItem ();
				if (mode == Mode.WithText)
					newItem.Title = item.Title;
				if (item.Image != null)
					newItem.Image = item.Image;
			}
			SelectItem (-1);
			Cell.MenuItem = newItem;

			if (commitValue && CommitEvent != null)
				CommitEvent (lastColor);
			else if (!commitValue && changedEvent && ChangedEvent != null)
				ChangedEvent ();
		}

		public void SetColor (NSColor color)
		{
			OnColorResponderColorChanged (color);
		}

		void AddColorToRecentList (NSColor newColor)
		{
			if (!recentColorList.Add (newColor))
				return;

			recentMenu.AddItem (MakeRecentColorMenuItem (newColor));
		}

		Tuple<string, NSColor>[] GetBaseColors ()
		{
			return typeof (NSColor).GetProperties (System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
				.Where (m => m.CanRead && m.PropertyType == typeof (NSColor))
				// TODO .Where (m => m.IsSupported ())
				.Select (p => new { Name = p.Name, Color = (NSColor)p.GetValue (null, null) })
				.Where (c => c.Color.ColorSpaceName == "NSCalibratedRGBColorSpace")
				.Select (c => Tuple.Create (c.Name, c.Color))
				.ToArray ();
		}

		NSMenuItem MakeRecentColorMenuItem (NSColor color)
		{
			var desc = ColorToString (color, forceAlpha: true);
			var item = new NSMenuItem (desc, (sender, e) => ApplyColorChange ((NSMenuItem)sender, ColorMode.Custom, color)) { Image = PrepareIconForColor (color, IconSize.Small) };
			return item;
		}

		NSImage GetDefaultColorIconImage (NSColor defaultColor, string defaultColorName)
		{
			var colorIconSize = mode == Mode.WithText ? IconSize.Medium : IconSize.Small;
			if (defaultColor == null && !string.IsNullOrEmpty (defaultColorName)) {
				NSColor c;
				PredefinedColor predefined = predefinedColors.FirstOrDefault (p => p.Name == defaultColorName);
				string colorDescription = null;
				colorDescription = predefined.ColorDescription;
				if (TryParseColor (colorDescription, out c)) {
					return PrepareIconForColor (c, colorIconSize, isForDefaultItem: false);
				}
			}
			return PrepareIconForColor (defaultColor, colorIconSize, isForDefaultItem: true);
		}

		void UpdateDefaultColor (NSColor defaultColor, string defaultColorName)
		{
			this.defaultColor = defaultColor;
			this.defaultColorName = defaultColorName;
			NSMenuItem defaultItem = Menu.ItemWithTitle ("Default Color"); // TODO Translation
			defaultItem.Image = GetDefaultColorIconImage (defaultColor, defaultColorName);
		}

		void UpdatePredefinedColors (PredefinedColor[] predefinedColors)
		{
			var colorIconSize = mode == Mode.WithText ? IconSize.Medium : IconSize.Small;
			var menu = Menu;

			foreach (var predefColor in this.predefinedColors) {
				NSMenuItem item = menu.ItemWithTitle (predefColor.DisplayName);
				menu.RemoveItem (item);
				item.Dispose ();
			}

			// Predefined colors
			this.predefinedColors = predefinedColors;
			foreach (var predefColor in predefinedColors.Reverse ()) {
				NSColor c;
				TryParseColor (predefColor.ColorDescription, out c);
				NSMenuItem item = new NSMenuItem (predefColor.DisplayName) { Image = PrepareIconForColor (c, colorIconSize) };
				item.Activated += (sender, e) => ApplyColorChange ((NSMenuItem)sender, ColorMode.Predefined, (object)predefColor.Name ?? (object)c);
				menu.InsertItem (item, predefinedColorStartingIndex);
			}
		}

		void UpdateColorMenu (PredefinedColor[] predefinedColors, NSColor defaultColor, string defaultColorName)
		{
			if (!this.predefinedColors.SequenceEqual (predefinedColors))
				UpdatePredefinedColors (predefinedColors);

			if (this.defaultColor != defaultColor || this.defaultColorName != defaultColorName)
				UpdateDefaultColor (defaultColor, defaultColorName);
		}

		NSMenu BuildColorMenu ()
		{
			var menu = new NSMenu ();
			menu.AutoEnablesItems = true;
			NSMenuItem item;
			var colorIconSize = mode == Mode.WithText ? IconSize.Medium : IconSize.Small;

			// Default placeholder
			item = new NSMenuItem ("Default Color", (sender, e) => ApplyColorChange ((NSMenuItem)sender, IsAttributedStringChooser ? ColorMode.NoColor : ColorMode.Default, null)) { // TODO Translation
				Image = GetDefaultColorIconImage (defaultColor, defaultColorName)
			};
			menu.AddItem (item);

			menu.AddItem (NSMenuItem.SeparatorItem);

			item = new NSMenuItem ("Recent Colors"); // TODO Translation
			recentMenu = new NSMenu () { AutoEnablesItems = false };
			foreach (var color in recentColorList)
				recentMenu.AddItem (MakeRecentColorMenuItem (color));
			item.Submenu = recentMenu;
			menu.AddItem (item);

			menu.AddItem (NSMenuItem.SeparatorItem);

			// Predefined colors
			foreach (var predefColor in predefinedColors) {
				NSColor c;
				TryParseColor (predefColor.ColorDescription, out c);
				item = new NSMenuItem (predefColor.DisplayName) { Image = PrepareIconForColor (c, colorIconSize) };
				item.Activated += (sender, e) => ApplyColorChange ((NSMenuItem)sender, ColorMode.Predefined, (object)predefColor.Name ?? (object)c);
				menu.AddItem (item);
			}

			menu.AddItem (NSMenuItem.SeparatorItem);
			foreach (var c in GetBaseColors ()) {
				nfloat r, g, b, a;
				c.Item2.GetRgba (out r, out g, out b, out a);
				var realColor = NSColor.FromColorSpace (NSColorSpace.GenericRGBColorSpace, new nfloat[] { r, g, b, a });
				menu.AddItem (new NSMenuItem (c.Item1, (sender, e) => ApplyColorChange ((NSMenuItem)sender, ColorMode.Predefined, realColor)) {
					Image = PrepareIconForColor (realColor)
				});
			}

			menu.AddItem (NSMenuItem.SeparatorItem);

			// Custom color
			item = new NSMenuItem ("Custom Colors...", (sender, e) => ShowColorPickerPopover ((NSMenuItem)sender, this)); // TODO Translation

			menu.AddItem (item);

			return menu;
		}

		static IPopover popover;
		NSObject closeObserver;


		public static void MakeFirstResponder (NSWindow sender)
		{
			if (popover != null)
				sender.MakeFirstResponder (((NSPopoverWrapper)popover).popover);
		}

		NSMenuItem colorMenuItem;

		void OnColorResponderColorChanged (NSColor newColor)
		{
			if (IsAttributedStringChooser && colorMenuItem == null)
				return;
			if (lastColor == newColor)
				return;

			lastColor = newColor;
			ApplyColorChange (colorMenuItem, ColorMode.Custom, newColor);
		}

		void HideColorMenuItem ()
		{
			if (colorMenuItem == null || mode == Mode.IconOnly)
				return;
			colorMenuItem.Hidden = true;
		}

		void ShowColorMenuItem ()
		{
			if (colorMenuItem == null || mode == Mode.IconOnly)
				return;
			colorMenuItem.Hidden = false;
		}

		void AfterCloseColorPanel ()
		{
			var colorPanel = NSColorPanel.SharedColorPanel;

			foreach (var subview in ColorPanelView.Subviews)
				if (subview.Class.Name == "NSColorPanelResizeDimple") {
					subview.Hidden = false;
					break;
				}

			colorPanel.SetTarget (null);
			colorPanel.SetAction (null);

			ShowColorMenuItem ();
			AddColorToRecentList (lastColor);
			colorMenuItem = null;
		}

		public void EnsureColorPanelClose ()
		{
			if (closeObserver == null)
				return;
			NSColorPanel colorPanel = NSColorPanel.SharedColorPanel;
			colorPanel.Close ();
		}

		public void ShowColorPickerPopover (NSMenuItem item, object senderWindow, NSColor color, CGRect relativePositioningRect = default (CGRect))
		{
			colorMenuItem = item;

			ApplyColorChange (colorMenuItem, ColorMode.Custom, color, false, false);

			HideColorMenuItem ();
			popover = GetOrCreateColorPickerPopover (color);

			if (popover == null)
				return;

			NSView positioningView = null;

			if (senderWindow != null)
				positioningView = senderWindow as NSView;
			if (positioningView == null)
				positioningView = this;

			popover.Show (relativePositioningRect,
				positioningView,
				NSRectEdge.MinYEdge);
		}

		public void ShowColorPickerPopover (NSMenuItem item, object senderWindow, CGRect relativePositioningRect = default (CGRect))
		{
			ShowColorPickerPopover (item, senderWindow, lastColor, relativePositioningRect);
		}

		public class ContainerView : NSView
		{
			NSView ColorPanelView;
			NSView ToolBarView;

			public Action ClosePopover {
				get; set;
			}

			public bool PopoverClosing {
				get; set;
			}

			public override NSAppearance Appearance {
				get {
					return base.Appearance;
				}
				set {
					base.Appearance = value;
					if (value != null) {
						if (ColorPanelView != null)
							UpdateColorPanelSubviewsAppearance (ColorPanelView, value);
						if (ToolBarView != null) {
							UpdateColorPanelSubviewsAppearance (ToolBarView, value);
							ToolBarView.WantsLayer = true;
							ToolBarView.Layer.BackgroundColor = new CGColor (0.0f, 0.0f, 0.0f, 0.0f); // TODO MacExtensions.ToCGColor (BaseStyles.General.BaseBackgroundColor);
						}
					}
				}
			}

			public void AddToolbarAndColorPanel (NSView colorPanelView, NSView toolBarView)
			{
				ColorPanelView = colorPanelView;
				ToolBarView = toolBarView;

				AddSubview (ColorPanelView);
				AddSubview (ToolBarView);
			}

			static void UpdateColorPanelSubviewsAppearance (NSView view, NSAppearance appearance)
			{
				if (view.Class.Name == "NSPageableTableView")
					((NSTableView)view).BackgroundColor = NSColor.Clear.UsingColorSpace (NSColorSpace.GenericRGBColorSpace);// TODO BaseStyles.General.BaseBackgroundColor.ToNSColor ();
				view.Appearance = appearance;

				foreach (var subview in view.Subviews)
					UpdateColorPanelSubviewsAppearance (subview, appearance);
			}
		}

		[Export ("changeColor:")]
		public void ChangeColor (NSObject sender)
		{
			var panel = sender as NSColorPanel;
			nfloat r, g, b, a;
			var nsColor = panel.Color.UsingColorSpace (NSColorSpace.GenericRGBColorSpace);
			nsColor.GetRgba (out r, out g, out b, out a);
			var color = NSColor.FromColorSpace (NSColorSpace.GenericRGBColorSpace, new nfloat[] { r, g, b, a });
			OnColorResponderColorChanged (color);
		}

#if !USE_NSCOLOR_PANEL
		static NSView ColorPanelView;
		static NSView ColorToolbarView;
#endif

		IPopover GetOrCreateColorPickerPopover (NSColor color)
		{
			var colorPanel = NSColorPanel.SharedColorPanel;

			colorPanel.SetTarget (null);
			colorPanel.SetAction (null);
			colorPanel.Color = color;
			colorPanel.SetTarget (this);
			colorPanel.SetAction (new Selector ("changeColor:"));
			colorPanel.Continuous = true;

#if !USE_NSCOLOR_PANEL

			var Container = new ContainerView () {
				Frame = new CGRect (CGPoint.Empty, colorPanel.Frame.Size)
			};

			popover?.Close ();

			popover = new NSPopoverWrapper (new NSPopover {
				Behavior = NSPopoverBehavior.Semitransient,
				ContentViewController = new NSViewController (null, null) { View = Container },
			});

			Container.ClosePopover = popover.Close;
			popover.Closed += (o, e) => {
				// If the popover we're closing is the active popover, then we should disconnect our
				// events. If it is *not* the active popover then we have already disconnected everything.
				if (o != popover)
					return;

				AfterCloseColorPanel ();
			};

			if (ColorPanelView == null || ColorToolbarView == null) {
				var superView = colorPanel.ContentView.Superview;
				int last = superView.Subviews.Length - 1;
				if (ColorPanelView == null)
					ColorPanelView = superView.Subviews[last - 1];
				if (ColorToolbarView == null)
					ColorToolbarView = superView.Subviews[last];
			}

			foreach (var subview in ColorPanelView.Subviews)
				if (subview.Class.Name == "NSColorPanelResizeDimple") {
					subview.Hidden = true;
					break;
				}

			Container.AddToolbarAndColorPanel (ColorPanelView, ColorToolbarView);
			return popover;
#else
			closeObserver = NSNotificationCenter.DefaultCenter.AddObserver (NSWindow.WillCloseNotification, OnCloseColorPanel, colorPanel);
			colorPanel.MakeKeyAndOrderFront (this);
			return null;
#endif
		}

		void OnCloseColorPanel (NSNotification notification)
		{
			if (closeObserver != null) {
				NSNotificationCenter.DefaultCenter.RemoveObserver (closeObserver);
				closeObserver = null;
			}
			AfterCloseColorPanel ();
		}

		NSImage PrepareIconForColor (NSColor color, IconSize iconSize = IconSize.Medium, bool isForDefaultItem = false, bool isForMultiselection = false)
		{
			string key = (color == null ? "nocolor" : color.ToString ()) + iconSize.ToString ();
			NSImage preMade;
			if (!isForDefaultItem && !isForMultiselection && colorPreviews.TryGetValue (key, out preMade))
				return preMade;

			const int RightPadding = 4;
			var size = GetSizeForIconSize (iconSize);
			var ib = new NSImage (new CGSize ((int)size.Width + RightPadding, (int)size.Height));
			ib.BackgroundColor = NSColor.Black.UsingColorSpace (NSColorSpace.GenericRGBColorSpace);
			ib.LockFocusFlipped (true);
			var rect = new CGRect (CGPoint.Empty, size);

			NSColor.Black.SetFill ();
			NSGraphics.FrameRectWithWidth (rect, 1);

			var innerRect = new CGRect (rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
			if (isForMultiselection) {
				// Draw the Default color well which is a white background crossed by a single red line
				NSColor.White.SetFill ();
				NSGraphics.RectFill (innerRect);
				/*if (mode == Mode.IconWithLetter)
					DrawIconLetter (ctx, ib.Width, ib.Height);*/
				nfloat xoffset = size.Width * .25f;

				NSColor.Red.Set ();
				NSBezierPath.StrokeLine (new CGPoint (xoffset, size.Height * .5f),
					new CGPoint (size.Width - xoffset, size.Height * .5f));
			}
			else if (color == null || (isForDefaultItem && color.AlphaComponent == 0)) {
				// Draw the Default color well which is a white background crossed by a single red line
				NSColor.White.SetFill ();
				NSGraphics.RectFill (innerRect);
				/*if (mode == Mode.IconWithLetter)
					DrawIconLetter (ctx, ib.Width, ib.Height);*/
				int xoffset = iconSize == IconSize.Small ? 1 : 2;

				NSColor.Red.Set ();
				NSBezierPath.StrokeLine (new CGPoint (xoffset, size.Height - 1),
					new CGPoint (size.Width - xoffset, 1));
			}
			else {
				//CIColor ci = new CIColor (color);
				//var nscolor = NSColor.FromCalibratedRgba (ci.Red, ci.Green, ci.Blue, ci.Alpha);
				color.DrawSwatchInRect (innerRect);
			}

			ib.UnlockFocus ();
			if (!isForDefaultItem && !isForMultiselection)
				colorPreviews[key] = ib;
			return ib;
		}

		// It's impossible to correctly place text layout
		/*void DrawIconLetter (Context ctx, int ctxWidth, int ctxHeight, char letter = 'x')
		{
			var layout = new TextLayout (ctx) {
				Text = char.ToUpper (letter).ToString (),
				Font = Font.FromName ("Monaco 6px"),
			};
			var textSize = layout.GetSize ();
			layout.Height = textSize.Height;
			layout.Width = textSize.Width;
			ctx.SetColor (Colors.Black);
			var x = Math.Floor ((ctxWidth - textSize.Width) / 2);
			var y = Math.Floor ((ctxHeight - textSize.Height) / 2);
			ctx.DrawTextLayout (layout, x, y);
		}*/

		CGSize GetSizeForIconSize (IconSize iconSize)
		{
			switch (iconSize) {
				case IconSize.Small:
					return new CGSize (10, 10);
				case IconSize.Large:
					return new CGSize (180, 10);
				case IconSize.Medium:
				default:
					return new CGSize (25, 10);
			}
		}

		public static double ToDesignerDouble (string value)
		{
			if (string.IsNullOrEmpty (value))
				return 0;

			double result;
			if (ToDesignerDoubleIfPossible (value, out result))
				return result;

			// Constraint multilpliers can be specified as '9:5'. we should treat this as the float '1.8'
			// They can also be specified as 9/5, which should be treated as the float 1.8 as well.
			var parts = value.Split (':');
			if (parts.Length != 2)
				parts = value.Split ('/');

			if (parts.Length == 2) {
				try {
					var first = double.Parse (parts[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					var second = double.Parse (parts[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					return first / second;
				}
				catch {
					throw new ArgumentException (string.Format ("The value '{0}' could not be parsed as a float", value));
				}
			}
			throw new NotSupportedException (string.Format ("Unsupported number format '{0}'", value));
		}

		static bool ToDesignerDoubleIfPossible (string value, out double number)
		{
			number = 0;
			return double.TryParse (value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out number);
		}

		public static NSColor ParseDefaultColorDesc (PredefinedColor[] predefinedColors, string desc, out string defaultColorName)
		{
			NSColor result = null;
			defaultColorName = null;
			// Either the values comes from a property or it's a constant color description
			if (desc.StartsWith ("NS")) {
				var components = desc.Split (' ');
				if (components.Length != 0) {
					switch (components[0]) {
						case "NSCalibratedWhiteColorSpace":
							var rgb = (nfloat)ToDesignerDouble (components[1]);
							var alpha = (nfloat)ToDesignerDouble (components[2]);
							result = NSColor.FromColorSpace (NSColorSpace.GenericRGBColorSpace, new nfloat[] { rgb, (nfloat)alpha });
							break;
					}
				}
			}
			else {
				/* var value = ctx.GetPropertyValue (desc);*/
				if (!string.IsNullOrEmpty (desc) && predefinedColors.Any (pc => pc.Name == desc))
					defaultColorName = desc;
				NSColor color;
				if (desc != null)
					if (TryParseColor (desc, out color))
						result = color;
			}
			return result;
		}
	}
}
