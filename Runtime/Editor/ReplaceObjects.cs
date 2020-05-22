// Anthony Ackermans
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Replace a selection of objects with a specified object or prefab
/// </summary>
public class ReplaceObjects : EditorWindow
{
    // FIELDS
    private List<TransformElement> _transformElementsToReplace = new List<TransformElement>();
    private List<GameObject> _instantiatedObjects = new List<GameObject>(); // List of all the new instantiated objects
    private ObjectGetter _objectGetter = new ObjectGetter();
    private GameObject _ReplacementObject;
    private bool _keepObjects;
    private bool _keepTag;
    private bool _matchScale;
    private bool _matchRotation;
    private bool _matchName;
    private string _newNameString;
    private bool _numbered;
    private bool _newName;
    string[] nameOptions = new string[] { "Same as prefab", "Same as originals", "Create new name" };
    private int _nameOptionsIndex = 0;
    private int _newNameIndex = 0;
    string format; // The format to use with String.Format for the leading zeros in a numbered name.

    // Add menu item 
    [MenuItem("Tools/Replace Objects")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ReplaceObjects));
    }

    private void OnGUI()
    {
        _transformElementsToReplace = _objectGetter.ObjectSelectionShowUi("Search for objects");
        EditorGUILayout.Space(10);
        _ReplacementObject =(GameObject)EditorGUILayout.ObjectField(_ReplacementObject, typeof(GameObject), true);
        EditorGUILayout.Space(20);
        GUILayout.Label("Options", EditorStyles.boldLabel);
        _keepObjects = EditorGUILayout.Toggle(new GUIContent("Keep original object", "Keep the original object"), _keepObjects);
        _keepTag = EditorGUILayout.Toggle(new GUIContent("Keep tag", "Keep the original tagname"), _keepTag);
        _matchScale = EditorGUILayout.Toggle(new GUIContent( "Match scale", "Should the new object match the scale of original object?"), _matchScale);
        _matchRotation = EditorGUILayout.Toggle(new GUIContent("Match rotation", "Should the new object match the rotation the original object?"), _matchRotation);


        _nameOptionsIndex = EditorGUILayout.Popup("Naming",_nameOptionsIndex, nameOptions);

        if (_nameOptionsIndex == 2)
        {
            EditorGUI.indentLevel++;
            _newNameString = EditorGUILayout.TextField(new GUIContent("New name", "Give the new object a new name"), _newNameString);
            _numbered = EditorGUILayout.Toggle(new GUIContent("Numbered", "Add a numbered suffix to every new object. For example: *_03"), _numbered);
            EditorGUI.indentLevel--;
        }


        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset Values"))
        {
            ResetValues();
        }
        if (GUILayout.Button("Replace objects"))
        {
            Replace();
        }

    }

    private void ResetValues()
    {
        _transformElementsToReplace.Clear();
        _keepObjects = false;
        _keepTag = false;
        _matchScale = false;
        _matchRotation = false;
        _matchName = false;
        _numbered = false;
        _ReplacementObject = null;
    }

    private void Replace()
    {
        _instantiatedObjects.Clear();

        format = _transformElementsToReplace.Count <= 1000 ? "{0:00}" : "{0:000}"; // Format with leading zeros depending on the amount of selected objects
        GameObject instantiatedObject;

        PrefabAssetType assetType = PrefabUtility.GetPrefabAssetType(_ReplacementObject);

        foreach (var te in _transformElementsToReplace)
        {
            bool hasparent = te.TheGameObject.transform.parent != null;

            if (assetType == PrefabAssetType.NotAPrefab)
            {
                if (hasparent)
                {
                    instantiatedObject = GameObject.Instantiate(_ReplacementObject, te.TheGameObject.transform.position, Quaternion.identity, te.TheGameObject.transform.parent);
                }
                else
                {
                    instantiatedObject = GameObject.Instantiate(_ReplacementObject, te.TheGameObject.transform.position, Quaternion.identity);

                }
            }
            else
            {
                if (hasparent)
                {
                    instantiatedObject = (GameObject)PrefabUtility.InstantiatePrefab(_ReplacementObject, te.TheGameObject.transform.parent);                   
                }
                else
                {
                    instantiatedObject = (GameObject)PrefabUtility.InstantiatePrefab(_ReplacementObject);
                }
                instantiatedObject.transform.localPosition = te.originalPosition;
            }
           

            

            if (_matchScale)
            {
                instantiatedObject.transform.localScale = te.originalScale;
            }

            if (_matchRotation)
            {
                instantiatedObject.transform.localRotation = te.originalRotation;
            }

            if (_keepTag)
            {
                instantiatedObject.tag = te.TheGameObject.tag;
            }

            if (_matchName)
            {
                instantiatedObject.name = te.TheGameObject.name;
            }

            switch (_nameOptionsIndex)
            {
                case 0:
                    instantiatedObject.name = _ReplacementObject.name;
                    break;

                case 1:
                    instantiatedObject.name = te.TheGameObject.name;
                    break;

                case 2:
                    NewNameInstantiatedObject(instantiatedObject);
                    break;
            }
            _instantiatedObjects.Add(instantiatedObject);

            Undo.RegisterCreatedObjectUndo(instantiatedObject, "Replace objects");

            if (!_keepObjects)
            {
                Undo.DestroyObjectImmediate(te.TheGameObject);
            }
        }
        _transformElementsToReplace.Clear();
    }

    private void NewNameInstantiatedObject(GameObject instantiatedObject)
    {
        string newName = "";
        newName = _newNameString;
        if (_numbered)
        {
            newName += "_" + string.Format( format, _newNameIndex);
            
        }
        instantiatedObject.name = newName;
        _newNameIndex++;
    }
}
