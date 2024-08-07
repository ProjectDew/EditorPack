namespace ExtensionMethods
{
	using System;
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	public static class EditorExtensions
	{
		private static readonly string missingField = "The \"{0}\" field wasn't found.";

		public static Type GetFieldType (this SerializedProperty property)
		{
			Type classType = property.serializedObject.targetObject.GetType ();
			FieldInfo fieldInfo = GetFieldInfo (classType, property.propertyPath);

			if (fieldInfo == null)
			{
				string message = string.Format (missingField, property.propertyPath);
				Debug.LogError (message);

				return null;
			}

			return fieldInfo.FieldType;
		}

		public static Type GetFieldType (this SerializedArray array)
		{
			Type classType = array.TargetObject.GetType ();
			FieldInfo fieldInfo = GetFieldInfo (classType, array.PropertyPath);

			if (fieldInfo == null)
			{
				string message = string.Format (missingField, array.PropertyPath);
				Debug.LogError (message);

				return null;
			}

			return fieldInfo.FieldType.GetElementType ();
		}

		private static FieldInfo GetFieldInfo (Type classType, string fieldPath)
		{
			FieldInfo fieldInfo = null;

			while (classType != null && fieldInfo == null)
			{
				fieldInfo = classType.GetField (fieldPath, BindingFlags.NonPublic | BindingFlags.Instance);
				classType = classType.BaseType;
			}

			return fieldInfo;
		}
	}
}
