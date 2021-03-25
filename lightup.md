# Light Up Features

This documents some features that are not strictly required to be implemented by the interfaces and therefore
may go unnoticed.

## Go To Source

To implement this feature available through the property context button, the interface `ICanNavigateToSource` is implemented on `IPropertyInfo` instances. It provides an availability query through `bool CanNavigateToSource (IObjectEditor);` and a signal through `NavigateToSource (IObjectEditor)`. The integrator will route the signal from `NavigateToSource` to whatever mechanism they have to handle requests to open files and move to lines.

## Nameable objects

The name of an object (field name, x:Name, etc) holds a special place within the UI for the property editor. While you can certainly treat this as a normal string property, there is a special interface that elevates this property to the top level. If you implement this property you should _not_ report it through the normal `IObjectEditor.Properties`.

To support, implement `INameableObject` on your `IObjectEditor`. The API is a simple string get/set in `GetNameAsync()` and `SetNameAsync (string)`.

## Defaulted Open Categories

The implementing platform can specify property categories that should be opened by default (otherwise they are all collapsed). In `TargetPlatform.AutoExpandGroups`, add the category names that should be expanded by default.