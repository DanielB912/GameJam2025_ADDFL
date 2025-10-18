using UnityEngine;
using TMPro;

public class OptionsController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown resolutionDropdown;
    public UnityEngine.UI.Toggle fullscreenToggle;

    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;

    void Start()
    {
        // Llenar resoluciones soportadas
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string label = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRateRatio.value:0}Hz";
            options.Add(label);

            // detectar resolución actual
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Fullscreen
        fullscreenToggle.isOn = Screen.fullScreen;

        // Listeners
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
