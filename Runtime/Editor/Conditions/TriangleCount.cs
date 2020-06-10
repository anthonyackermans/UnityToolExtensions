
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    [System.Serializable]
    public class TriangleCount : UnityEngine.Object, ICondition
    {
        public int MinTriangles;
        public int MaxTriangles;
        bool expanded = false;

        public delegate void OnRemoveDelegate(ICondition iCondition);
        public event OnRemoveDelegate OnRemove;
        private string[] _options = new string[] { "Less than", "Greater than", "Between values" };
        private int _indexOptions;
        private int _GreaterThanValue;
        private int _lessThanValue;

        public TriangleCount(int min, int max)
        {
            MinTriangles = min;
            MaxTriangles = max;
        }

        public TriangleCount()
        {
            MinTriangles = 0;
            MaxTriangles = 0;
        }

        public List<GameObject> Select()
        {
            List<GameObject> gameObjectByFaces = new List<GameObject>();
            MeshFilter[] allMeshfilters = UnityEngine.Object.FindObjectsOfType<MeshFilter>();


            foreach (var meshfilter in allMeshfilters)
            {
                int facesAmount = meshfilter.sharedMesh.triangles.Length / 3;

                if (_indexOptions == 0) // Less than
                {
                    if (facesAmount <= _lessThanValue)
                    {
                        gameObjectByFaces.Add(meshfilter.gameObject);
                    }
                }

                if (_indexOptions == 1) // greater than
                {
                    if (facesAmount >= _GreaterThanValue)
                    {
                        gameObjectByFaces.Add(meshfilter.gameObject);
                    }
                }

                if (_indexOptions == 2) // Between values
                {
                    if (facesAmount >= MinTriangles && facesAmount <= MaxTriangles)
                    {
                        gameObjectByFaces.Add(meshfilter.gameObject);
                    }
                }

            }
            return gameObjectByFaces;
        }

        public void ShowUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            _indexOptions = EditorGUILayout.Popup("Triangle amount",_indexOptions, _options);

            EditorGUI.indentLevel++;
            switch (_indexOptions)
            {
                case 0:
                    LowerThan();
                    break;
                case 1:
                    GreaterThan();
                    break;
                case 2:
                    BewteenValues();
                    break;
                default:
                    break;
            }
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

        }

        private void BewteenValues()
        {
            MinTriangles = EditorGUILayout.IntField("Min triangles", MinTriangles);
            MaxTriangles = EditorGUILayout.IntField("Max triangles", MaxTriangles);
        }

        private void GreaterThan()
        {
            _GreaterThanValue = EditorGUILayout.IntField("Triangle amount", _GreaterThanValue);
        }

        private void LowerThan()
        {
            _lessThanValue = EditorGUILayout.IntField("Triangle amount", _lessThanValue);
        }
    }
}