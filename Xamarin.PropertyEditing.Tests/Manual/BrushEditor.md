# Brush Editor Manual Test Plan

Those manual tests should be performed on the standalone app.

## Brush properties are properly displayed in the property grid

1. Select "Mocked Sample 1".
	- The `ReadOnlySolidBrush` and `SolidBrush` properties are properly labeled and have a property menu button on the right.
	- The `ReadOnlySolidBrush` should appear as a column-wide yellow solid color, with opacity that shows the checkerboard pattern underneath.
	- The `SolidBrush` property should show as a column-wide blue solid color, with opacity that shows the checkerboard pattern underneath, but not as visible as for `ReadOnlySolidBrush`.

## Read-only brush properties are not clickable

1. Click on the `ReadOnlySolidBrush` colored rectangle.
	- Nothing opens, nothing happens.

## Writable brush properties show a popup on click

1. Click on the `SolidBrush` colored rectangle.
	- The brush editor appears in a popup.
2. Click outside of the popup.
	- The popup disappears.

## Brush type can be changed

1. Select "Mocked Sample 1". Click on the `SolidBrush` property preview box.
	- The popup opens.
2. Click on the no brush tab.
	- The "nobrush" tab gets selected.
	- The part of the popup below the brush type tabs disappears (the popup itself might move depending on available screen real estate).
	- The preview box shows no color, just the text "No brush".
9. Click on the solid brush tab.
	- The solid brush tab gets selected.
	- The solid brush editor appears back (the popup may move depending on available screen real estate) with the previously selected color.

## Brush popup is keyboard accessible

1. Tab from the previous property.
	- The brush preview box is focused but does not immediately open the popup.
2. Tab when focused on the brush preview box
	- Focus goes to the next property.
3. `Shift-tab` from the next property.
	- Focus goes back to the brush preview box, still no popup.
4. `Shift-tab` again.
	- Focus goes to the previous property.
5. Tab back to the brush preview box, then hit `space`.
	- The popup opens, and sets the focus on the first brush type tab.
6. Hit `Esc`.
	- The popup disappears and the focus goes back to the brush preview box.
7. Hit `space` again to re-open the popup. Inside the popup, `tab` / `shift-tab` between the tab bar, the color space drop-down if there's one, the color component text boxes, and the hex textbox.
	- Focus travels accordingly inside the popup but doesn't escape it.
8. Tab to the brush types tab bar, then press the 'n' key (shifted or not).
	- The "nobrush" tab gets selected.
	- The part of the popup below the brush type tabs disappears (the popup itself might move depending on available screen real estate).
	- The preview box shows no color, just the text "No brush".
9. Press the 's' key (shifted or not).
	- The solid brush tab gets selected.
	- The solid brush editor appears back (the popup may move depending on available screen real estate) with the previously selected color.

## Brush editor popup shows the current brush

1. Clicking on the blue preview for the `SolidBrush` property of a mocked sample button shows the correct data for the color:
	- shade cursor is near the top-right corner.
	- gradient for the shade picker shows shades of blue.
	- hue cursor is a little below center.
	- initial color, current color, last color all show the same color as the preview.
	- `R = 20`, `G = 120`, `B = 220`, `A = 94%`, `Hex = #F01478DC`, `H = 210°`, `L = 47%`, `S = 91%`, `B = 86%`, `C = 91%`, `M = 45%`, `Y = 0%`, `K = 14%`, `Opacity = 100%`.
2. Clicking on any brush property of the "Mocked WPF button" shows the popup with "no brush" selected.

## Shade picker changes the shade, not the hue

1. In a brush popup with solid brush active, click and hold inside the shade picker, then move around without letting go.
	- Shade picker cursor follows the mouse but stays inside the gradient.
	- Last color and initial color remain the same.
	- Hue remains unchanged, both in components when HSL and HBS modes are active, and the hue picker's cursor doesn't move.
	- Alpha channel remains unchanged (and its effects can be seen on the preview, initial color, last color, and current color).
	- Preview, current color, hex value change while the cursor is dragged around
	- Components (R, G, B, C, M, Y, K, S, B, L) change while the cursor is dragged around.

