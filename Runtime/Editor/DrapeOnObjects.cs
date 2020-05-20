// Anthony Ackermans
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Move objects in a world space axis until it collides with an object. 
/// Ideal for draping objects on a surface, like trees on a terrain.
/// </summary>
public class DrapeOnObjects : EditorWindow
{
    // FIELDS
    public enum Axis { X, Y, Z };
    private Axis _projectionAxis = Axis.Y;
    private bool _inverseAxis;
    private List<TransformElement> _transformElementsToMove = new List<TransformElement>();
    private List<TransformElement> _transformElementsToProjectOn = new List<TransformElement>();
    public ObjectGetter ObjectGetterToMove = new ObjectGetter();
    public ObjectGetter ObjectGetterToProjectOn = new ObjectGetter();
    private Vector3 _raycastDirection;
    private string _logBox;
    int objectsHit = 0;
    int objectsMissed = 0;

    // Add menu item 
    [MenuItem("Tools/Drape on objects")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DrapeOnObjects));
    }

    private void AlignObjectToNormal(Axis projectionAxis)
    {

        switch (projectionAxis)
        {
            case Axis.X:
                _raycastDirection = _inverseAxis == true ? Vector3.left : Vector3.right;
                break;
            case Axis.Y:
                _raycastDirection = _inverseAxis == true ? Vector3.down : Vector3.up;
                break;
            case Axis.Z:
                _raycastDirection = _inverseAxis == true ? Vector3.back : Vector3.forward;
                break;
            default:
                break;
        }

        foreach (var transfomElementtoMove in _transformElementsToMove)
        {
            RaycastHit[] hits;

            hits = Physics.RaycastAll(transfomElementtoMove.TheGameObject.transform.position, _raycastDirection, 100.0F);
            foreach (var item in hits)
            {
                Debug.Log(item);
            }

            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    foreach (var transformElementtoProjectTo in _transformElementsToProjectOn)
                    {
                        if (transformElementtoProjectTo.TheGameObject.transform == hit.transform)
                        {
                            transfomElementtoMove.TheGameObject.transform.position = hit.point;
                            objectsHit++;
                            break;
                        }
                    }
                }      
            }
            else
            {
                objectsMissed++;
            }
            
            
        }
        _logBox = $"{objectsHit} objects draped \n{objectsMissed} objects missed";
    }

    private void OnGUI()
    {
        _transformElementsToMove = ObjectGetterToMove.ObjectSelectionShowUi("Search for transforms");
        EditorGUILayout.Space(10);
        _transformElementsToProjectOn = ObjectGetterToProjectOn.ObjectSelectionShowUi("Project on transforms");
        EditorGUILayout.Space(10);
        _projectionAxis = (Axis)EditorGUILayout.EnumPopup("Projection axis", _projectionAxis);
        _inverseAxis = EditorGUILayout.Toggle("Inverse axis", _inverseAxis);

        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(_logBox, EditorStyles.helpBox, GUILayout.Height(30));
        if (GUILayout.Button("Reset Values"))
        {
            ResetValues();
        }
        if (GUILayout.Button("Apply"))
        {
            ObjectGetter.RecordeUndoForSelectedObjects(_transformElementsToMove, "Project objects");
            AlignObjectToNormal(_projectionAxis);
        }
    }

    private void ResetValues()
    {
        objectsHit = 0;
        objectsMissed = 0;
        _logBox = "";
        ObjectGetterToMove.ClearList();
        ObjectGetterToProjectOn.ClearList();
        
    }
}
