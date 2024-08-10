# EditorPack

This repository contains a number of scripts for the Unity Editor that are specifically tailored to my needs and preferences. They are divided into several namespaces and are designed to allow me to customize it more easily in less time (note that they are made for the IMGUI system).

## CustomizedEditor

- ***Layout***: a class that works as a sort of facade for some GUILayout and EditorGUILayout functions that I use often.

   - ***Line***: draws a horizontal line that can be used as a separator.
   - ***BlackLine***: draws a black horizontal line that can be used as a separator.
   - ***GreyLine***: draws a grey horizontal line that can be used as a separator.
   - ***Button (4 overloads)***: draws a simple button to perform a given action.
   - ***FoldoutButton***: draws a button that returns true when the given property is expanded and false when it's not.
   - ***CenteredContent***: centers the given content in the horizontal axis of the window.
   - ***InlinedContent (3 overloads)***: draws the contents horizontally, each one separated from the rest by flexible spaces.

- ***Properties***: an abstract class that I use to encapsulate the serialized properties of a class.

   - ***SerializedObject***: returns the serialized object that contains the properties.
   - ***TargetObject***: returns the Unity object that is being serialized.
   - ***GetTargetObjects***: returns the Unity objects that are being serialized.

- ***SerializedArray***: it works as a wrapper for serialized arrays and offers a more convenient way to use them. It implements IList (of type System.Object) from the System.Collections.Generic namespace.

  - ***SerializedObject***: returns the serialized object that contains the serialized array.
  - ***TargetObject***: returns the Unity object that is being serialized.
  - ***BaseProperty***: returns the property that is being handled.
  - ***ArrayProperty***: returns the property that contains the information about the array.
  - ***PropertyPath***: returns the path of the property.
  - ***GetArray (generic, 2 overloads)***: returns an array with all the elements contained in the serialized property.
  - ***GetPropertyAtIndex***: returns the serialized property at the specified index in the array.
  - ***GetFirstInstanceOf***: returns the first object that matches the one that is provided.
  - ***AnimationCurve***: returns the AnimationCurve value of the serialized property (equivalent to "animationCurveValue"). It can be used instead of the indexer to prevent boxing/unboxing operations.
    - *There are other 22 exposed methods akin to this one, each of them returning a different type of value, from bool to Vector4*.
  - ***Set (23 overloads)***: sets the value of the serialized property. It can be used instead of the indexer to prevent boxing/unboxing operations.

- ***SerializedMultiarray***: a class used by SerializedArray to handle multiselection (although it can also be used independently). It mostly contains the same methods than the SerializedArray class, so I'm considering a potential abstraction and refactoring here.

  - ***GetTargetObjects***: returns the Unity objects that are being serialized.

- ***Styles***: a class that serves as a more direct way of applying a GUIStyle to an Editor field. It exposes a number of predetermined styles that I use often.

  - ***Style (7 overloads)***: returns a GUIStyle with the desired settings.
  - ***BoldText***: returns a GUIStyle with the desired skin and bold text.
  - ***ItalicText***: returns a GUIStyle with the desired skin and italic text.
  - ***BoldItalicText***: returns a GUIStyle with the desired skin and bold, italic text.
  - ***CenteredText***: returns a GUIStyle with the desired skin and regular text that is displayed centered.
  - ***BoldCenteredText***: returns a GUIStyle with the desired skin and bold text that is displayed centered.
  - ***ItalicCenteredText***: returns a GUIStyle with the desired skin and italic text that is displayed centered.
  - ***BoldItalicCenteredText***: returns a GUIStyle with the desired skin and bold, italic text that is displayed centered.
  - ***RightAlignedText***: returns a GUIStyle with the desired skin and regular text that is aligned to the right.
  - ***BoldRightAlignedText***: returns a GUIStyle with the desired skin and bold text that is aligned to the right.
  - ***ItalicRightAlignedText***: returns a GUIStyle with the desired skin and italic text that is aligned to the right.
  - ***BoldItalicRightAlignedText***: returns a GUIStyle with the desired skin and bold, italic text that is aligned to the right.

## EditorSections

- ***ISection***: an interface that it's sort of my personal PropertyDrawer. I use this because it gives me more freedom to customize how things are displayed (arrays in particular).

  - ***Draw***: displays the contents of the section.

- ***IInspectorSection***: used for custom inspectors. It inherits from ISection.

  - ***Initialize***: receives the serialized object whose properties are going to be drawn and performs some other initialization tasks if necessary.

## EditorSections.Presets

- ***CSVImporter***: an abstract class that acts as a framework to import data from a CSV document. It implements ISection.

  - ***GetImportedContent (protected)***: returns the contents that have been imported from the document.
  - ***ProcessImportedContent (protected)***: abstract method. It handles the imported contents.

- ***LinkedProperty***: a class that allows to set a mutual reference between two objects (mainly scriptable objects) by dragging one into the serialized property of the other. It implements ISection.

- ***LinkedArray***: same as LinkedProperty, but each object can hold multiple mutual references. It implements ISection.

- ***LinkedObject***: a class that acts as a mini-facade for the previous two. I made it for pure convenience, as all it does is determining if a property is an array or not and then choosing the appropriate section to display (either LinkedProperty or LinkedArray). It implements ISection too.

## AssetManagement

- ***AssetInfo***: a window that shows some information about the selected asset, such as its name (which can be modifed), its GUID, its ID, its flags (which can be modified too) and a reference to the parent (if it's a subasset). If it's an asset that contains one or more subassets, the corresponding information about all of them will be displayed.

- ***AssetInfoSection***: a class that encapsulates the main section of the AssetInfo window. It implements IInspectorSection.

- ***Assets***: a class that handles assets.

  - ***Find***: returns the first object that matches the given criteria.
  - ***Find (generic)***: returns the first object of the desired type that matches the given criteria.
  - ***FindAll***: returns all the objects that match the given criteria.
  - ***FindAll (generic, 2 overloads)***: returns all the objects of the desired type that match the given criteria.
  - ***GetCurrentFolder***: returns the path of the active folder.
  - ***CreateScriptableObject (2 overloads)***: creates a ScriptableObject of the desired type.
  - ***NestAsset***: assigns the given object to a property in a serialized object. It checks if it's an array or not and handles it as a SerializedArray if it is.
  - ***UnnestAsset***: same as before, but it unassigns the object.

- ***SearchOption***: an enum used to filter objects in the Assets class.

## ExtensionMethods

- ***ColumnarArray***

  - ***DrawCentered (2 overloads)***: draws the elements of a SerializedArray in a single centered column.
  - ***DrawInColumns (2 overloads)***: draws the elements of a SerializedArray in the given amount of columns.

- ***EditorExtensions***

  - ***GetFieldType (2 overloads)***: returns the type of the field that is being handled by a serialized property or array.
