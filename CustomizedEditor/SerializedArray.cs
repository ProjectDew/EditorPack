namespace CustomizedEditor
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using ExtensionMethods;

	using Object = UnityEngine.Object;

	public class SerializedArray : IList<object>
	{
		public SerializedArray (SerializedObject serializedObject, string propertyPath)
		{
			if (serializedObject == null)
				throw new ArgumentNullException (serializedObject.ToString (), "The SerializedObject provided in the constructor is null.");

			if (propertyPath.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is not valid.", "propertyPath");

			this.serializedObject = serializedObject;
			targetObject = serializedObject.targetObject;

			baseProperty = serializedObject.FindProperty (propertyPath);

			Initialize (propertyPath);
		}
		
		public SerializedArray (SerializedObject serializedObject, SerializedProperty parentProperty, string propertyPath)
		{
			if (serializedObject == null)
				throw new ArgumentNullException (serializedObject.ToString (), "The SerializedObject provided in the constructor is null.");
			
			if (parentProperty == null)
				throw new ArgumentNullException (parentProperty.ToString (), "The SerializedProperty provided in the constructor is null.");

			if (propertyPath.IsNullOrEmpty ())
				throw new ArgumentException ("The path provided in the constructor is not valid.", "propertyPath");

			this.serializedObject = serializedObject;
			targetObject = serializedObject.targetObject;

			baseProperty = parentProperty.FindPropertyRelative (propertyPath);

			Initialize (propertyPath, parentProperty);
		}
		
		private readonly Object targetObject;

		private readonly SerializedObject serializedObject;
		private readonly SerializedProperty baseProperty;

		private SerializedProperty arrayProperty;
		private SerializedProperty sizeProperty;
		
		private SerializedMultiarray multiarray;

		public SerializedObject SerializedObject
		{
			get
			{
				if (!serializedObject.isEditingMultipleObjects)
					return serializedObject;

				return multiarray.SerializedObject;
			}
		}

		public Object TargetObject
		{
			get
			{
				if (!serializedObject.isEditingMultipleObjects)
					return targetObject;

				return multiarray.TargetObject;
			}
		}

		public SerializedProperty BaseProperty
		{
			get
			{
				if (!serializedObject.isEditingMultipleObjects)
					return baseProperty.Copy ();

				return multiarray.BaseProperty;
			}
		}

		public SerializedProperty ArrayProperty
		{
			get
			{
				if (!serializedObject.isEditingMultipleObjects)
					return arrayProperty.Copy ();

				return multiarray.ArrayProperty;
			}
		}

		public string PropertyPath
		{
			get
			{
				if (!serializedObject.isEditingMultipleObjects)
					return baseProperty.propertyPath;

				return multiarray.PropertyPath;
			}
		}

		public int Count
		{
			get
			{
				if (!serializedObject.isEditingMultipleObjects)
					return sizeProperty.intValue;

				return multiarray.Count;
			}

			set
			{
				if (value < 0)
					value = 0;
				
				if (!serializedObject.isEditingMultipleObjects)
					sizeProperty.intValue = value;
				else
					multiarray.Count = value;
			}
		}

		public bool IsReadOnly => false;

		public object this[int index]
		{
			get
			{
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException ();

				if (!serializedObject.isEditingMultipleObjects)
					return arrayProperty.GetArrayElementAtIndex (index).boxedValue;

				return multiarray[index];
			}

			set
			{
				if (!serializedObject.isEditingMultipleObjects)
					arrayProperty.GetArrayElementAtIndex (index).boxedValue = value;
				else
					multiarray[index] = value;
			}
		}

		public void Clear () => Count = 0;

		public T[] GetArray<T> ()
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				T[] targetArray = new T[Count];

				for (int i = 0; i < targetArray.Length; i++)
					targetArray[i] = (T)this[i];

				return targetArray;
			}

			return multiarray.GetArray<T> ();
		}

		public T[] GetArray<T> (int length)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				T[] targetArray = new T[length];

				for (int i = 0; i < targetArray.Length; i++)
					targetArray[i] = (T)this[i];

				return targetArray;
			}

			return multiarray.GetArray<T> ();
		}

		public void CopyTo (object[] destinationArray, int startIndex)
		{
			for (int i = 0; i < Count; i++)
			{
				int destinationIndex = startIndex + i;
				destinationArray[destinationIndex] = this[i];
			}
		}

		public SerializedProperty GetPropertyAtIndex (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index);

			return multiarray.GetPropertyAtIndex (index);
		}

		public object GetFirstInstanceOf (object element)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				for (int i = 0; i < Count; i++)
					if (element.Equals (this[i]))
						return this[i];

				return null;
			}

			return multiarray.GetFirstInstanceOf (element);
		}

		public int IndexOf (object element)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				for (int i = 0; i < Count; i++)
				{
					if (element.Equals (this[i]))
						return i;
				}

				return -1;
			}

			return multiarray.IndexOf (element);
		}

		public bool Contains (object element)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				for (int i = 0; i < Count; i++)
					if (element.Equals (this[i]))
						return true;

				return false;
			}

			return multiarray.Contains (element);
		}

		private void Initialize (string propertyPath, SerializedProperty parentProperty = null)
		{
			if (baseProperty == null)
			{
				string message = string.Concat ("The \"", propertyPath, "\" property wasn't found.");
				throw new Exception (message);
			}

			if (!baseProperty.isArray && baseProperty.propertyType != SerializedPropertyType.String)
				throw new Exception ("The property is not an array.");

			if (serializedObject.isEditingMultipleObjects)
			{
				if (parentProperty == null)
					multiarray = new (serializedObject.targetObjects, propertyPath);
				else
					multiarray = new (serializedObject.targetObjects, parentProperty, propertyPath);
			}

			SerializedProperty copiedProperty = baseProperty.Copy ();
		
			copiedProperty.Next (true);
			arrayProperty = copiedProperty.Copy ();

			copiedProperty.Next (true);
			sizeProperty = copiedProperty.Copy ();
		}

		public void Add (object element)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				int index = Count;

				Count++;
			
				arrayProperty.GetArrayElementAtIndex (index).boxedValue = element;

				return;
			}
			
			multiarray.Add (element);
		}

		public void Insert (int index, object newElement)
		{
			if (index < 0)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
			{
				object nextElement;
				
				Count++;
				
				for (int i = index; i < Count; i++)
				{
					nextElement = arrayProperty.GetArrayElementAtIndex (i).boxedValue;
					arrayProperty.GetArrayElementAtIndex (i).boxedValue = newElement;
					newElement = nextElement;
				}
				
				return;
			}

			multiarray.Insert (index, newElement);
		}

		public bool Remove (object element)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				for (int i = 0; i < Count; i++)
				{
					if (!element.Equals (this[i]))
						continue;
				
					arrayProperty.DeleteArrayElementAtIndex (i);
					return true;
				}

				return false;
			}

			return multiarray.Remove (element);
		}

		public void RemoveAt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.DeleteArrayElementAtIndex (index);
				return;
			}

			multiarray.RemoveAt (index);
		}

		public void RemoveAll (object element)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				for (int i = 0; i < Count; i++)
				{
					if (!element.Equals (this[i]))
						continue;
				
					arrayProperty.DeleteArrayElementAtIndex (i);
				}

				return;
			}

			multiarray.RemoveAll (element);
		}

		public void Move (int indexFrom, int indexTo)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{	
				if (indexFrom == indexTo)
					return;
				
				if (indexFrom < 0 || indexFrom >= Count)
					throw new IndexOutOfRangeException ("The origin index is out of the bounds of the array.");
				
				if (indexTo < 0 || indexTo >= Count)
					throw new IndexOutOfRangeException ("The goal index is out of the bounds of the array.");
				
				arrayProperty.MoveArrayElement (indexFrom, indexTo);
				return;
			}

			multiarray.Move (indexFrom, indexTo);
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

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).animationCurveValue;

			return multiarray.AnimationCurve (index);
		}

		public bool Bool (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).boolValue;

			return multiarray.Bool (index);
		}

		public BoundsInt BoundsInt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).boundsIntValue;

			return multiarray.BoundsInt (index);
		}

		public Bounds Bounds (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).boundsValue;

			return multiarray.Bounds (index);
		}

		public Color Color (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).colorValue;

			return multiarray.Color (index);
		}

		public double Double (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).doubleValue;

			return multiarray.Double (index);
		}

		public float Float (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).floatValue;

			return multiarray.Float (index);
		}

		public Gradient Gradient (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).gradientValue;

			return multiarray.Gradient (index);
		}

		public Hash128 Hash128 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).hash128Value;

			return multiarray.Hash128 (index);
		}

		public int Int (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).intValue;

			return multiarray.Int (index);
		}

		public long Long (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).longValue;

			return multiarray.Long (index);
		}

		public Object ObjectReference (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).objectReferenceValue;

			return multiarray.ObjectReference (index);
		}

		public Quaternion Quaternion (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).quaternionValue;

			return multiarray.Quaternion (index);
		}

		public RectInt RectInt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).rectIntValue;

			return multiarray.RectInt (index);
		}

		public Rect Rect (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).rectValue;

			return multiarray.Rect (index);
		}

		public string String (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).stringValue;

			return multiarray.String (index);
		}

		public uint UInt (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).uintValue;

			return multiarray.UInt (index);
		}

		public ulong ULong (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).ulongValue;

			return multiarray.ULong (index);
		}

		public Vector2Int Vector2Int (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).vector2IntValue;

			return multiarray.Vector2Int (index);
		}

		public Vector2 Vector2 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).vector2Value;

			return multiarray.Vector2 (index);
		}

		public Vector3Int Vector3Int (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).vector3IntValue;

			return multiarray.Vector3Int (index);
		}

		public Vector3 Vector3 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).vector3Value;

			return multiarray.Vector3 (index);
		}

		public Vector4 Vector4 (int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException ();

			if (!serializedObject.isEditingMultipleObjects)
				return arrayProperty.GetArrayElementAtIndex (index).vector4Value;

			return multiarray.Vector4 (index);
		}

		public void Set (int index, AnimationCurve value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).animationCurveValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, bool value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).boolValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, BoundsInt value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).boundsIntValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Bounds value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).boundsValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Color value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).colorValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, double value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).doubleValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, float value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).floatValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Gradient value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).gradientValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Hash128 value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).hash128Value = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, int value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).intValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, long value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).longValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Object value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).objectReferenceValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Quaternion value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).quaternionValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, RectInt value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).rectIntValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Rect value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).rectValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, string value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).stringValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, uint value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).uintValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, ulong value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).ulongValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Vector2Int value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).vector2IntValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Vector2 value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).vector2Value = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Vector3Int value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).vector3IntValue = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Vector3 value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).vector3Value = value;
				return;
			}

			multiarray.Set (index, value);
		}

		public void Set (int index, Vector4 value)
		{
			if (!serializedObject.isEditingMultipleObjects)
			{
				arrayProperty.GetArrayElementAtIndex (index).vector4Value = value;
				return;
			}

			multiarray.Set (index, value);
		}
	}
}
