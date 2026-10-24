using UnityEngine;


public class DesktopAssemblyController : MonoBehaviour
{
    public Camera viewCamera;
    public AssemblyManager manager;
    public float moveSpeed = 2f;
    public float mouseSensitivity = 2f;
    public float grabRange = 3f;
    public float carryDistance = 1.0f;
    public float placeRange = 0.30f;

    AssemblyPart held;
    float pitch;
    bool mouseLocked;

    void Start()
    {
        if (viewCamera == null) viewCamera = GetComponent<Camera>();
        if (manager == null) manager = FindFirstObjectByType<AssemblyManager>();
        pitch = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
        LockMouse(true);
    }

    void LockMouse(bool locked)
    {
        mouseLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape)) LockMouse(false);
        if (!mouseLocked && Input.GetMouseButtonDown(0)) { LockMouse(true); return; }
        if (!mouseLocked || manager == null || viewCamera == null) return;

        transform.parent.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0, Space.World);
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -80, 80);
        transform.localRotation = Quaternion.Euler(pitch, 0, 0);

        Vector3 planarForward = transform.parent.forward;
        planarForward.y = 0;
        planarForward.Normalize();
        Vector3 planarRight = transform.parent.right;
        planarRight.y = 0;
        planarRight.Normalize();
        Vector3 motion = planarForward * Input.GetAxisRaw("Vertical") + planarRight * Input.GetAxisRaw("Horizontal");
        transform.parent.position += Vector3.ClampMagnitude(motion, 1) * moveSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            if (held == null) GrabLookedAtPart();
            else DropHeld();
        }
        if (held != null)
        {
            held.transform.position = viewCamera.transform.position + viewCamera.transform.forward * carryDistance;
            // Preserve orientation for easier placement.
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Vector3.Distance(held.transform.position, held.snapTarget.position) <= placeRange)
                    DropHeld();
                else Debug.Log("Move the held part closer to its green placement guide.");
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && manager.currentStep >= manager.parts.Length)
            manager.Restart();
#endif
    }

    void GrabLookedAtPart()
    {
        if (!Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out RaycastHit hit, grabRange)) return;
        AssemblyPart candidate = hit.collider.GetComponentInParent<AssemblyPart>();
        if (candidate == null || !manager.CanGrab(candidate) || candidate.held) return;
        held = candidate;
        held.held = true;
    }

    void DropHeld()
    {
        AssemblyPart released = held;
        held = null;
        released.held = false;
    
        manager.TryInstall(released);
    }

    void OnDisable() { if (mouseLocked) LockMouse(false); }
}
