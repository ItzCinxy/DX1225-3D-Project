using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class NightVisionController : MonoBehaviour
{
    [SerializeField] private Color defaultLightColour;
    [SerializeField] private Color boostedLightColour;
    [SerializeField] private Volume volume;
    [SerializeField] private PlayerInput input;

    private bool canNightVision = false;

    private bool isNightVisionEnabled;

    private void Start()
    {
        RenderSettings.ambientLight = defaultLightColour;
    }

    private void Update()
    {
        if (input.actions["ToggleNightVision"].WasPressedThisFrame() && canNightVision)
        {
            ToggleNightVision();
        }
    }

    private void ToggleNightVision()
    {
        if (!canNightVision) return;

        isNightVisionEnabled = !isNightVisionEnabled;

        if (isNightVisionEnabled)
        {
            RenderSettings.ambientLight = boostedLightColour;
            volume.weight = 1;
        }
        else
        {
            RenderSettings.ambientLight = defaultLightColour;
            volume.weight = 0;
        }
    }

    public void UnlockNightVision()
    {
        canNightVision = true;
    }
}