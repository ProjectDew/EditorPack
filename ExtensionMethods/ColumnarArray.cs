namespace ExtensionMethods
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;

	using Object = UnityEngine.Object;

	public static class ColumnarArray
	{
		private const int minColumns = 1;

		private static readonly string columnsOutOfRange = "The number of columns must be equal to or greater than {0}.";

		public static void DrawCentered (this SerializedArray array, float spaceBefore = 0)
		{
			for (int i = 0; i < array.Count; i++)
				Layout.CenteredContent (() => array.DrawArrayElement (i), spaceBefore);
		}

		public static void DrawCentered (this SerializedArray array, Action<int> drawArrayElement, float spaceBefore = 0)
		{
			for (int i = 0; i < array.Count; i++)
				Layout.CenteredContent (() => drawArrayElement (i), spaceBefore);
		}

		public static void DrawInColumns (this SerializedArray array, int columns, float spaceBefore = 0)
		{
			if (columns < minColumns)
			{
				string message = string.Format (columnsOutOfRange, minColumns);
				Debug.Log (message);

				return;
			}

			DrawInColumns (array, columns, (i) => array.DrawArrayElement (i), spaceBefore);
		}

		public static void DrawInColumns (this SerializedArray array, int columns, Action<int> drawArrayElement, float spaceBefore = 0)
		{
			if (columns < minColumns)
			{
				string message = string.Format (columnsOutOfRange, minColumns);
				Debug.Log (message);

				return;
			}

			for (int i = 0; i < array.Count; i++)
			{
				if (spaceBefore > 0)
					GUILayout.Space (spaceBefore);
			
				if (i % columns == 0)
					GUILayout.BeginHorizontal ();

				GUILayout.FlexibleSpace ();

				drawArrayElement (i);
			
				GUILayout.FlexibleSpace ();

				if (i % columns == columns - 1 || i == array.Count - 1)
					GUILayout.EndHorizontal ();
			}
		}

		private static void DrawArrayElement (this SerializedArray array, int index)
		{
			DrawField (array, index);
			Layout.Button ("X", () => array.RemoveAt (index), 20);
		}

		private static void DrawField (SerializedArray array, int index)
		{
			Type type = array.GetFieldType ();

			if (type == typeof (AnimationCurve))
			{
				EditorGUI.BeginChangeCheck ();

				AnimationCurve value = EditorGUILayout.CurveField (array.AnimationCurve (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (bool))
			{
				EditorGUI.BeginChangeCheck ();

				bool value = EditorGUILayout.Toggle (array.Bool (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (BoundsInt))
			{
				EditorGUI.BeginChangeCheck ();

				BoundsInt value = EditorGUILayout.BoundsIntField (array.BoundsInt (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Bounds))
			{
				EditorGUI.BeginChangeCheck ();

				Bounds value = EditorGUILayout.BoundsField (array.Bounds (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Color))
			{
				EditorGUI.BeginChangeCheck ();

				Color value = EditorGUILayout.ColorField (array.Color (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (float))
			{
				EditorGUI.BeginChangeCheck ();

				float value = EditorGUILayout.FloatField (array.Float (index), Styles.CenteredText (EditorStyles.numberField));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (double))
			{
				EditorGUI.BeginChangeCheck ();

				double value = EditorGUILayout.DoubleField (array.Double (index), Styles.CenteredText (EditorStyles.numberField));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Gradient))
			{
				EditorGUI.BeginChangeCheck ();

				Gradient value = EditorGUILayout.GradientField (array.Gradient (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Hash128))
			{
				EditorGUILayout.PropertyField (array.BaseProperty);
			}
			else if (type == typeof (int))
			{
				EditorGUI.BeginChangeCheck ();

				int value = EditorGUILayout.IntField (array.Int (index), Styles.CenteredText (EditorStyles.numberField));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (long))
			{
				EditorGUI.BeginChangeCheck ();

				long value = EditorGUILayout.LongField (array.Long (index), Styles.CenteredText (EditorStyles.numberField));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (typeof (Object).IsAssignableFrom (type))
			{
				EditorGUI.BeginChangeCheck ();

				Object value = EditorGUILayout.ObjectField (array.ObjectReference (index), type, !EditorUtility.IsPersistent (array.TargetObject));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Quaternion))
			{
				EditorGUILayout.PropertyField (array.BaseProperty);
			}
			else if (type == typeof (RectInt))
			{
				EditorGUI.BeginChangeCheck ();

				RectInt value = EditorGUILayout.RectIntField (array.RectInt (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Rect))
			{
				EditorGUI.BeginChangeCheck ();

				Rect value = EditorGUILayout.RectField (array.Rect (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (string))
			{
				EditorGUI.BeginChangeCheck ();

				string value = EditorGUILayout.TextField (array.String (index), Styles.CenteredText (EditorStyles.textField));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (uint))
			{
				EditorGUILayout.PropertyField (array.BaseProperty);
			}
			else if (type == typeof (ulong))
			{
				EditorGUILayout.PropertyField (array.BaseProperty);
			}
			else if (type == typeof (Vector2Int))
			{
				EditorGUI.BeginChangeCheck ();

				Vector2Int value = EditorGUILayout.Vector2IntField ("", array.Vector2Int (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Vector2))
			{
				EditorGUI.BeginChangeCheck ();

				Vector2 value = EditorGUILayout.Vector2Field ("", array.Vector2 (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Vector3Int))
			{
				EditorGUI.BeginChangeCheck ();

				Vector3Int value = EditorGUILayout.Vector3IntField ("", array.Vector3Int (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Vector3))
			{
				EditorGUI.BeginChangeCheck ();

				Vector3 value = EditorGUILayout.Vector3Field ("", array.Vector3 (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else if (type == typeof (Vector4))
			{
				EditorGUI.BeginChangeCheck ();

				Vector4 value = EditorGUILayout.Vector4Field ("", array.Vector4 (index));

				if (EditorGUI.EndChangeCheck ())
					array.Set (index, value);
			}
			else
				EditorGUILayout.PropertyField (array.BaseProperty);
		}
	}
}
