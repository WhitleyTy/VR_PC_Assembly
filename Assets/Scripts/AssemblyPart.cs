using UnityEngine;

public class AssemblyPart : MonoBehaviour
{
    public int stepIndex;
    public Transform snapTarget;
    public Renderer[] visualRenderers;
    [HideInInspector] public bool installed;
    [HideInInspector] public bool held;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Quaternion initialRotation;

    public void CacheStart()
    {
        originalParent = transform.parent;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
}
