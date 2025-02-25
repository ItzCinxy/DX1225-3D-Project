using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using TMPro;

[RequireComponent(typeof(Volume))]
public class NightVisionController : MonoBehaviour
{
    [SerializeField] private Color defaultLightColour;
    [SerializeField] private Color boostedLightColour;
    [SerializeField] private Volume volume;
    [SerializeField] private PlayerInput input;
    [SerializeField] private TMP_Text offOnText;

    private bool canNightVision = false;

    private bool isNightVisionEnabled;

    private void Start()
    {
        RenderSettings.ambientLight = defaultLightColour;

        if (offOnText != null)
        {
            offOnText.text = "";
        }
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
            offOnText.text = "ON";
        }
        else
        {
            RenderSettings.ambientLight = defaultLightColour;
            volume.weight = 0;
            offOnText.text = "OFF";
        }
    }

    public void UnlockNightVision()
    {
        canNightVision = true;
    }
}