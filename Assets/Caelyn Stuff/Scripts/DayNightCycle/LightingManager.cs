using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    // References
    [SerializeField] private Light directionalLight;
    [SerializeField] private LightingPreset preset;
    // Variables
    [SerializeField, Range(0, 24)] private float time;
    [SerializeField] private float timeMultiplier = 0.1f; // Adjust this to control speed

    private void Update()
    {
        if (preset == null)
            return;

        if (Application.isPlaying)
        {
            time += Time.deltaTime * timeMultiplier;
            time %= 24; // Clamp between 0-24
            UpdateLighting(time / 24f);
        }
        else
        {
            UpdateLighting(time / 24f);
        }
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);

        if (directionalLight != null)
        {
            directionalLight.color = preset.directionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (directionalLight != null)
            return;

        if (RenderSettings.sun != null)
            directionalLight = RenderSettings.sun;
        else
        { 
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    return;
                }
            }
        }

    }
}
