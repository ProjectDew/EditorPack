namespace AssetManagement
{
	using System;
	using System.Reflection;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using ExtensionMethods;

	using Object = UnityEngine.Object;

	public class Assets
	{
		public static Object Find (string searchFilter)
		{
			string[] guids = AssetDatabase.FindAssets (searchFilter);
		
			if (guids.IsNullOrEmpty ())
				return null;

			string path = AssetDatabase.GUIDToAssetPath (guids[0]);

			return AssetDatabase.LoadAssetAtPath (path, typeof (Object));
		}

		public static T Find<T> (string searchFilter) where T : Object
		{
			string[] guids = AssetDatabase.FindAssets (searchFilter);
		
			if (guids.IsNullOrEmpty ())
				return null;

			string path = AssetDatabase.GUIDToAssetPath (guids[0]);

			return AssetDatabase.LoadAssetAtPath<T> (path);
		}

		public static Object[] FindAll (string searchFilter)
		{
			string[] guids = AssetDatabase.FindAssets (searchFilter);
		
			if (guids.IsNullOrEmpty ())
				return new Object[0];

			Object[] assets = new Object[guids.Length];
			string path;

			for (int i = 0; i < guids.Length; i++)
			{
				path = AssetDatabase.GUIDToAssetPath (guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath (path, typeof (Object));
			}

			return assets;
		}

		public static T[] FindAll<T> (string searchFilter) where T : Object
		{
			string[] guids = AssetDatabase.FindAssets (searchFilter);
		
			if (guids.IsNullOrEmpty ())
				return new T[0];

			T[] assets = new T[guids.Length];
			string path;

			for (int i = 0; i < guids.Length; i++)
			{
				path = AssetDatabase.GUIDToAssetPath (guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<T> (path);
			}

			return assets;
		}

		public static T[] FindAll<T> (SearchOption searchOption) where T : Object
		{
			if (searchOption == SearchOption.CurrentSelection)
			{
				Object[] selectedObjects = Selection.objects;

				List<T> assets = new ();

				for (int i = 0; i < selectedObjects.Length; i++)
				{
					if (selectedObjects[i] is not T asset)
						continue;

					assets.Add (asset);
				}

				return assets.ToArray ();
			}

			string searchFilter = GetSearchFilter<T> (searchOption);

			return FindAll<T> (searchFilter);
		}

		public static string GetCurrentFolder ()
		{
			MethodInfo getActiveFolderPath = typeof (ProjectWindowUtil).GetMethod ("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
			string currentPath = (string)getActiveFolderPath.Invoke (null, null);

			if (currentPath.IsNullOrEmpty () || !currentPath.Contains ("Assets"))
				return "Assets/";

			if (currentPath[^1] != '/')
				currentPath = string.Concat (currentPath, "/");

			return currentPath;
		}

		public static ScriptableObject CreateScriptableObject (string assetName, Type assetType, bool overwriteExisting = false)
		{
			return CreateScriptableObject (GetCurrentFolder (), assetName, assetType, overwriteExisting);
		}

		public static ScriptableObject CreateScriptableObject (string folderName, string assetName, Type assetType, bool overwriteExisting = false)
		{
			if (!folderName.EndsWith ('/'))
				folderName = string.Concat (folderName, "/");

			string fullPath = string.Concat (folderName, assetName, ".asset");
		
			if (!overwriteExisting)
			{
				ScriptableObject existingAsset = (ScriptableObject)AssetDatabase.LoadAssetAtPath (fullPath, typeof (ScriptableObject));

				if (existingAsset != null)
					return existingAsset;
			}

			AssetDatabase.CreateAsset (ScriptableObject.CreateInstance (assetType), fullPath);
			AssetDatabase.Refresh ();

			return (ScriptableObject)AssetDatabase.LoadAssetAtPath (fullPath, assetType);
		}

		public static void NestAsset (SerializedObject serializedObject, Object nestedObject, string propertyPath)
		{
			if (serializedObject == null)
			{
				Debug.LogError ("The SerializedObject provided is null.");
				return;
			}

			if (nestedObject == null)
			{
				Debug.LogError ("The object that you are trying to nest is null.");
				return;
			}

			if (propertyPath.IsNullOrEmpty ())
			{
				Debug.LogError (string.Concat ("Invalid property path: \"", propertyPath, "\""));
				return;
			}

			SerializedProperty property = serializedObject.FindProperty (propertyPath);

			if (property == null)
			{
				Debug.LogError (string.Concat ("No property found at path: ", propertyPath));
				return;
			}

			if (property.isArray && property.propertyType != SerializedPropertyType.String)
			{
				NestAssetAsArrayElement (serializedObject, nestedObject, propertyPath);
				return;
			}

			serializedObject.Update ();

			property.objectReferenceValue = nestedObject;
		
			serializedObject.ApplyModifiedProperties ();
		}

		public static void UnnestAsset (SerializedObject serializedObject, Object nestedObject, string propertyPath)
		{
			if (serializedObject == null)
			{
				Debug.LogError ("The SerializedObject provided is null.");
				return;
			}

			if (nestedObject == null)
			{
				Debug.LogError ("The object that you are trying to unnest is null.");
				return;
			}

			if (propertyPath.IsNullOrEmpty ())
			{
				Debug.LogError (string.Concat ("Invalid property path: \"", propertyPath, "\""));
				return;
			}

			SerializedProperty property = serializedObject.FindProperty (propertyPath);

			if (property == null)
			{
				Debug.LogError (string.Concat ("No property found at path: ", propertyPath));
				return;
			}

			if (property.isArray && property.propertyType != SerializedPropertyType.String)
			{
				UnnestAssetAsArrayElement (serializedObject, nestedObject, propertyPath);
				return;
			}

			serializedObject.Update ();

			if (property.objectReferenceValue == nestedObject)
				property.objectReferenceValue = null;
		
			serializedObject.ApplyModifiedProperties ();
		}

		private static string GetSearchFilter<T> (SearchOption searchOption) where T : Object
		{
			string typeFilter = string.Concat ("t:", typeof (T).FullName);

			if (searchOption == SearchOption.All)
				return typeFilter;
			
			string folderPath = GetCurrentFolder ();

			if (!folderPath.EndsWith ('/'))
				folderPath = string.Concat (folderPath, "/");

			if (searchOption == SearchOption.CurrentFolderAndSubfolders)
				folderPath = string.Concat (folderPath, "**/");

			return string.Concat ("glob:\"", folderPath, "*.asset\" ", typeFilter);
		}

		private static void NestAssetAsArrayElement (SerializedObject serializedObject, Object nestedObject, string propertyPath)
		{
			SerializedArray array = new (serializedObject, propertyPath);
		
			for (int i = 0; i < array.Count; i++)
				if (nestedObject == array.ObjectReference (i))
					return;

			serializedObject.Update ();

			array.Add (nestedObject);
		
			serializedObject.ApplyModifiedProperties ();
		}

		private static void UnnestAssetAsArrayElement (SerializedObject serializedObject, Object nestedObject, string propertyPath)
		{
			SerializedArray array = new (serializedObject, propertyPath);

			serializedObject.Update ();

			array.Remove (nestedObject);
		
			serializedObject.ApplyModifiedProperties ();
		}
	}
}
