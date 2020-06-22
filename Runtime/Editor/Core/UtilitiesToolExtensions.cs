// Anthony Ackermans
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class UtilitiesToolExtensions : MonoBehaviour
    {
        // FIELDS
        bool unfolded = false;
        bool GroupEnabled = false;



        /// <summary>
        /// Draws a horizontal split line.
        /// </summary>
        public static void DrawSplitter()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f);

            // Splitter rect should be full-width
            rect.xMin = 0f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, ToolExtensionsStyles.splitter);
        }

        /// <summary>
        /// Draws a header label.
        /// </summary>
        /// <param name="title">The label to display as a header</param>
        public static void DrawHeaderLabel(string title)
        {
            EditorGUILayout.LabelField(title, ToolExtensionsStyles.headerLabel);
        }

        /// <summary>
        /// Return an array of all the objects, active and inactive, in the scene.
        /// Hidden scene objects are ignored with the hideflags.
        /// </summary>
        /// <returns></returns>
        public static GameObject[] GetAllGameObjects()
        {
            GameObject[] allUnfilteredObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            List<GameObject> allfilteredObjectsList = new List<GameObject>();

            foreach (var go in allUnfilteredObjects)
            {
                if (go.hideFlags == HideFlags.None && !EditorUtility.IsPersistent(go.transform.root.gameObject)) // Make sure to not add prefabs from the project view and only count those in the scene
                {
                    allfilteredObjectsList.Add(go);
                }
                
            }
            return allfilteredObjectsList.ToArray();
        }
    }
}