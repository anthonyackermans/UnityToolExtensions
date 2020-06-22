// Anthony Ackermans

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ToolExtensions
{

    /// <summary>
    /// Find and rename objects in the scene
    /// </summary>
    public class Renamer : EditorWindow
    {
        // FIELDS
        private List<TransformElement> _transformelements = new List<TransformElement>();
        private ObjectGetter _objectGetter = new ObjectGetter();
        private bool _baseName;
        private string _baseNameString;
        private bool _prefix;
        private bool _suffix;
        private string _suffixString;
        private string _prefixString;
        private bool _removefirstdigits;
        private bool _numbered;
        private int _baseNumber = 1;
        private int _step = 1;
        private int _removefirstdigitsAmount;
        private bool _removeLastdigits;
        private int _removeLastdigitsAmount;

        // Add menu item 
        [MenuItem("Tools/Renamer")]
        public static void ShowWindow()
        {
            GetWindow(typeof(Renamer));
        }


        private void OnGUI()
        {
            _transformelements = _objectGetter.ObjectSelectionShowUi("Search for objects");
            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(_baseName);
            EditorGUILayout.BeginHorizontal();
            _removefirstdigits = EditorGUILayout.BeginToggleGroup("Remove first digits", _removefirstdigits);
            _removefirstdigitsAmount = EditorGUILayout.IntField(_removefirstdigitsAmount);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _removeLastdigits = EditorGUILayout.BeginToggleGroup("Remove last digits", _removeLastdigits);
            _removeLastdigitsAmount = EditorGUILayout.IntField(_removeLastdigitsAmount);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_removefirstdigits || _removeLastdigits);
            EditorGUILayout.BeginHorizontal();
            _baseName = EditorGUILayout.BeginToggleGroup("Base Name", _baseName);
            _baseNameString = EditorGUILayout.TextField(_baseNameString);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            _prefix = EditorGUILayout.BeginToggleGroup("Prefix", _prefix);
            _prefixString = EditorGUILayout.TextField(_prefixString);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _suffix = EditorGUILayout.BeginToggleGroup("Suffix", _suffix);
            _suffixString = EditorGUILayout.TextField(_suffixString);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndHorizontal();

            _numbered = EditorGUILayout.BeginToggleGroup("Numbered", _numbered);
            EditorGUI.indentLevel++;
            _baseNumber = EditorGUILayout.IntField("Base Number", _baseNumber);
            _step = EditorGUILayout.IntField("Step", _step);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset Values"))
            {
                ResetValues();
            }
            if (GUILayout.Button("Apply"))
            {
                RenameObjects();
            }
        }

        private void ResetValues()
        {
            _baseName = false;
            _baseNameString = "";
            _prefix = false;
            _prefixString = "";
            _suffix = false;
            _suffixString = "";
            _removefirstdigits = false;
            _removefirstdigitsAmount = 0;
            _removeLastdigits = false;
            _removeLastdigitsAmount = 0;
            _numbered = false;
            _baseNumber = 1;
            _step = 1;

        }

        private void RenameObjects()
        {

            int indexNumber = 0;
            foreach (var te in _transformelements)
            {

                string newName = _removefirstdigits || _removeLastdigits ? "" : te.TheGameObject.name;

                if (_baseName)
                {
                    newName = _baseNameString;
                }

                if (_prefix)
                {
                    newName = string.Concat(_prefixString, newName);
                }

                string newName2 = "";
                if (_removefirstdigits)
                {
                    newName2 = te.TheGameObject.name.Substring(_removefirstdigitsAmount);
                }

                if (_removeLastdigits)
                {
                    newName2 = te.TheGameObject.name.Remove(te.TheGameObject.name.Length - _removeLastdigitsAmount);
                }

                if (_suffix)
                {
                    newName2 = string.Concat(newName2, _suffixString);
                }

                if (_numbered)
                {
                    string format = _transformelements.Count <= 1000 ? "{0:00}" : "{0:000}"; // Format with leading zeros depending on the amount of selected objects

                    newName2 = string.Concat(newName2, "_", string.Format(format, _baseNumber + indexNumber));

                    indexNumber += _step;
                }

                te.TheGameObject.name = string.Concat(newName, newName2);

            }
        }
    }
}