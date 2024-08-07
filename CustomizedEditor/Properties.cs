namespace CustomizedEditor
{
	using UnityEngine;
	using UnityEditor;
	using ExtensionMethods;

	[System.Serializable]
	public abstract class Properties
	{
		public Properties (SerializedObject serializedObject)
		{
			if (serializedObject == null)
			{
				Debug.LogError ("The SerializedObject provided is null.");
				return;
			}

			this.serializedObject = serializedObject;

			targetObject = serializedObject.targetObject;
			targetObjects = serializedObject.targetObjects.Copy ();
		}
	
		public Properties (Object targetObject)
		{
			if (targetObject == null)
			{
				Debug.LogError ("The object provided is null.");
				return;
			}

			this.targetObject = targetObject;
			targetObjects = new Object[] { targetObject };

			serializedObject = new (targetObject);
		}

		private readonly SerializedObject serializedObject;

		private readonly Object targetObject;
		private readonly Object[] targetObjects;

		public virtual SerializedObject SerializedObject => serializedObject;

		public virtual Object TargetObject => targetObject;

		public virtual Object[] GetTargetObjects () => targetObjects.Copy ();
	}
}
