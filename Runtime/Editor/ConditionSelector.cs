// Anthony Ackermans
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{

    /// <summary>

    /// </summary>
    public class ConditionSelector : EditorWindow
    {
        // FIELDS
        public List<GameObject> SelectedGameObjects;
        private List<ICondition> _conditions;
        private bool _inverse;
        private bool _isolate;
        private List<FoldoutHeaderToggle> allFoldoutHeadertoggles = new List<FoldoutHeaderToggle>();
        private string _logBox;

        // Add menu item 
        [MenuItem("Tools/Selection Filter")]
        public static void ShowWindow()
        {
            ConditionSelector window = (ConditionSelector)GetWindow(typeof(ConditionSelector));
            window.title = "Selection Filter";
            window.Show();
        }

        private void OnEnable()
        {
            SelectedGameObjects = new List<GameObject>();
            _conditions = new List<ICondition>();

            FoldoutHeaderToggle _foldoutTriangleCount = new FoldoutHeaderToggle("Triangle count", new TriangleCount());
            FoldoutHeaderToggle _foldoutTag = new FoldoutHeaderToggle("Contains tag", new HasTag());
            FoldoutHeaderToggle _foldoutLayer = new FoldoutHeaderToggle("Contains layer", new HasLayer());
            FoldoutHeaderToggle _foldoutStaticActive = new FoldoutHeaderToggle("Gameobject property", new StaticOrActive());
            FoldoutHeaderToggle _foldoutContainsMaterial = new FoldoutHeaderToggle("Contains Material", new ContainsMaterial());

            allFoldoutHeadertoggles.Add(_foldoutTriangleCount);
            allFoldoutHeadertoggles.Add(_foldoutTag);
            allFoldoutHeadertoggles.Add(_foldoutLayer);
            allFoldoutHeadertoggles.Add(_foldoutContainsMaterial);
            allFoldoutHeadertoggles.Add(_foldoutStaticActive);

        }

        private void OnGUI()
        {
            foreach (var foldout in allFoldoutHeadertoggles)
            {
                foldout.ShowHeader();
            }

            GUILayout.FlexibleSpace();
            
            UtilitiesToolExtensions.DrawSplitter();

            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            _inverse = EditorGUILayout.Toggle("Invert selection", _inverse);
            _isolate = EditorGUILayout.Toggle("Isolate selection", _isolate);
            EditorGUILayout.LabelField(_logBox, EditorStyles.helpBox, GUILayout.Height(30));
            if (GUILayout.Button("Reset Values"))
            {
                ResetValues();
            }
            if (GUILayout.Button("Select objects"))
            {
                SelectObjects();
            }
        }

        private void SelectObjects()
        {
            List<FoldoutHeaderToggle> activeFoldouts = new List<FoldoutHeaderToggle>();
            foreach (var foldout in allFoldoutHeadertoggles)
            {
                if (foldout.groupEnabled)
                {
                    activeFoldouts.Add(foldout);
                }
            }

            if (activeFoldouts.Count == 0)
            {
                return;
            }

            var filteredGameObjectsPerCondition = activeFoldouts[0].theCondition.Select();

            for (int i = 1; i < activeFoldouts.Count; i++)
            {
                filteredGameObjectsPerCondition = filteredGameObjectsPerCondition.Intersect(activeFoldouts[i].theCondition.Select()).ToList();
            }

            if (_inverse)
            {
                List<GameObject> tempList = new List<GameObject>();
                tempList = InvertSelection(filteredGameObjectsPerCondition);
                filteredGameObjectsPerCondition.Clear();
                filteredGameObjectsPerCondition = tempList;
            }

            if (_isolate)
            {
                var sv = SceneVisibilityManager.instance;
                sv.Isolate(filteredGameObjectsPerCondition.ToArray(), false);
            }

            Selection.objects = filteredGameObjectsPerCondition.ToArray();
            _logBox = $"Selected {filteredGameObjectsPerCondition.Count} objects";
        }

        private List<GameObject> InvertSelection(List<GameObject> selectedGameObjects)
        {
            List<GameObject> invertedSelection = new List<GameObject>();
            foreach (GameObject obj in UtilitiesToolExtensions.GetAllGameObjects())
            {
                if (!selectedGameObjects.Contains(obj))
                    invertedSelection.Add(obj);
            }
            return invertedSelection;
        }

        private void ResetValues()
        {
            Selection.objects = null;
            _inverse = false;
            _isolate = false;
            allFoldoutHeadertoggles.Clear();
            _logBox = "";
            OnEnable();
        }
    }
}