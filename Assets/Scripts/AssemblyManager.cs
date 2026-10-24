using UnityEngine;

public class AssemblyManager : MonoBehaviour
{
    public AssemblyPart[] parts;
    public Renderer[] guides;
    public TextMesh statusText;
    public TextMesh progressText;
    public float snapDistance = 0.17f;
    public int currentStep;

    void Start()
    {
        foreach (AssemblyPart part in parts) part.CacheStart();
        Refresh();
    }

    public bool CanGrab(AssemblyPart part) => part != null && !part.installed && part.stepIndex == currentStep;

    public bool TryInstall(AssemblyPart part)
    {
        if (!CanGrab(part) || Vector3.Distance(part.transform.position, part.snapTarget.position) > snapDistance)
            return false;
        part.transform.SetParent(part.snapTarget, true);
        part.transform.SetPositionAndRotation(part.snapTarget.position, part.snapTarget.rotation);
        part.installed = true;
        part.held = false;
        currentStep++;
        Refresh();
        return true;
    }

    public void Restart()
    {
        currentStep = 0;
        foreach (AssemblyPart part in parts)
        {
            part.installed = false;
            part.held = false;
            part.transform.SetParent(part.originalParent, true);
            part.transform.SetPositionAndRotation(part.initialPosition, part.initialRotation);
        }
        Refresh();
    }

    void Refresh()
    {
        for (int i = 0; i < guides.Length; i++)
            if (guides[i] != null) guides[i].enabled = (i == currentStep);
        if (progressText != null) progressText.text = "ASSEMBLY  " + currentStep + "/" + parts.Length;
        if (statusText != null)
            statusText.text = currentStep >= parts.Length
                ? "COMPLETE!  All 8 parts installed.\nPress A to restart."
                : "STEP " + (currentStep + 1) + ": " + parts[currentStep].name +
                  "\nGrip to pick up. Release at the green guide.";
    }
}
