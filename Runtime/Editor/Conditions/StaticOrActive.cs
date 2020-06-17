// Anthony Ackermans
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class StaticOrActive : UnityEngine.Object, ICondition
    {
        public delegate void OnRemoveDelegate(ICondition iCondition);
        public event OnRemoveDelegate OnRemove;

        private int _objecttypesIndex;
        private bool _isStatic;
        private bool _isActive;
        private bool _isPrefab;
        private string[] _objectTypes = new string[] { "All", "3D Mesh", "Light", "Terrain", "Camera", "AudioSource", "Particle System", "Empty", "Empty and no children", "Reflection Probe", "Windzone" };
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

                    switch (_objecttypesIndex)
                    {
                        case 0: 
                        break;
                    case 1:
                        CheckTypeAndAddToList<MeshRenderer>(go, filteredGameObjects);
                        break;
                    case 2:
                        CheckTypeAndAddToList<Light>(go, filteredGameObjects);
                        break;
                    case 3:
                        CheckTypeAndAddToList<Terrain>(go, filteredGameObjects);
                        break;
                    case 4:
                        CheckTypeAndAddToList<Camera>(go, filteredGameObjects);
                        break;
                    case 5:
                        CheckTypeAndAddToList<AudioSource>(go, filteredGameObjects);
                        break;
                    case 6:
                        CheckTypeAndAddToList<ParticleSystem>(go, filteredGameObjects);                        
                        break;
                    case 7:
                        if (CheckEmptyGameObject(go))
                        {
                            filteredGameObjects.Add(go);
                        }
                        break;
                    case 8:
                        if (CheckEmptyGameObject(go) && go.transform.childCount == 0)
                        {
                            filteredGameObjects.Add(go);
                        }
                        break;
                    case 9:
                        CheckTypeAndAddToList<ReflectionProbe>(go, filteredGameObjects);
                        break;
                    case 10:
                        CheckTypeAndAddToList<WindZone>(go, filteredGameObjects);
                        break;
                    default:
                            break;
                    }
                

            }
            return filteredGameObjects;
        }

        private bool CheckEmptyGameObject(GameObject go)
        {
            var components = go.GetComponents<Component>();
            if (components.Length <= 1)
            {
                return true;
            }
            else return false;
        }

        private void CheckTypeAndAddToList<T> (GameObject go, List<GameObject> theList)
        {
            if (go.GetComponent(typeof(T)) != null)
            {
                theList.Add(go);
            }
        }



        public void ShowUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUI.indentLevel++;
            _objecttypesIndex = EditorGUILayout.Popup("ObjectType", _objecttypesIndex, _objectTypes);
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