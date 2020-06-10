using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{

    /// <summary>
    /// Class with an editor UI to help select objects in the scene based on name, tagname or current selection
    /// </summary>
    public class ObjectGetter
    {

        //------------- FIELDS----------------
        public int _searchOptionsIndex = 0;
        public string[] _searchOptions = new string[] { "Manual selection", "Contains name", "By tag" };
        public List<TransformElement> _transformElements = new List<TransformElement>();
        public bool _showSelectedObjects;
        private Vector2 scrollPos;
        public string _TagSearchField = "Untagged";
        public string _nameSearchField;
        public static GameObject[] _gameObjects;
        //------------------------------------

        public ObjectGetter()
        {

        }

        public List<TransformElement> ObjectSelectionShowUi(string headerText)
        {
            EditorGUILayout.LabelField(headerText, EditorStyles.boldLabel);


            _searchOptionsIndex = EditorGUILayout.Popup(_searchOptionsIndex, _searchOptions);

            switch (_searchOptionsIndex)
            {
                case 0:
                    ManualSelection();
                    break;
                case 1:
                    SearchByName();
                    break;
                case 2:
                    SearchByTag();
                    break;

                default:
                    break;
            }


            EditorGUILayout.Space(5);
            if (_transformElements.Count > 0)
            {
                _showSelectedObjects = EditorGUILayout.Foldout(_showSelectedObjects, $"{_transformElements.Count} objects selected");
            }
            else if (_transformElements.Count == 0)
            {
                _showSelectedObjects = false;
                EditorGUILayout.LabelField("No objects selected");
            }

            if (_showSelectedObjects)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, EditorStyles.textArea, EditorStyles.textArea, EditorStyles.textArea, GUILayout.Width(300), GUILayout.Height(150));
                List<TransformElement> transformelementsToDelete = new List<TransformElement>();
                foreach (var te in _transformElements)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(te.TheGameObject.name);
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        transformelementsToDelete.Add(te);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (transformelementsToDelete.Count != 0)
                {
                    foreach (var te in transformelementsToDelete)
                    {
                        if (_transformElements.Contains(te))
                        {
                            _transformElements.Remove(te);
                        }
                    }
                    transformelementsToDelete.Clear();
                }
                EditorGUILayout.EndScrollView();

            }

            GUILayout.Space(10);


            void SearchByTag()
            {
                EditorGUILayout.BeginHorizontal();
                _TagSearchField = EditorGUILayout.TagField(_TagSearchField);
                if (GUILayout.Button("Search"))
                {
                    GameObject[] allObjects = GameObject.FindGameObjectsWithTag(_TagSearchField);
                    _transformElements.Clear();
                    foreach (var go in allObjects)
                    {
                        TransformElement te = new TransformElement(go, go.GetComponent<Transform>());
                        _transformElements.Add(te);
                    }
                }
                if (GUILayout.Button("Reset list"))
                {
                    _transformElements.Clear();
                }
                EditorGUILayout.EndHorizontal();
            }


            void SearchByName()
            {
                EditorGUILayout.BeginHorizontal();
                _nameSearchField = EditorGUILayout.TextField(_nameSearchField);
                if (GUILayout.Button("Search"))
                {
                    GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
                    _transformElements.Clear();
                    foreach (var go in allObjects)
                    {
                        if (go.name.Contains(_nameSearchField))
                        {
                            TransformElement te = new TransformElement(go, go.GetComponent<Transform>());
                            _transformElements.Add(te);
                        }
                    }
                }
                if (GUILayout.Button("Reset list"))
                {
                    _transformElements.Clear();
                }
                EditorGUILayout.EndHorizontal();
            }

            void ManualSelection()
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add selected"))
                {
                    _gameObjects = Selection.gameObjects;
                    foreach (var gameObject in _gameObjects)
                    {
                        TransformElement te = new TransformElement(gameObject, gameObject.GetComponent<Transform>());
                        if (!_transformElements.Contains(te)) _transformElements.Add(te);
                    }
                }

                if (GUILayout.Button("Reset list"))
                {
                    _transformElements.Clear();
                }
                EditorGUILayout.EndHorizontal();
            }
            return _transformElements;
        }

        

        /// <summary>
        /// Record the Transform state to allow the undo operation
        /// </summary>
        public static void RecordeUndoForSelectedObjects(List<TransformElement> transformelements, string undoLabel)
        {
            Transform[] undoableObjects = new Transform[transformelements.Count];
            for (int i = 0; i < transformelements.Count; i++)
            {
                undoableObjects[i] = transformelements[i].TheGameObject.GetComponent<Transform>();
            }
            Undo.RegisterCompleteObjectUndo(undoableObjects, undoLabel);
            Undo.FlushUndoRecordObjects();
        }

        public static void RecordeUndoForSelectedObjects(List<GameObject> gameobjects, string undoLabel)
        {
            Transform[] undoableObjects = new Transform[gameobjects.Count];
            for (int i = 0; i < gameobjects.Count; i++)
            {
                undoableObjects[i] = gameobjects[i].GetComponent<Transform>();
            }
            Undo.RegisterCompleteObjectUndo(undoableObjects, undoLabel);
            Undo.FlushUndoRecordObjects();
        }

        public void ClearList()
        {
            _transformElements.Clear();
        }
    }
}