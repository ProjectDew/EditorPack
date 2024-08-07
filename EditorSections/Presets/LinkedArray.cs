namespace EditorSections.Presets
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using AssetManagement;
	using EditorSections;
	using ExtensionMethods;

	using Object = UnityEngine.Object;

	public class LinkedArray : ISection
	{
		public LinkedArray (SerializedObject serializedObject, SerializedArray array, string inspectedObjectPathInLinkedProperty)
		{
			if (serializedObject == null)
				throw new ArgumentNullException ("serializedObject", "The SerializedObject provided in the constructor is null.");

			this.serializedObject = serializedObject;
			targetObjects = serializedObject.targetObjects;

			this.array = array ?? throw new ArgumentNullException ("array", "The SerializedArray provided in the constructor is null.");

			if (inspectedObjectPathInLinkedProperty.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is null or empty.", "inspectedObjectPathInLinkedArrays");

			this.inspectedObjectPathInLinkedProperty = inspectedObjectPathInLinkedProperty;

			RemoveDuplicates (serializedObject);
		}

		private const int arrayColumns = 2;

		private readonly SerializedArray array;

		private readonly SerializedObject serializedObject;
		private readonly Object[] targetObjects;
		private readonly string inspectedObjectPathInLinkedProperty;

		private string[] arrayIndices;

		public virtual void Draw ()
		{
			if (arrayIndices == null || arrayIndices.Length != array.Count)
				SetArrayIndices ();

			array.DrawInColumns (arrayColumns, DrawArrayElement);

			if (array.Count > 0)
				GUILayout.Space (10);
		
			Layout.CenteredContent (DrawObjectPicker);
		}

		private void RemoveDuplicates (SerializedObject serializedObject)
		{
			List<int> indicesToRemove = new ();

			serializedObject.Update ();

			int offset = 1;

			for (int i = 0; i < array.Count - offset; i++)
				for (int j = i + offset; j < array.Count; j++)
					if (array[i] == array[j])
						if (!indicesToRemove.Contains (j))
							indicesToRemove.Add (j);
			
			int lastElement = indicesToRemove.Count - 1;

			for (int i = lastElement; i >= 0; i--)
				if (indicesToRemove[i] < array.Count)
					array.RemoveAt (indicesToRemove[i]);

			serializedObject.ApplyModifiedProperties ();
		}

		private void SetArrayIndices ()
		{
			arrayIndices = new string[array.Count];

			for (int i = 0; i < arrayIndices.Length; i++)
				arrayIndices[i] = (i + 1).ToString ();
		}

		private void DrawArrayElement (int index)
		{
			if (array.ObjectReference (index) == null)
			{
				array.RemoveAt (index);
				return;
			}

			Object asset = array.ObjectReference (index);
			float buttonWidth = Screen.width * 0.79f / arrayColumns - 25;

			int newIndex = index;
			newIndex = EditorGUILayout.Popup (newIndex, arrayIndices, Styles.CenteredText (EditorStyles.popup), GUILayout.Width (20));

			if (newIndex != index)
				array.Move (index, newIndex);

			Layout.Button (asset.name, () => Selection.activeObject = asset, Styles.BoldCenteredText (GUI.skin.button), buttonWidth);
			Layout.Button ("X", () => UnlinkAssets (array.ObjectReference (index)), 20);
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
			Assets.NestAsset (serializedObject, nestedObject, array.BaseProperty.propertyPath);

			for (int i = 0; i < targetObjects.Length; i++)
				Assets.NestAsset (new (nestedObject), targetObjects[i], inspectedObjectPathInLinkedProperty);
		}

		private void UnlinkAssets (Object nestedObject)
		{
			Assets.UnnestAsset (serializedObject, nestedObject, array.BaseProperty.propertyPath);
			
			for (int i = 0; i < targetObjects.Length; i++)
				Assets.UnnestAsset (new (nestedObject), targetObjects[i], inspectedObjectPathInLinkedProperty);
		}
	}
}