2. Let go of the mouse button.
	- Initial color remains unchanged.
	- Last color takes the current color.
	- If the cursor was let go of on the left or bottom edge of the shade picker (which correspond to gray or black, for which hue is undefined), the hue remains what it was before the cursor was dragged.

## Hue picker changes the hue, not the shade

1. In a brush popup with solid brush active, click and hold inside the hue picker, then move around without letting go.
	- Hue picker cursor follows the mouse but stays inside the gradient.
	- Last color and initial color remain the same.
	- Shade remains unchanged: the shade cursor doesn't move.
	- Alpha channel remains unchanged (and its effects can be seen on the preview, initial color, last color, and current color).
	- Preview, current color, hex value change while the cursor is dragged around.
	- Components R, G, B, C, M, Y, H change while the cursor is dragged around, while S, B, L, and K remain unchanged.

2. Let go of the mouse button.
	- Initial color remains unchanged.
	- Last color takes the current color.

## Initial color button resets the brush

1. Open the brush editor for the "Mocked Sample 1" button, then change the shade and hue.
2. Click the initial color button.
	- Preview, current color, last color, shade picker, hue picker, hex, and all components are reset to `R = 20`, `G = 120`, `B = 220`, `A = 94%`, `Hex = #F01478DC`, `H = 210°`, `L = 47%`, `S = 91%`, `B = 86%`, `C = 91%`, `M = 45%`, `Y = 0%`, `K = 14%`, `Opacity = 100%`.

## Switching back and forth between no brush and solid brush doesn't lose color selection

1. Open the brush editor for the "Mocked Sample 1" button, change the shade and hue, switch to no brush, then back to solid brush.
	- The color should be the same as before the switch to no brush.
2. Repeat the above check with a brush property from "Mocked WPF button".

## Color component entry mode can be changed

1. Open the brush editor for the "Mocked Sample 1" button, then click the label of one of the component boxes.
	- A menu opens, showing a choice of HLS, HSB, RGB, and CMYK.
2. Choose each of the options in turn
	- The correct component boxes are shown.
	- Each box shows a value with a unit, rounded to the nearest integer.
	- Each box shows a mini-gradient on the bottom (rainbow for H, preview of changing this component alone from minimum to maximum value, taking the alpha channel into account).
3. For each entry mode, change each of the components.
	- When focusing on a component, it switches from a number rounded to the nearest integer with a unit where applicable, to a unitless number rounded to the nearest tenth.
	- When focusing, the value is selected.
	- A click on the text while the whole value is selected removes the selection and sets the cursor to the place that was clicked.
	- When focused, the mini-gradient temporarily disappears.
	- The value gets committed when either `return` or `tab` is pressed, or when focusing out.
	- The new value causes the rest of the brush editor to update accordingly and register the updated color as the new current selection.
4. Try invalid values such as non-numerical, or out of range, then focus out.
	- Validation should kick in and show the textbox with a red outline, and revert to the previous value.
	- Focusing back shows the invalid value entered before.
5. Fix the invalid value.
	- Border should go back to normal
	- New value should be committed.

## Colors can be entered with the hex box

1. Open the brush editor for the "Mocked Sample 1" button, focus on the hex textbox.
	- The whole value should be selected.
	- Clicking inside the box while the whole value is selected should deselect and position the cursor where the click happened.
	- Hitting `return`, `tab`, or focusing out commits the new value.
2. Change the value to "#e071a73c", hit return.
	- The value is capitalized to "#E071A73C".
	- The shade picker get a green gradient, and moves its cursor down and left.
	- The hue picker moves its cursor up to a green hue.
	- The preview, current color, and last color adopt the new color (including transparency that is a little more visible).
	- The initial color doesn't change.
	- Component values change to `R = 113`, `G = 167`, `B = 60`, `A = 88%`, `H = 90°`, `L = 45%`, `S = 64%`, `B = 65%`, `C = 32%`, `M = 0%`, `Y = 64%`, `K = 35%`, `Opacity = 100%`

