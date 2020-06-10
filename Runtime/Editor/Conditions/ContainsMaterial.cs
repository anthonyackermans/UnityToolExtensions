// Anthony Ackermans
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class ContainsMaterial : Object, ICondition
    {
        public delegate void OnRemoveDelegate(ICondition iCondition);
        public event OnRemoveDelegate OnRemove;
        private Material _material;
        private bool expanded;


        public List<GameObject> Select()
        {
            List<GameObject> gameObjectsWithMaterial = new List<GameObject>();
            List<MeshRenderer> allMeshRenderers = GameObject.FindObjectsOfType<MeshRenderer>().ToList<MeshRenderer>();

            foreach (var mr in allMeshRenderers)
            {
                if (mr.sharedMaterial == _material)
                {
                    gameObjectsWithMaterial.Add(mr.gameObject);
                }
            }

            return gameObjectsWithMaterial;
        }

        public void ShowUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

                EditorGUI.indentLevel++;
                _material = (Material)EditorGUILayout.ObjectField("Material", _material, typeof(Material), true);
                EditorGUI.indentLevel--;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
    }
}