using UnityEngine;

public class TransformElement
{

    public GameObject TheGameObject;
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public Vector3 originalScale;

    public TransformElement(GameObject go, Transform tr)
    {
        TheGameObject = go;
        originalPosition = tr.localPosition;
        originalRotation = tr.localRotation;
        originalScale = tr.localScale;
    }


}
