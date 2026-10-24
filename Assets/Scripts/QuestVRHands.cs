using UnityEngine;
using UnityEngine.XR;

public class QuestVRHands : MonoBehaviour
{
    public Camera headCamera;
    public Transform leftHand;
    public Transform rightHand;
    public AssemblyManager manager;
    public float grabRadius = 0.22f;
    public float holdDistance = 0.10f;

    AssemblyPart leftHeld, rightHeld;
    bool leftGripPrevious, rightGripPrevious, restartPrevious;

    void Update()
    {
        UpdatePose(XRNode.Head, headCamera.transform);
        UpdatePose(XRNode.LeftHand, leftHand);
        UpdatePose(XRNode.RightHand, rightHand);
        HandleHand(XRNode.LeftHand, leftHand, ref leftHeld, ref leftGripPrevious);
        HandleHand(XRNode.RightHand, rightHand, ref rightHeld, ref rightGripPrevious);

        var rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool restart = rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool button) && button;
        if (restart && !restartPrevious && manager.currentStep == manager.parts.Length)
            manager.Restart();
        restartPrevious = restart;
    }

    void UpdatePose(XRNode node, Transform target)
    {
        if (target == null) return;
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;
        if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 p))
            target.localPosition = p;
        if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion r))
            target.localRotation = r;
    }

    void HandleHand(XRNode node, Transform hand, ref AssemblyPart held, ref bool previous)
    {
        if (hand == null) return;
        var device = InputDevices.GetDeviceAtXRNode(node);
        bool grip = device.TryGetFeatureValue(CommonUsages.grip, out float strength) && strength > 0.65f;
        if (grip && !previous && held == null)
        {
            AssemblyPart nearest = null;
            float best = grabRadius;
            foreach (AssemblyPart part in manager.parts)
            {
                if (!manager.CanGrab(part) || part.held) continue;
                float d = Vector3.Distance(hand.position, part.transform.position);
                if (d < best) { nearest = part; best = d; }
            }
            if (nearest != null) { held = nearest; held.held = true; }
        }
        if (held != null && grip)
        {
            held.transform.position = hand.TransformPoint(new Vector3(0f, 0f, holdDistance));
            held.transform.rotation = hand.rotation;
        }
        if (!grip && previous && held != null)
        {
            AssemblyPart dropped = held;
            held = null;
            dropped.held = false;
            manager.TryInstall(dropped);
        }
        previous = grip;
    }
}