## Components can be entered sequentially

1. Open the brush editor for the "Mocked Sample 1" , then switch to the HLS view.
2. Enter a hue of 300.
	- The shade picker should change its gradient to purple, but the cursor should not move.
	- The hue picker shouldchange its cursor to purple.
	- Other components, as well as thepreview, current color, and last color should update.
3. Enter a lightness (L) of 0.
	- The preview, current color, and last color switch to a translucent black.
	- The shade picker gradient remains unchanged, but its cursor jumps down to the black edge on the bottom.
	- The hue picker doesn't move.
	- The hue still shows 300°.
	- Other components update.
4. Enter a lightness of 80.
	- The preview, current color, and last color switch back to a purplish pink.
	- The shade picker keeps its purple gradient unchanged, but its cursor jumps up towards the top.
	- The hue picker doesn't change.
	- The hue still shows 300°.
	- Other components update.
5. Enter a saturation (S) of 0.
	- The preview, current color, and last color switch to a translucent white.
	- The shade picker gradient remains unchanged, but its cursor jumps down to the white edge on the left.
	- The hue picker doesn't move.
	- The hue still shows 300°.
	- Other components update.
6. Enter a saturation of 80.
	- The preview, current color, and last color switch back to a purplish pink.
	- The shade picker keeps its purple gradient unchanged, but its cursor jumps up towards the top-right.
	- The hue picker doesn't change.
	- The hue still shows 300°.
	- Other components update.
7. Switch to CMYK and enter 100 in the black (K) component.
	- The preview, current color, and last color switch to a translucent black.
	- The shade picker gradient remains unchanged, but its cursor jumps down to the lower-left corner.
	- The hue picker doesn't move.
	- The hue still shows 300°.
	- Other components update.
8. Enter a blackness of 10.
	- The preview, current color, and last color switch back to a purplish pink.
	- The shade picker keeps its purple gradient unchanged, but its cursor jumps up towards the top and slightly to the right.
	- The hue picker doesn't change.
	- The hue still shows 300°.
	- Other components update.
9. All the other components are less tricky, and trying them one by one will yield expectable results. Be sure to check each time that:
	- The preview, current color, and last color update.
	- The shade picker keeps up in both gradient hue and cursor position.
	- The hue picker updates its cursor position.
	- Other components update as relevant.

## The brush editor is properly themed

1. Open the brush editor and check that all UI elements are properly styled.
2. Switch the theme, and verify again.

## Brush opacity can be edited

1. Open the brush editor on a solid brush property, then deploy the advanced pane by clicking on the chevron on the bottom.
	- The opacity editor deploys and shows the current percentage (including a '%' sign after the value), rounded to the nearest unit.
2. Click the opacity textbox.
	- The opacity value gets selected, and is without unit, and rounded to the nearest tenth.
	- The opacity can be edited.

## Color space appears as needed and can be edited

1. Open a brush property on the "Mocked WPF button" and switch it to a solid brush.
	- No color space drop-down should be visible.
2. Open the `SolidBrush` property on "Mocked Sample 1".
	- A color space drop-down can be found between the brush type tabs and the solid brush editor.
	- The drop-down contains a "RGB" and a "sRGB" entry.
	- The "sRGB" option is selected.
3. Select the "RGB" option, then move to a different control and back.
	- The "RGB" option is still selected.

## Reset

1. Open a brush property on the "Mocked WPF button".
	- The property menu indicator is empty and the "Reset" option is disabled.
2. Switch it to a solid brush and select a shade and hue.
	- The property menu indicator is filled, and "Reset" is enabled.
3. Close the popup, then select "Reset" in the property menu.
	- The property is back to "No brush".
	- The property menu indicator is empty, and the reset option is disabled.
4. Re-open the popup.
	- The property menu indicator is still empty.
5. Switch to solid brush.
	- The property menu indicator is filled.
	- The selected color is black.

## Setting a brush property to a resource

TBD
