// Anthony Ackermans
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class HasLayer : Object, ICondition
    {
        public delegate void OnRemoveDelegate(ICondition iCondition);
        public event OnRemoveDelegate OnRemove;

        private bool expanded;
        private int _layerSearchField = 0;

        public List<GameObject> Select()
        {
            List<GameObject> gameObjectByLayer = new List<GameObject>();
            List<GameObject> allGameObjects = GameObject.FindObjectsOfType<GameObject>().ToList<GameObject>();

            foreach (var go in allGameObjects)
            {
                if (go.layer == _layerSearchField)
                {
                    gameObjectByLayer.Add(go);
                }
            }

            return gameObjectByLayer;
        }

        public void ShowUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUI.indentLevel++;
                _layerSearchField = EditorGUILayout.LayerField("Select Layer", _layerSearchField);
                EditorGUI.indentLevel--;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }


    }
}