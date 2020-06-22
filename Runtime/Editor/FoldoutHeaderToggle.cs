using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{
    public class FoldoutHeaderToggle
    {
        private bool _unfolded;
        public bool groupEnabled;
        private string _headerTitle;
        public ICondition theCondition;

        public FoldoutHeaderToggle(string headerTitle, ICondition condition)
        {
            _headerTitle = headerTitle;
            theCondition = condition;
        }

        public void ShowHeader()
        {
            EditorGUILayout.BeginHorizontal("box");
            // foldout
            GUIStyle mystyle = EditorStyles.foldout;
            mystyle.fixedWidth = 20f;
            _unfolded = EditorGUILayout.Toggle(_unfolded, mystyle, GUILayout.MaxWidth(17f));

            // Active checkbox
            groupEnabled = EditorGUILayout.ToggleLeft(_headerTitle, groupEnabled, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            if (_unfolded)
            {

                EditorGUI.BeginDisabledGroup(!groupEnabled);
                theCondition.ShowUI();
                EditorGUI.EndDisabledGroup();
            }
        }


    }
}