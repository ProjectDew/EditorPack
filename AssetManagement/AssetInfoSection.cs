namespace AssetManagement
{
	using UnityEngine;
	using UnityEditor;
	using CustomizedEditor;
	using EditorSections;
	using ExtensionMethods;

	public class AssetInfoSection : IInspectorSection
	{
		[SerializeReference]
		private IInspectorSection[] childrenInfo;

		[SerializeField]
		private Object asset;

		[SerializeField]
		private string assetPath;
		
		[SerializeField]
		private Object parentAsset;
		
		[SerializeField]
		private string guid;
		
		[SerializeField]
		private long localID;

		private string assetName;

		private GUILayoutOption descriptorMinWidth, contentMinWidth;
		private GUIStyle labelStyle;

		private bool expandSubAssets;

		public void Initialize (SerializedObject serializedObject)
		{
			asset = serializedObject.targetObject;

			if (asset == null)
				return;

			assetPath = AssetDatabase.GetAssetPath (asset);

			AssetDatabase.TryGetGUIDAndLocalFileIdentifier (asset, out guid, out localID);

			if (AssetDatabase.IsMainAsset (asset))
			{
				Object[] childAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath (assetPath);

				childrenInfo = new IInspectorSection[childAssets.Length];

				for (int i = 0; i < childrenInfo.Length; i++)
				{
					SerializedObject serializedChild = new (childAssets[i]);

					childrenInfo[i] = new AssetInfoSection ();
					childrenInfo[i].Initialize (serializedChild);
				}

				return;
			}

			parentAsset = AssetDatabase.LoadMainAssetAtPath (assetPath);
		}

		public void Draw ()
		{
			if (asset == null)
			{
				EditorGUILayout.HelpBox ("Null asset.", MessageType.Error);
				return;
			}

			descriptorMinWidth = GUILayout.MinWidth (Screen.width * 0.25f);
			contentMinWidth = GUILayout.MinWidth (Screen.width * 0.65f);

			labelStyle = Styles.BoldItalicText ();
			
			Layout.CenteredContent (DrawAssetName);

			Layout.InlinedContent (() => GUILayout.Label ("GUID", labelStyle, descriptorMinWidth), () => GUILayout.Label (guid, contentMinWidth), 10);
			Layout.InlinedContent (() => GUILayout.Label ("ID", labelStyle, descriptorMinWidth), () => GUILayout.Label (localID.ToString (), contentMinWidth), 5);
			Layout.InlinedContent (() => GUILayout.Label ("FLAGS", labelStyle, descriptorMinWidth), DrawHideFlags, 5);

			if (AssetDatabase.IsSubAsset (asset))
			{
				Layout.InlinedContent (() => GUILayout.Label ("PARENT", labelStyle, descriptorMinWidth), () => Layout.Button (parentAsset.name, () => Selection.activeObject = parentAsset, contentMinWidth), 5);
				return;
			}

			if (!childrenInfo.IsNullOrEmpty ())
				DrawChildrenInfo ();
		}

		private void DrawAssetName ()
		{
			bool renameAsset = false;

			if (assetName.IsNullOrEmpty ())
				assetName = asset.name;

			Event e = Event.current;

			if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return))
				renameAsset = true;

			assetName = EditorGUILayout.TextField (assetName, Styles.CenteredText (EditorStyles.textField), contentMinWidth);
			
			if (!renameAsset)
				return;
			
			if (AssetDatabase.IsMainAsset (asset))
				AssetDatabase.RenameAsset (assetPath, assetName);
			else
				asset.name = assetName;

			assetPath = AssetDatabase.GUIDToAssetPath (guid);

			AssetDatabase.SaveAssets ();
		}

		private void DrawHideFlags ()
		{
			HideFlags hideFlags = asset.hideFlags;

			EditorGUI.BeginChangeCheck ();

			hideFlags = (HideFlags)EditorGUILayout.EnumFlagsField (hideFlags, Styles.CenteredText (EditorStyles.popup), contentMinWidth);

			if (!EditorGUI.EndChangeCheck ())
				return;
			
			asset.hideFlags = hideFlags;

			EditorUtility.SetDirty (asset);
			AssetDatabase.SaveAssets ();
		}

		private void DrawChildrenInfo ()
		{
			GUIStyle buttonStyle = Styles.BoldCenteredText (EditorStyles.helpBox);

			Layout.GreyLine (10);

			Layout.CenteredContent (() => Layout.Button ("SUB-ASSETS", () => expandSubAssets = !expandSubAssets, buttonStyle), 10);

			if (!expandSubAssets)
				return;

			for (int i = 0; i < childrenInfo.Length; i++)
			{
				if (childrenInfo[i] == null)
					continue;
				
				GUILayout.Space (10);

				childrenInfo[i].Draw ();

				if (i < childrenInfo.Length - 1)
					Layout.GreyLine (10);
			}
		}
	}
}
