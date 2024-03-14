using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public struct Item
{
    public GameObject itemObj;
    public Image itemPreview;
    public string itemName;
}

public struct ItemField
{
    public SerializedProperty objField;
    public SerializedProperty itemField;
    public SerializedProperty itemFieldName;
}