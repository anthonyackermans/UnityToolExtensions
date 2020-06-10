// Anthony Ackermans
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class HasTag : Object, ICondition
    {
        public delegate void OnRemoveDelegate(ICondition iCondition);
        public event OnRemoveDelegate OnRemove;

        private string _TagSearchField = "Untagged";

        public List<GameObject> Select()
        {
            List<GameObject> gameObjectByTag = new List<GameObject>();

            if (_TagSearchField == "Untagged") // FindGameObjectsWithTag seems to not work well with untagged gameobjects
            {
                var allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (var go in allGameObjects)
                {
                    if (go.tag == "Untagged")
                    {
                        gameObjectByTag.Add(go);
                    }
                }
            }
            else
            {
                gameObjectByTag = GameObject.FindGameObjectsWithTag(_TagSearchField).ToList<GameObject>();
            }

            return gameObjectByTag;
            }

            public void ShowUI()
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUI.indentLevel++;
                _TagSearchField = EditorGUILayout.TagField("Select tag", _TagSearchField);
                EditorGUI.indentLevel--;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }
    }