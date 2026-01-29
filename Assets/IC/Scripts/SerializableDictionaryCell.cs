using UnityEngine;


/* 
 * This is a simple utility class to enable the serialization of two informations at once in the Unity Editor.
 * Using it with a List or array simulates the use of a Dictionary instance.
 * Even though it lacks the functionalities of a Dictionary, you may want to use it since Dictionaries aren't visible nor editable through the Editor.
 */
[System.Serializable]
public class SerializableDictionaryCell<T, K>
{
    public T key;
    public K value;
}