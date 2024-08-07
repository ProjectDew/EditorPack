namespace AssetManagement
{
	using UnityEngine;
	using UnityEditor;

	public class AssetInfo : EditorWindow
	{
		[SerializeReference]
		private AssetInfoSection mainSection;

		[SerializeField]
		private Vector2 scrollPosition;

		[MenuItem ("CONTEXT/ScriptableObject/Asset Info")]
		[MenuItem ("Assets/Asset Info")]
		public static void OpenAssetManager ()
		{
			AssetInfo window = GetWindow<AssetInfo> ();
			
			window.SetActiveObject (Selection.activeObject);
			window.Show ();
		}
		
		[MenuItem ("Assets/Asset Info", validate = true)]
		public static bool ValidateOpenAssetManager ()
		{
			if (Selection.activeObject is ScriptableObject)
				return true;

			return false;
		}

		public void SetActiveObject (Object activeObject)
		{
			SerializedObject serializedObject = new (activeObject);

			mainSection = new ();
			mainSection.Initialize (serializedObject);
		}

		private void Awake ()
		{
			titleContent = new GUIContent ("Asset Info");
			minSize = new Vector2 (400, 300);
		}

		private void OnGUI ()
		{
			if (mainSection == null)
			{
				EditorGUILayout.HelpBox ("The main section of the inspector is null.", MessageType.Error);
				return;
			}

			GUILayout.Space (10);

			scrollPosition = GUILayout.BeginScrollView (scrollPosition);

			mainSection.Draw ();

			GUILayout.EndScrollView ();

			GUILayout.Space (10);
		}
	}
}
