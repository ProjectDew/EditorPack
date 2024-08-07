namespace EditorSections.Presets
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using EditorSections;
	using ExtensionMethods;

	public class LinkedObject : ISection
	{
		public LinkedObject (SerializedObject serializedObject, string nestedAssetPath, string inspectedObjectPathInNestedAsset)
		{
			if (serializedObject == null)
				throw new ArgumentNullException ("serializedObject", "The SerializedObject provided in the constructor is null.");

			if (nestedAssetPath.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is null or empty.", "nestedAssetPath");

			if (inspectedObjectPathInNestedAsset.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is null or empty.", "inspectedObjectPathInNestedAsset");

			SerializedProperty property = serializedObject.FindProperty (nestedAssetPath);

			if (property == null)
				throw new ArgumentException ("No property was found in the provided path.", "nestedAssetPath");

			if (property.isArray && property.propertyType != SerializedPropertyType.String)
			{
				SerializedArray array = new (serializedObject, nestedAssetPath);
				linkedObject = new LinkedArray (serializedObject, array, inspectedObjectPathInNestedAsset);

				return;
			}

			linkedObject = new LinkedProperty (serializedObject, property, inspectedObjectPathInNestedAsset);
		}

		[SerializeReference]
		private ISection linkedObject;

		public virtual void Draw ()
		{
			linkedObject.Draw ();
		}
	}
}
