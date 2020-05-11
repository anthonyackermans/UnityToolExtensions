
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Randomize the transform values (position, rotation, scale) of selected objects
/// </summary>
public class RandomTransformer : EditorWindow
{
    public enum Axis { X, Y, Z };
    public enum RelationType { Absolute, Relative };
    private RelationType _positionType = RelationType.Relative, _rotationType = RelationType.Absolute, _scaleType = RelationType.Absolute;
    public static GameObject[] _gameObjects;
    public static List<TransformElement> _transformElements = new List<TransformElement>();
    public static List<Transform> _originalTransforms; // Keep a list of all the transforms before modifying them.
    private bool showPositionOptions;
    public Vector3 MaximunPositionExtends;
    public Vector3 MaximumRotationExtends;

    Vector3 _maximumAxisScale;

    bool _randomizePosition;
    Vector3 _posMinRange, _posMaxRange;


    bool _randomizerotation;
    Vector3 _rotMinRange, _rotMaxRange;


    bool _randomizeScale;
    float _minUniformScale = 1;
    float _maxUniformScale = 2;
    float _minUniformScaleLimit = 0;
    float _maxUniformScaleLimit = 10;
    bool _seperateAxisScale;
    Vector3 _scaleMinRange = Vector3.one;
    Vector3 _scaleMaxRange = Vector3.one;

    GUIStyle _rangeOptions = new GUIStyle();
    private bool _showSelectedObjects;
    private Vector2 scrollPos;
    string[] _searchOptions = new string[] { "Manual selection", "Contains name", "By tag" };
    int _searchOptionsIndex = 0;
    private string _TagSearchField = "Untagged";
    private string _nameSearchField;


