// Anthony Ackermans
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class StaticOrActive : Object, ICondition
    {
        public delegate void OnRemoveDelegate(ICondition iCondition);
        public event OnRemoveDelegate OnRemove;

        //private int _objecttypesIndex;
        private bool _isStatic;
        private bool _isActive;
        private bool _isPrefab;
        //private string[] _objectTypes = new string[] { "All", "3D Mesh", "Light", "Terrain", "Camera", "AudioSource", "Particle System" };
        public List<GameObject> Select()
        {
            List<GameObject> filteredGameObjects = new List<GameObject>();
            GameObject[] allGameObjects = UtilitiesToolExtensions.GetAllGameObjects();


            foreach (var go in allGameObjects)
            {
                if (_isPrefab)
                {
                    PrefabAssetType assetType = PrefabUtility.GetPrefabAssetType(go);
                    if (assetType == PrefabAssetType.Regular)
                    {
                        filteredGameObjects.Add(go);
                        continue;
                    }
                }

                if (_isStatic)
                {
                    if (go.isStatic == true)
                    {
                        filteredGameObjects.Add(go);
                        continue;
                    }

                }

                if (_isActive)
                {
                    if (go.activeInHierarchy == true)
                    {
                        filteredGameObjects.Add(go);
                        
                        continue;
                    }

                }

            }
            return filteredGameObjects;
        }

        public void ShowUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUI.indentLevel++;
            //_objecttypesIndex = EditorGUILayout.Popup("ObjectType", _objecttypesIndex, _objectTypes);
            _isStatic = EditorGUILayout.Toggle("Is static", _isStatic);
            _isActive = EditorGUILayout.Toggle("Is active", _isActive);
            _isPrefab = EditorGUILayout.Toggle("Is a regular prefab", _isPrefab);
            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}