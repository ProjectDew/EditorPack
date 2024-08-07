namespace EditorSections.Presets
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;
	using EditorSections;
	using ExtensionMethods;

	using Object = UnityEngine.Object;

	public class LinkedProperty : ISection
	{
		public LinkedProperty (SerializedObject serializedObject, SerializedProperty property, string inspectedObjectPathInLinkedProperty)
		{
			if (serializedObject == null)
				throw new ArgumentNullException ("serializedObject", "The SerializedObject provided in the constructor is null.");
		
			this.serializedObject = serializedObject;
			inspectedObject = serializedObject.targetObject;

			this.property = property ?? throw new ArgumentNullException ("property", "The property provided in the constructor is null.");

			if (inspectedObjectPathInLinkedProperty.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is null or empty.", "inspectedObjectPathInLinkedProperties");

			this.inspectedObjectPathInLinkedProperty = inspectedObjectPathInLinkedProperty;
		}

		private readonly SerializedProperty property;
	
		private readonly SerializedObject serializedObject;
		private readonly Object inspectedObject;
		private readonly string inspectedObjectPathInLinkedProperty;

		public virtual void Draw ()
		{
			if (property.objectReferenceValue == null)
				Layout.CenteredContent (DrawObjectPicker);
			else
				Layout.CenteredContent (DrawSelectProperty);
		}

		private void DrawSelectProperty ()
		{
			GUIStyle style = Styles.CenteredText (GUI.skin.button);
			GUILayoutOption height = GUILayout.Height (20);
			GUILayoutOption width = GUILayout.Width (Screen.width * 0.33f);

			Layout.Button ("X", () => UnlinkAssets (property.objectReferenceValue), 20);
			Layout.Button (property.objectReferenceValue.name, () => Selection.activeObject = property.objectReferenceValue, style, height, width);
			Layout.Button ("X", () => UnlinkAssets (property.objectReferenceValue), 20);
		}

		private void DrawObjectPicker ()
		{
			Object linkedProperty = null;

			linkedProperty = EditorGUILayout.ObjectField (linkedProperty, typeof (Object), !EditorUtility.IsPersistent (linkedProperty));

			if (linkedProperty == null)
				return;

			LinkAssets (linkedProperty);
		}

		private void LinkAssets (Object nestedObject)
		{
			Assets.NestAsset (serializedObject, nestedObject, property.propertyPath);
			Assets.NestAsset (new (nestedObject), inspectedObject, inspectedObjectPathInLinkedProperty);
		}

		private void UnlinkAssets (Object nestedObject)
		{
			Assets.UnnestAsset (serializedObject, nestedObject, property.propertyPath);
			Assets.UnnestAsset (new (nestedObject), inspectedObject, inspectedObjectPathInLinkedProperty);
		}
	}
}
