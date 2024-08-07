namespace CustomizedEditor
{
	using UnityEngine;

	public class Styles
	{
		private static readonly Color textColor = new (1, 1, 1, 1);

		public static GUIStyle Style (FontStyle textWeight, TextAnchor textAlignment) => Style (GUI.skin.label, textWeight, textAlignment);

		public static GUIStyle Style (FontStyle textWeight) => Style (GUI.skin.label, textWeight, TextAnchor.UpperLeft);

		public static GUIStyle Style (TextAnchor textAlignment) => Style (GUI.skin.label, FontStyle.Normal, textAlignment);

		public static GUIStyle Style (GUIStyle style) => Style (style, FontStyle.Normal, TextAnchor.UpperLeft);

		public static GUIStyle Style (GUIStyle style, FontStyle textWeight) => Style (style, textWeight, TextAnchor.UpperLeft);

		public static GUIStyle Style (GUIStyle style, TextAnchor textAlignment) => Style (style, FontStyle.Normal, textAlignment);

		public static GUIStyle Style (GUIStyle style, FontStyle textWeight, TextAnchor textAlignment, int size = 12, float opacity = 0.8f)
		{
			GUIStyle guiStyle = new (style);

			Color color = new (opacity, opacity, opacity, textColor.a);

			guiStyle.fontSize = size;
			guiStyle.fontStyle = textWeight;
			guiStyle.alignment = textAlignment;
			guiStyle.normal.textColor = color;
			guiStyle.active.textColor = color;
			guiStyle.focused.textColor = color;
			guiStyle.hover.textColor = color;
		
			return guiStyle;
		}

		public static GUIStyle BoldText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.Bold);

			return Style (FontStyle.Bold);
		}

		public static GUIStyle ItalicText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.Italic);

			return Style (FontStyle.Italic);
		}

		public static GUIStyle BoldItalicText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.BoldAndItalic);

			return Style (FontStyle.BoldAndItalic);
		}

		public static GUIStyle CenteredText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, TextAnchor.UpperCenter);
		
			return Style (TextAnchor.UpperCenter);
		}

		public static GUIStyle BoldCenteredText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.Bold, TextAnchor.UpperCenter);
		
			return Style (FontStyle.Bold, TextAnchor.UpperCenter);
		}

		public static GUIStyle ItalicCenteredText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.Italic, TextAnchor.UpperCenter);

			return Style (FontStyle.Italic, TextAnchor.UpperCenter);
		}

		public static GUIStyle BoldItalicCenteredText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.BoldAndItalic, TextAnchor.UpperCenter);

			return Style (FontStyle.BoldAndItalic, TextAnchor.UpperCenter);
		}

		public static GUIStyle RightAlignedText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, TextAnchor.UpperRight);

			return Style (TextAnchor.UpperRight);
		}

		public static GUIStyle BoldRightAlignedText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.Bold, TextAnchor.UpperRight);
		
			return Style (FontStyle.Bold, TextAnchor.UpperRight);
		}

		public static GUIStyle ItalicRightAlignedText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.Italic, TextAnchor.UpperRight);

			return Style (FontStyle.Italic, TextAnchor.UpperRight);
		}

		public static GUIStyle BoldItalicRightAlignedText (GUIStyle backgroundStyle = null)
		{
			if (backgroundStyle != null)
				return Style (backgroundStyle, FontStyle.BoldAndItalic, TextAnchor.UpperRight);

			return Style (FontStyle.BoldAndItalic, TextAnchor.UpperRight);
		}
	}
}