    // Add menu item 
    [MenuItem("Tools/RandomTransformer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(RandomTransformer));

    }

    public struct TransformElement
    {
        public GameObject TheGameObject;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Vector3 originalScale;

        public TransformElement(GameObject go, Transform tr)
        {
            TheGameObject = go;
            originalPosition = tr.localPosition;
            originalRotation = tr.localRotation;
            originalScale = tr.localScale;
        }
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Search for transforms", EditorStyles.boldLabel);


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

        // POSITION
        #region
        _randomizePosition = EditorGUILayout.ToggleLeft("Randomize Position", _randomizePosition);
        if (_randomizePosition)
        {
            EditorGUI.indentLevel++;
            _positionType = (RelationType)EditorGUILayout.EnumPopup(_positionType);
            _posMinRange = EditorGUILayout.Vector3Field("Minimum position range", _posMinRange);
            _posMaxRange = EditorGUILayout.Vector3Field("Maximum position range", _posMaxRange);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(10);
        #endregion
        // ROTATION
        #region
        _randomizerotation = EditorGUILayout.ToggleLeft("Randomize Rotation", _randomizerotation);
        if (_randomizerotation)
        {
            EditorGUI.indentLevel++;
            _rotationType = (RelationType)EditorGUILayout.EnumPopup(_rotationType);
            _rotMinRange = EditorGUILayout.Vector3Field("Minimum rotation range", _rotMinRange);
            _rotMaxRange = EditorGUILayout.Vector3Field("Maximum rotation range", _rotMaxRange);
            EditorGUI.indentLevel--;
        }
        GUILayout.Space(10);
        #endregion



        // SCALE
        #region

        _randomizeScale = EditorGUILayout.ToggleLeft("Randomize Scale", _randomizeScale);


        if (_randomizeScale)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            _scaleType = (RelationType)EditorGUILayout.EnumPopup(_scaleType);
            _seperateAxisScale = EditorGUILayout.ToggleLeft("Separate Axis", _seperateAxisScale, GUILayout.MaxWidth(150));
            EditorGUILayout.EndHorizontal();
            if (_seperateAxisScale)
            {
                EditorGUI.indentLevel++;
                _scaleMinRange = EditorGUILayout.Vector3Field("Minimum scale range", _scaleMinRange);
                _scaleMaxRange = EditorGUILayout.Vector3Field("Maximum scale range", _scaleMaxRange);
                EditorGUI.indentLevel--;
            }
            else
            {
                _minUniformScale = EditorGUILayout.FloatField("Minimum scale range", _minUniformScale, GUILayout.MaxWidth(250));
                _maxUniformScale = EditorGUILayout.FloatField("Maximum scale range", _maxUniformScale, GUILayout.MaxWidth(250));
                EditorGUI.indentLevel--;

            }

            if (GUILayout.Button("Reset Scale to 1"))
            {
                foreach (var gameObject in _gameObjects)
                {
                    gameObject.transform.localScale = Vector3.one;
                }
            }
        }

        

        GUILayout.Space(10);
        #endregion

        

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply"))
        {
            RecordeUndoForSelectedObjects();
            foreach (var transformElement in _transformElements)
            {
                if (_randomizePosition)
                {
                    Vector3 currentPosition = transformElement.originalPosition;
                    Vector3 offsetPosition = new Vector3(UnityEngine.Random.Range(_posMinRange.x, _posMaxRange.x), UnityEngine.Random.Range(_posMinRange.y, _posMaxRange.y), UnityEngine.Random.Range(_posMinRange.z, _posMaxRange.z));
                    if (_positionType == RelationType.Absolute)
                    {
                        transformElement.TheGameObject.transform.localPosition = offsetPosition;
                    }
                    else if (_positionType == RelationType.Relative)
                    {
                        transformElement.TheGameObject.transform.localPosition = currentPosition + offsetPosition;
                    }

                }
                if (_randomizerotation)
                {
                    Quaternion currentRotation = transformElement.TheGameObject.transform.localRotation;
                    Quaternion offsetRotation = Quaternion.Euler(UnityEngine.Random.Range(_rotMinRange.x, _rotMaxRange.x), UnityEngine.Random.Range(_rotMinRange.y, _rotMaxRange.y), UnityEngine.Random.Range(_rotMinRange.z, _rotMaxRange.z));

                    if (_rotationType == RelationType.Absolute)
                    {
                        transformElement.TheGameObject.transform.localRotation = offsetRotation;
                    }
                    else if (_rotationType == RelationType.Relative)
                    {
                        transformElement.TheGameObject.transform.localRotation = currentRotation * offsetRotation;

                    }

                }

                if (_randomizeScale)
                {
                    Vector3 currentScale = transformElement.TheGameObject.transform.localScale;
                    Vector3 offsetScale;
                    if (_seperateAxisScale)
                    {
                        offsetScale = new Vector3(UnityEngine.Random.Range(_scaleMinRange.x, _scaleMaxRange.x), UnityEngine.Random.Range(_scaleMinRange.y, _scaleMaxRange.y), UnityEngine.Random.Range(_scaleMinRange.z, _scaleMaxRange.z));

                    }
                    else
                    {
                        float randomRangeValue = UnityEngine.Random.Range(_minUniformScale, _maxUniformScale);
                        offsetScale = new Vector3(randomRangeValue, randomRangeValue, randomRangeValue);

                    }

                    switch (_scaleType)
                    {
                        case RelationType.Absolute:
                            transformElement.TheGameObject.transform.localScale = Vector3.Scale(Vector3.one, offsetScale);
                            break;
                        case RelationType.Relative:
                            transformElement.TheGameObject.transform.localScale = Vector3.Scale(transformElement.originalScale, offsetScale);
                            break;
                        default:
                            break;
                    }

                }

            }

            
        }



    }

    /// <summary>
    /// Record the Transform state to allow the undo operation
    /// </summary>
    private static void RecordeUndoForSelectedObjects()
    {
        Transform[] undoableObjects = new Transform[_transformElements.Count];
        for (int i = 0; i < _transformElements.Count; i++)
        {
            undoableObjects[i] = _transformElements[i].TheGameObject.GetComponent<Transform>();
        }
        Undo.RegisterCompleteObjectUndo(undoableObjects, "Transform Randomizer");
        Undo.FlushUndoRecordObjects();
    }

    private void SearchByTag()
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

    private void SearchByName()
    {
        EditorGUILayout.BeginHorizontal();
        _nameSearchField = EditorGUILayout.TextField(_nameSearchField);
        if (GUILayout.Button("Search"))
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
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

    private static void ManualSelection()
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

    private void SetPositionRangePerAxis(float minValue, float maxValue, Axis axis)
    {
        if (axis == Axis.X)
        {
            _posMinRange.Set(minValue, _posMinRange.y, _posMinRange.z);
            _posMaxRange.Set(maxValue, _posMinRange.y, _posMinRange.z);
        }
        if (axis == Axis.Y)
        {
            _posMinRange.Set(_posMinRange.x, minValue, _posMinRange.z);
            _posMaxRange.Set(_posMaxRange.x, maxValue, _posMinRange.z);
        }
        if (axis == Axis.Z)
        {
            _posMinRange.Set(_posMinRange.x, _posMinRange.y, minValue);
            _posMaxRange.Set(_posMaxRange.x, _posMaxRange.y, maxValue);
        }

    }

    private void SetRangesPerAxis(float minValue, float maxValue, Axis axis, ref Vector3 axisMinValues, ref Vector3 axisMaxValues)
    {
        if (axis == Axis.X)
        {
            axisMinValues.Set(minValue, axisMinValues.y, axisMinValues.z);
            axisMaxValues.Set(maxValue, axisMaxValues.y, axisMaxValues.z);
        }
        if (axis == Axis.Y)
        {
            axisMinValues.Set(axisMinValues.x, minValue, axisMinValues.z);
            axisMaxValues.Set(axisMaxValues.x, maxValue, axisMaxValues.z);
        }
        if (axis == Axis.Z)
        {
            axisMinValues.Set(axisMinValues.x, axisMinValues.y, minValue);
            axisMaxValues.Set(axisMaxValues.x, axisMaxValues.y, maxValue);
        }

    }

    private void SetRangeValues(ref float minRange, ref float maxRange, ref float minLimit, ref float maxLimit)
    {
        minRange = (float)EditorGUILayout.DoubleField(System.Math.Round(minRange, 2), GUILayout.MaxWidth(70));
        EditorGUILayout.MinMaxSlider(ref minRange, ref maxRange, minLimit, maxLimit, GUILayout.MaxWidth(200));
        maxRange = (float)EditorGUILayout.DoubleField(System.Math.Round(maxRange, 2), GUILayout.MaxWidth(70));
    }
}
