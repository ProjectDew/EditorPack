namespace CustomizedEditor
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using ExtensionMethods;

	using Object = UnityEngine.Object;

	public class SerializedMultiarray : IList<object>
	{
		public SerializedMultiarray (Object[] targetObjects, string propertyPath)
		{
			if (targetObjects.IsNullOrEmpty ())
				throw new ArgumentNullException (targetObjects.ToString (), "The array of objects provided in the constructor is null or doesn't contain any element.");

			if (propertyPath.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is not valid.", propertyPath.ToString ());

			SerializedObject serializedTarget;

			targets = new SerializedArray[targetObjects.Length];

			for (int i = 0; i < targetObjects.Length; i++)
			{
				if (targetObjects[i] == null)
					throw new NullReferenceException (string.Concat ("The target object at index ", i.ToString (), " is null."));

				serializedTarget = new (targetObjects[i]);
				targets[i] = new (serializedTarget, propertyPath);
			}

			targetObject = targetObjects[^1];
			serializedObject = new (targetObject);

			currentTarget = targets[^1];

			baseProperty = serializedObject.FindProperty (propertyPath);

			if (baseProperty == null)
				throw new Exception ("No property was found at the path provided in the constructor.");

			if (!baseProperty.isArray && baseProperty.propertyType != SerializedPropertyType.String)
				throw new Exception ("The property is not an array.");
		}
		
		public SerializedMultiarray (Object[] targetObjects, SerializedProperty parentProperty, string propertyPath)
		{
			if (targetObjects.IsNullOrEmpty ())
				throw new ArgumentNullException (targetObjects.ToString (), "The array of objects provided in the constructor is null or doesn't contain any element.");

			if (propertyPath.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is not valid.", propertyPath.ToString ());

			SerializedObject serializedTarget;

			targets = new SerializedArray[targetObjects.Length];

			for (int i = 0; i < targetObjects.Length; i++)
			{
				if (targetObjects[i] == null)
					throw new NullReferenceException (string.Concat ("The target object at index ", i.ToString (), " is null."));

				serializedTarget = new (targetObjects[i]);
				targets[i] = new (serializedTarget, parentProperty, propertyPath);
			}

			targetObject = targetObjects[^1];
			serializedObject = new (targetObject);

			currentTarget = targets[^1];

			baseProperty = parentProperty.FindPropertyRelative (propertyPath);

			if (baseProperty == null)
				throw new Exception ("No property was found at the path provided in the constructor.");

			if (!baseProperty.isArray && baseProperty.propertyType != SerializedPropertyType.String)
				throw new Exception ("The property is not an array.");
		}
		
		private readonly Object targetObject;

		private readonly SerializedObject serializedObject;
		private readonly SerializedProperty baseProperty;
		
		private readonly SerializedArray[] targets;
		private readonly SerializedArray currentTarget;

		public SerializedObject SerializedObject => serializedObject;

		public Object TargetObject => currentTarget.TargetObject;

		public SerializedProperty BaseProperty => currentTarget.BaseProperty;

		public SerializedProperty ArrayProperty => currentTarget.ArrayProperty;

		public string PropertyPath => currentTarget.PropertyPath;

		public int Count
		{
			get => currentTarget.Count;

			set
			{
				if (value < 0)
					value = 0;

				for (int i = 0; i < targets.Length; i++)
					ApplyChangesInMultiEditingMode (i, () => targets[i].Count = value);
			}
		}

		public bool IsReadOnly => false;

		public object this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException ();

				return currentTarget[index];
			}

			set
			{
				for (int i = 0; i < targets.Length; i++)
				{
					if (index < 0 || index >= targets[i].Count)
						continue;
					
					ApplyChangesInMultiEditingMode (i, () => targets[i][index] = value);
				}
			}
		}

		public void Clear () => Count = 0;

		public Object[] GetTargetObjects ()
		{
			Object[] targetObjects = new Object[targets.Length];

			for (int i = 0; i < targetObjects.Length; i++)
				targetObjects[i] = targets[i].TargetObject;

			return targetObjects;
		}

		public T[] GetArray<T> () => currentTarget.GetArray<T> ();

		public T[] GetArray<T> (int length) => currentTarget.GetArray<T> (length);

		public void CopyTo (object[] destinationArray, int startIndex) => currentTarget.CopyTo (destinationArray, startIndex);

		public SerializedProperty GetPropertyAtIndex (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.GetPropertyAtIndex (index);
		}

		public object GetFirstInstanceOf (object element) => currentTarget.GetFirstInstanceOf (element);

		public int IndexOf (object element) => currentTarget.IndexOf (element);

		public bool Contains (object element) => currentTarget.Contains (element);

		private void ApplyChangesInMultiEditingMode (int index, Action applyChanges)
		{
			targets[index].SerializedObject.Update ();

			applyChanges ();

			targets[index].SerializedObject.ApplyModifiedProperties ();
		}

		public void Add (object element)
		{
			for (int i = 0; i < targets.Length; i++)
				ApplyChangesInMultiEditingMode (i, () => targets[i].Insert (targets[i].Count, element));
		}

		public void Insert (int index, object newElement)
		{
			if (index < 0)
				throw new IndexOutOfRangeException ();

			for (int i = 0; i < targets.Length; i++)
			{
				int constrainedIndex = (index > targets[i].Count) ? targets[i].Count : index;
				ApplyChangesInMultiEditingMode (i, () => targets[i].Insert (constrainedIndex, newElement));
			}
		}

		public bool Remove (object element)
		{
			bool removed = false;

			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i].Contains (element))
					removed = true;

				ApplyChangesInMultiEditingMode (i, () => targets[i].Remove (element));
			}

			return removed;
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			object element = this[index];

			for (int i = 0; i < targets.Length; i++)
				ApplyChangesInMultiEditingMode (i, () => targets[i].Remove (element));
		}

		public void RemoveAll (object element)
		{
			for (int i = 0; i < targets.Length; i++)
				ApplyChangesInMultiEditingMode (i, () => targets[i].RemoveAll (element));
		}

		public void Move (int indexFrom, int indexTo)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (indexTo < 0 || indexTo >= targets[i].Count)
					continue;
				
				int elementIndex = targets[i].IndexOf (this[indexFrom]);

				if (elementIndex < 0 || elementIndex == indexTo)
					continue;

				ApplyChangesInMultiEditingMode (i, () => targets[i].Move (elementIndex, indexTo));
			}
		}

		public IEnumerator<object> GetEnumerator ()
		{
			object[] array = GetArray<object> ();
			return (IEnumerator<object>)array.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();

		public AnimationCurve AnimationCurve (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.AnimationCurve (index);
		}

		public bool Bool (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Bool (index);
		}

		public BoundsInt BoundsInt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.BoundsInt (index);
		}

		public Bounds Bounds (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Bounds (index);
		}

		public Color Color (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Color (index);
		}

		public double Double (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Double (index);
		}

		public float Float (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Float (index);
		}

		public Gradient Gradient (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Gradient (index);
		}

		public Hash128 Hash128 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Hash128 (index);
		}

		public int Int (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Int (index);
		}

		public long Long (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Long (index);
		}

		public Object ObjectReference (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.ObjectReference (index);
		}

		public Quaternion Quaternion (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Quaternion (index);
		}

		public RectInt RectInt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.RectInt (index);
		}

		public Rect Rect (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Rect (index);
		}

		public string String (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.String (index);
		}

		public uint UInt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.UInt (index);
		}

		public ulong ULong (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.ULong (index);
		}

		public Vector2Int Vector2Int (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Vector2Int (index);
		}

		public Vector2 Vector2 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Vector2 (index);
		}

		public Vector3Int Vector3Int (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Vector3Int (index);
		}

		public Vector3 Vector3 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Vector3 (index);
		}

		public Vector4 Vector4 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			return currentTarget.Vector4 (index);
		}

		public void Set (int index, AnimationCurve value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, bool value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, BoundsInt value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Bounds value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Color value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, double value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, float value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Gradient value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Hash128 value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, int value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, long value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Object value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Quaternion value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, RectInt value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Rect value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, string value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, uint value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, ulong value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Vector2Int value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Vector2 value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Vector3Int value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Vector3 value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}

		public void Set (int index, Vector4 value)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (index < 0 || index >= targets[i].Count)
					continue;
				
				ApplyChangesInMultiEditingMode (i, () => targets[i].Set (index, value));
			}
		}
	}
}
