namespace CustomizedEditor
{
	using System;
	using UnityEngine;
	using UnityEditor;

	public static class Layout
	{
		public static void Line (float height, float spaceBefore, Color color)
		{
			if (spaceBefore > 0)
				GUILayout.Space (spaceBefore);

			EditorGUI.DrawRect (EditorGUILayout.GetControlRect (GUILayout.Height (height)), color);
		}

		public static void BlackLine (float spaceBefore = 0)
		{
			Line (1, spaceBefore, Color.black);
		}

		public static void GreyLine (float spaceBefore = 0)
		{
			Color color = new (0.4f, 0.4f, 0.4f, 1);
			Line (1, spaceBefore, color);
		}

		public static void Button (string buttonText, Action onClick, GUIStyle style, params GUILayoutOption[] layoutOptions)
		{
			if (GUILayout.Button (buttonText, style, layoutOptions))
				onClick?.Invoke ();
		}

		public static void Button (string buttonText, Action onClick, GUIStyle style, float width)
		{
			if (GUILayout.Button (buttonText, style, GUILayout.Width (width)))
				onClick?.Invoke ();
		}

		public static void Button (string buttonText, Action onClick, params GUILayoutOption[] layoutOptions)
		{
			if (GUILayout.Button (buttonText, layoutOptions))
				onClick?.Invoke ();
		}

		public static void Button (string buttonText, Action onClick, float width)
		{
			if (GUILayout.Button (buttonText, GUILayout.Width (width)))
				onClick?.Invoke ();
		}

		public static bool FoldoutButton (string label, SerializedProperty property, float spaceBefore = 0)
		{
			if (property == null)
			{
				CenteredContent (() => GUILayout.Label (label, Styles.BoldCenteredText (EditorStyles.helpBox)));
				Debug.LogError ("The property provided is null. The content will remain unfolded.");

				return true;
			}

			GUIStyle buttonStyle = property.isExpanded ? Styles.BoldCenteredText (EditorStyles.helpBox) : Styles.Style (EditorStyles.helpBox, FontStyle.Bold, TextAnchor.UpperCenter, opacity: 0.9f);

			CenteredContent (() => Button (label, () => property.isExpanded = !property.isExpanded, buttonStyle, Screen.width * 0.33f), spaceBefore);

			return property.isExpanded;
		}

		public static void CenteredContent (Action showElement, float spaceBefore = 0)
		{
			if (spaceBefore > 0)
				GUILayout.Space (spaceBefore);
		
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
		
			showElement?.Invoke ();

			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		public static void InlinedContent (Action drawLeftContent, Action drawRightContent, float spaceBefore = 0)
		{
			if (spaceBefore > 0)
				GUILayout.Space (spaceBefore);

			GUILayout.BeginHorizontal ();

			drawLeftContent?.Invoke ();

			GUILayout.FlexibleSpace ();

			drawRightContent?.Invoke ();

			GUILayout.EndHorizontal ();
		}

		public static void InlinedContent (Action drawLeftContent, Action drawMiddleContent, Action drawRightContent, float spaceBefore = 0)
		{
			if (spaceBefore > 0)
				GUILayout.Space (spaceBefore);

			GUILayout.BeginHorizontal ();

			drawLeftContent?.Invoke ();

			GUILayout.FlexibleSpace ();

			drawMiddleContent?.Invoke ();

			GUILayout.FlexibleSpace ();

			drawRightContent?.Invoke ();

			GUILayout.EndHorizontal ();
		}

		public static void InlinedContent (Action[] drawContents, float spaceBefore = 0)
		{
			if (spaceBefore > 0)
				GUILayout.Space (spaceBefore);

			GUILayout.BeginHorizontal ();

			for (int i = 0; i < drawContents.Length; i++)
			{
				if (i > 0)
					GUILayout.FlexibleSpace ();

				drawContents[i]?.Invoke ();
			}

			GUILayout.EndHorizontal ();
		}
	}
}
