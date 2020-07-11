// Anthony Ackermans
// The array creator creates an array of object in &, 2 or 3 dimensions. The workflow is similar to 3ds max's array tool.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{

    /// <summary>
    /// Find and rename objects in the scene
    /// </summary>
    public class ArrayCreator : EditorWindow
    {
        // FIELDS
        private GameObject _objectToArray;
        private Vector3 _moveTransformation;
        private Vector3 _rotationTransformation;
        private Vector3 _ScaleTransformation;
        private bool _uniformScale;
        private string[] _dimension = new string[] { "1D", "2D", "3D" };
        private int _dimensionIndex;
        private string[] _incrementalTotal = new string[] { "Incremental", "Total" };
        private int _incrementalTotalMoveIndex;
        private int _incrementalTotalrotateIndex;
        private int _incrementalTotalScaleIndex;
        private Material _previewMaterial;
        private List<GameObject> _allPreviewObjects = new List<GameObject>();
        private PrefabAssetType assetType;
        bool _isInstanciated = false;
        int _instancesAmount = 0;

        private int _oneDimensionCount;

        public int OneDimensionCount
        {
            get { return _oneDimensionCount; }
            set
            {
                _oneDimensionCount = Mathf.Max(1, value);
            }
        }


        private Vector2Int _twoDimensionCount;
        public Vector2Int TwoDimensionCount
        {
            get { return _twoDimensionCount; }
            set
            {
                Vector2Int clampedValue = new Vector2Int(Mathf.Max(1, value.x), Mathf.Max(1, value.y));
                _twoDimensionCount = clampedValue;
            }
        }


        private Vector3 _incrementalRowOffset2D;
        private Vector3 _incrementalRowOffset3D;

        private Vector3Int _threeDimensionCount;
        public Vector3Int ThreeDimensionCount
        {
            get { return _threeDimensionCount; }
            set
            {
                Vector3Int clampedValue = new Vector3Int(Mathf.Max(1, value.x), Mathf.Max(1, value.y), Mathf.Max(1, value.z));
                _threeDimensionCount = clampedValue;
            }
        }


        private bool _previewVisible;
        private bool _isShowingPreview;
        private float _uniformScaleValue;
        private Transform _instantiateAsChildren;
        private bool _keepAsPrefab = true;
        private bool _avoidChildren;

        // Add menu item 
        [MenuItem("Tools/Array Creator")]
        public static void ShowWindow()
        {
            ArrayCreator window = (ArrayCreator)GetWindow(typeof(ArrayCreator));
            window.title = "Array creator";
            window.Show();
        }

        private void OnEnable()
        {
            _previewMaterial = new Material(Shader.Find("Unlit/Color"));
            _previewMaterial.color = Color.yellow;
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Array Object", EditorStyles.boldLabel);
            _objectToArray = (GameObject)EditorGUILayout.ObjectField(_objectToArray, typeof(GameObject), true);
            if (GUILayout.Button("Add selected"))
            {
                _objectToArray = (GameObject)Selection.activeObject;
            }
            if (_objectToArray != null)
            {
                assetType = PrefabUtility.GetPrefabAssetType(_objectToArray);
            }



            EditorGUILayout.EndVertical();
            // --------- ARRAY TRANSFORMATION --------- //
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Array Transformation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            _moveTransformation = EditorGUILayout.Vector3Field("Move", _moveTransformation);
            _incrementalTotalMoveIndex = EditorGUILayout.Popup(_incrementalTotalMoveIndex, _incrementalTotal);
            GUILayout.Space(10);
            _rotationTransformation = EditorGUILayout.Vector3Field("Rotation", _rotationTransformation);
            _incrementalTotalrotateIndex = EditorGUILayout.Popup(_incrementalTotalrotateIndex, _incrementalTotal);
            GUILayout.Space(10);
            if (_uniformScale)
            {
                _uniformScaleValue = EditorGUILayout.FloatField("Scale:", _uniformScaleValue);
                _ScaleTransformation = new Vector3(_uniformScaleValue, _uniformScaleValue, _uniformScaleValue);
            }
            else
            {
                _ScaleTransformation = EditorGUILayout.Vector3Field("Scale:", _ScaleTransformation);

            }
            _uniformScale = EditorGUILayout.ToggleLeft("Uniform", _uniformScale);
            _incrementalTotalScaleIndex = EditorGUILayout.Popup(_incrementalTotalScaleIndex, _incrementalTotal);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            // --------- ARRAY DIMENSION --------- //
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Array Dimension", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _dimensionIndex = EditorGUILayout.Popup(_dimensionIndex, _dimension);

            switch (_dimensionIndex)
            {
                case 0:
                    OneDimensionCount = EditorGUILayout.IntField("1D Count:", OneDimensionCount);
                    break;
                case 1:
                    TwoDimensionCount = EditorGUILayout.Vector2IntField("2D Count:", TwoDimensionCount);
                    _incrementalRowOffset2D = EditorGUILayout.Vector3Field("Incremental Row Offset", _incrementalRowOffset2D);
                    break;
                case 2:
                    ThreeDimensionCount = EditorGUILayout.Vector3IntField("3D Count:", ThreeDimensionCount);
                    _incrementalRowOffset3D = EditorGUILayout.Vector3Field("Incremental Row Offset", _incrementalRowOffset3D);
                    break;
                default:
                    break;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            // ----------------------------------- //

            // -------------- OPTIONS ------------- //
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _avoidChildren = EditorGUILayout.Toggle(new GUIContent("Don't include children", "Avoid all children from the object to be instantiated. For prefabs, this function will be ignored as it is not possible to destroy prefab's children"), _avoidChildren);
            _instantiateAsChildren = (Transform)EditorGUILayout.ObjectField(new GUIContent("Instantiate as child of", "Every instance will become a child of the selected object"), _instantiateAsChildren, typeof(Transform), true);
            _keepAsPrefab = EditorGUILayout.Toggle(new GUIContent("Keep as prefab", "If the object to be instanciated is a prefab, keep every copy as an instance of that prefab"), _keepAsPrefab);
            EditorGUILayout.EndVertical();
            // ----------------------------------- //

            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField($"Total instances in array: {_instancesAmount}");

            EditorGUILayout.EndVertical();



            GUILayout.FlexibleSpace();
            UtilitiesToolExtensions.DrawSplitter();
            _previewVisible = GUILayout.Toggle(_previewVisible, "Preview", "Button");
            if (_previewVisible && GUI.changed)
            {
                PreviewArray();
                _isShowingPreview = true;
            }
            else if (!_previewVisible && _isShowingPreview)
            {
                RemovePreviewArray();
                _isShowingPreview = false;
            }


            if (GUILayout.Button("Reset Values"))
            {
                ResetValues();
            }
            if (GUILayout.Button("Create array"))
            {
                CreateArray();
                _previewVisible = false;
            }
        }

        private void ResetValues()
        {
            _objectToArray = null;
            _instantiateAsChildren = null;
            _moveTransformation = Vector3.zero;
            _rotationTransformation = Vector3.zero;
            _ScaleTransformation = Vector3.zero;
            _uniformScale = false;
            _dimensionIndex = 0;
            _incrementalTotalMoveIndex = 0;
            _incrementalTotalrotateIndex = 0;
            _incrementalTotalScaleIndex = 0;
            _allPreviewObjects.Clear();
            _isInstanciated = false;
            _instancesAmount = 0;
            OneDimensionCount = 1;
            TwoDimensionCount = Vector2Int.one;
            ThreeDimensionCount = Vector3Int.one;
            _keepAsPrefab = true;
            _incrementalRowOffset2D = Vector3.zero;
            _incrementalRowOffset3D = Vector3.zero;
        }

        private void CreateArray()
        {
            switch (_dimensionIndex)
            {
                case 0:
                    OneDeeArray(false);
                    break;
                case 1:
                    TwoDeeArray(false);
                    break;
                case 2:
                    ThreeDeeArray(false);
                    break;
                default:
                    break;
            }
        }


        private void PreviewArray()
        {
            if (_objectToArray != null)
            {
                if (_dimensionIndex == 0) // 1D array
                {
                    OneDeeArray(true);
                }

                if (_dimensionIndex == 1) // 2D array
                {
                    TwoDeeArray(true);
                }

                if (_dimensionIndex == 2) // 3D array
                {
                    ThreeDeeArray(true);
                }
            }


        }

        private void RemovePreviewArray()
        {
            foreach (var go in _allPreviewObjects.ToArray())
            {
                DestroyImmediate(go);
            }
            _allPreviewObjects.Clear();
            _isInstanciated = false;
            _instancesAmount = 0;
        }

        private void OnDestroy()
        {
            RemovePreviewArray();
        }

        private void ObjectPreviewProperties(GameObject instantiatedObject)
        {
            instantiatedObject.hideFlags = HideFlags.HideInHierarchy;
            var allMeshRenderers = instantiatedObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in allMeshRenderers)
            {
                mr.material = _previewMaterial;
            }
            _allPreviewObjects.Add(instantiatedObject);
        }

        private GameObject ObjectInstantiate()
        {
            GameObject instantiatedObject;
            if (assetType == PrefabAssetType.NotAPrefab || !_keepAsPrefab)
            {
                instantiatedObject = _instantiateAsChildren ? Instantiate(_objectToArray, _instantiateAsChildren) : Instantiate(_objectToArray);
            }
            else
            {
                var _objectToArrayPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(_objectToArray);
                GameObject _objecttoArrayPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(_objectToArrayPrefabPath, typeof(GameObject));
                instantiatedObject = _instantiateAsChildren ? (GameObject)PrefabUtility.InstantiatePrefab(_objecttoArrayPrefab, _instantiateAsChildren) : (GameObject)PrefabUtility.InstantiatePrefab(_objecttoArrayPrefab);
            }

            if (_avoidChildren && assetType == PrefabAssetType.NotAPrefab)
            {
                foreach (Transform child in instantiatedObject.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            return instantiatedObject;
        }

        private void GenerateRow(float count, Vector3 rowOffset, bool isPreview, bool ignoreFirstInstance)
        {
            for (int i = !ignoreFirstInstance || _instantiateAsChildren != null ? 0 : 1; i < count; i++)
            {
                GameObject instantiatedObject = ObjectInstantiate();
                instantiatedObject.name = _objectToArray.name + "_" + string.Format("{0:00}", i);
                if (_instantiateAsChildren == null)
                {
                    instantiatedObject.transform.position = _incrementalTotalMoveIndex == 0 ? _objectToArray.transform.position + rowOffset + _moveTransformation * i : _objectToArray.transform.position + rowOffset + (_moveTransformation / _oneDimensionCount) * i;
                }
                else
                {

                    instantiatedObject.transform.position = _incrementalTotalMoveIndex == 0 ? (_instantiateAsChildren.position + rowOffset) + _moveTransformation * i : (_instantiateAsChildren.position + rowOffset) + (_moveTransformation / _oneDimensionCount) * i;
                }
                instantiatedObject.transform.localRotation = _incrementalTotalrotateIndex == 0 ? Quaternion.Euler(_rotationTransformation * i) : Quaternion.Euler((_rotationTransformation / _oneDimensionCount) * i);
                instantiatedObject.transform.localScale += _incrementalTotalScaleIndex == 0 ? _ScaleTransformation * i : (_ScaleTransformation / _oneDimensionCount) * i;

                if (isPreview)
                {
                    ObjectPreviewProperties(instantiatedObject);

                }
                else
                {
                    Undo.RegisterCreatedObjectUndo(instantiatedObject, "Create Array");
                }
            }
        }

        private void OneDeeArray(bool isPreview)
        {

            if (_isInstanciated)
            {
                RemovePreviewArray();
            }
            GenerateRow(OneDimensionCount, Vector3.zero, isPreview, true);

            _isInstanciated = true;
            _instancesAmount = _oneDimensionCount;
        }


        private void TwoDeeArray(bool isPreview)
        {
            if (_isInstanciated)
            {
                RemovePreviewArray();
            }

            for (int i = 0; i < TwoDimensionCount.y; i++)
            {
                GenerateRow(TwoDimensionCount.x, _incrementalRowOffset2D * i, isPreview, i == 0 ? true : false);
            }


            _isInstanciated = true;
            _instancesAmount = _twoDimensionCount.x * _twoDimensionCount.y;
        }

        private void ThreeDeeArray(bool isPreview)
        {
            if (_isInstanciated)
            {
                RemovePreviewArray();
            }

            for (int i = 0; i < ThreeDimensionCount.z; i++)
            {
                for (int j = 0; j < ThreeDimensionCount.y; j++)
                {
                    Vector3 newOffset = new Vector3(_incrementalRowOffset3D.x, _incrementalRowOffset3D.y * j, _incrementalRowOffset3D.z * i);
                    GenerateRow(ThreeDimensionCount.x, newOffset, isPreview, i == 0 && j == 0 ? true : false);
                }
            }


            _isInstanciated = true;
            _instancesAmount = _threeDimensionCount.x * _threeDimensionCount.y + _threeDimensionCount.z;
        }
    }
}