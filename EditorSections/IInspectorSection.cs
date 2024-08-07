namespace EditorSections
{
	using UnityEditor;

	public interface IInspectorSection : ISection
	{
		void Initialize (SerializedObject serializedObject);
	}
}
