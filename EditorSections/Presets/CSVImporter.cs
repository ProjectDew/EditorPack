namespace EditorSections.Presets
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using CSVUtility;
	using ExtensionMethods;

	public abstract class CSVImporter : ISection
	{
		public CSVImporter ()
		{
			ResetKeyword ();
		}

		private enum SearchOptions
		{
			[InspectorName ("exact")]
			EXACT_MATCH,

			[InspectorName ("contained")]
			CONTAINS_KEYWORD
		}

		private SearchOptions searchOptions;

		[SerializeField]
		private TextAsset sourceDocument;
	
		[SerializeField]
		private string[][] documentContent;
	
		[SerializeField]
		private string keyword;
	
		[SerializeField]
		private int selectedSearchOption, keywordColumn;
	
		[SerializeField]
		private int selectedColumn;

		[SerializeField]
		private Vector2 scrollPosition;
	
		private List<string[]> importedContent;

		protected virtual string[][] GetImportedContent ()
		{
			if (importedContent != null)
				return importedContent.ToArray ();
		
			return new string[0][];
		}

		public virtual void Draw ()
		{
			Layout.CenteredContent (DrawDocumentPicker);
		
			if (sourceDocument == null)
			{
				ResetKeyword ();
				return;
			}
		
			Layout.GreyLine (10);

			Layout.CenteredContent (ShowSearchKeywordInColumn, 10);

			Layout.GreyLine (10);

			Layout.CenteredContent (DisplayColumnSelector, 10);

			GUILayout.Space (10);

			scrollPosition = GUILayout.BeginScrollView (scrollPosition, GUI.skin.box);

			DisplayImportedContent ();

			GUILayout.EndScrollView ();

			Layout.GreyLine ();

			Layout.CenteredContent (() => Layout.Button ("Import to current folder", ProcessImportedContent), 10);

			Layout.CenteredContent (() => GUILayout.Label ("(This operation may take a few minutes for long sets of data.)", Styles.ItalicCenteredText ()), 5);
		}

		private void DrawDocumentPicker ()
		{
			sourceDocument = EditorGUILayout.ObjectField (sourceDocument, typeof (TextAsset), !EditorUtility.IsPersistent (sourceDocument)) as TextAsset;
		}

		private void ResetKeyword ()
		{
			keyword = string.Empty;
			keywordColumn = -1;
		}

		private void ShowSearchKeywordInColumn ()
		{
			Layout.Button ("Search keyword", ImportContent);

			keyword = EditorGUILayout.TextField (keyword, GUILayout.MaxWidth (Screen.width * 0.45f));

			searchOptions = (SearchOptions)EditorGUILayout.EnumPopup (searchOptions, Styles.CenteredText (EditorStyles.popup), GUILayout.Width (80));
			selectedSearchOption = (int)searchOptions;

			GUILayout.Label ("in column");

			keywordColumn = EditorGUILayout.IntField (keywordColumn, GUILayout.Width (20));
		}

		private void ImportContent ()
		{
			documentContent = CSVReader.Read (sourceDocument);

			if (documentContent.IsNullOrEmpty ())
			{
				Debug.LogError ("The document is empty or its data couldn't be read.");
				return;
			}

			int column = (keywordColumn > 0) ? keywordColumn - 1 : -1;

			if (column < 0 || column >= documentContent.ColumnCount ())
			{
				Debug.LogError ("The keyword column wasn't found.");
				return;
			}

			if (importedContent == null)
				importedContent = new ();
			else
				importedContent.Clear ();

			for (int i = 0; i < documentContent.Length; i++)
			{
				if (keyword.IsNullOrEmpty ())
				{
					importedContent.Add (documentContent[i]);
					continue;
				}

				if (selectedSearchOption == (int)SearchOptions.EXACT_MATCH)
				{
					if (documentContent[i][column] == keyword)
						importedContent.Add (documentContent[i]);
				}
				else if (selectedSearchOption == (int)SearchOptions.CONTAINS_KEYWORD)
				{
					if (documentContent[i][column].Contains (keyword))
						importedContent.Add (documentContent[i]);
				}
			}

			if (importedContent.Count == 0 || selectedColumn < 0 || selectedColumn >= importedContent[0].Length)
				selectedColumn = 0;
		}

		private void DisplayColumnSelector ()
		{
			if (importedContent.IsNullOrEmpty ())
			{
				GUILayout.Label ("There is no content to display.", Styles.ItalicCenteredText ());
				return;
			}

			GUILayout.Label ("Current column: ");

			Layout.Button ("<", ShowPreviousColumn, 20);

			int column = selectedColumn + 1;

			column = EditorGUILayout.IntField (column, GUILayout.Width (20));

			selectedColumn = column - 1;
			selectedColumn = Mathf.Clamp (selectedColumn, 0, importedContent[0].Length - 1);

			Layout.Button (">", ShowNextColumn, 20);
		}

		private void ShowPreviousColumn ()
		{
			if (selectedColumn > 0)
				selectedColumn--;
		}

		private void ShowNextColumn ()
		{
			if (selectedColumn < importedContent[0].Length - 1)
				selectedColumn++;
		}

		private void DisplayImportedContent ()
		{
			if (importedContent.IsNullOrEmpty ())
				return;

			GUILayoutOption width = GUILayout.Width (Screen.width * 0.93f);

			for (int i = 0; i < importedContent.Count; i++)
			{
				if (importedContent[i][keywordColumn - 1].IsNullOrEmpty ())
					Layout.CenteredContent (() => GUILayout.Label ("Empty key", Styles.BoldItalicCenteredText (), width), 5);
				else
					Layout.CenteredContent (() => GUILayout.Label (importedContent[i][keywordColumn - 1], Styles.BoldCenteredText (), width), 5);

				if (importedContent[i][selectedColumn].IsNullOrEmpty ())
					Layout.CenteredContent (() => GUILayout.Label ("Empty column", Styles.ItalicCenteredText (EditorStyles.helpBox), width));
				else
					Layout.CenteredContent (() => GUILayout.Label (importedContent[i][selectedColumn], EditorStyles.helpBox, width));
			}
		}

		protected abstract void ProcessImportedContent ();
	}
}
