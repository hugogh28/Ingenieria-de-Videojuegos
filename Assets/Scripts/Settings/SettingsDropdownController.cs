using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDropdownController : MonoBehaviour
{
    private const string QualityKey = "settings.quality";
    private const string ResolutionWidthKey = "settings.resolutionWidth";
    private const string ResolutionHeightKey = "settings.resolutionHeight";
    private const string FullScreenKey = "settings.fullscreen";

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Opcional")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Pantalla")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool useExclusiveFullscreen = false;

    private readonly List<ResolutionOption> resolutionOptions = new();

    private struct ResolutionOption
    {
        public int width;
        public int height;

        public ResolutionOption(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public string Label => width + " x " + height;
    }

    private void Awake()
    {
        BuildQualityDropdown();
        BuildResolutionDropdown();
        LoadSavedValuesIntoUI();

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        if (applyOnStart)
            ApplyCurrentUISettings();
    }

    private void OnDestroy()
    {
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }

    private void BuildQualityDropdown()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();

        List<string> qualityNames = new(QualitySettings.names);

        if (qualityNames.Count == 0)
            qualityNames.Add("Default");

        qualityDropdown.AddOptions(qualityNames);
        qualityDropdown.RefreshShownValue();
    }

    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        resolutionDropdown.ClearOptions();
        resolutionOptions.Clear();

        List<string> labels = new();
        HashSet<string> added = new();

        Resolution[] resolutions = Screen.resolutions;

        foreach (Resolution resolution in resolutions)
        {
            string key = resolution.width + "x" + resolution.height;

            if (!added.Add(key))
                continue;

            ResolutionOption option = new(resolution.width, resolution.height);
            resolutionOptions.Add(option);
            labels.Add(option.Label);
        }

        // Fallback para el editor o plataformas donde Screen.resolutions pueda venir vacío.
        if (resolutionOptions.Count == 0)
        {
            int width = Screen.width > 0 ? Screen.width : 1920;
            int height = Screen.height > 0 ? Screen.height : 1080;

            ResolutionOption option = new(width, height);
            resolutionOptions.Add(option);
            labels.Add(option.Label);
        }

        resolutionDropdown.AddOptions(labels);
        resolutionDropdown.RefreshShownValue();
    }

    private void LoadSavedValuesIntoUI()
    {
        if (qualityDropdown != null)
        {
            int savedQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
            savedQuality = Mathf.Clamp(savedQuality, 0, qualityDropdown.options.Count - 1);

            qualityDropdown.SetValueWithoutNotify(savedQuality);
            qualityDropdown.RefreshShownValue();
        }

        if (fullscreenToggle != null)
        {
            bool savedFullscreen = PlayerPrefs.GetInt(FullScreenKey, Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
        }

        if (resolutionDropdown != null && resolutionOptions.Count > 0)
        {
            int savedWidth = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width);
            int savedHeight = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height);

            int resolutionIndex = FindResolutionIndex(savedWidth, savedHeight);

            if (resolutionIndex < 0)
                resolutionIndex = FindResolutionIndex(Screen.width, Screen.height);

            if (resolutionIndex < 0)
                resolutionIndex = resolutionOptions.Count - 1;

            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
            resolutionDropdown.RefreshShownValue();
        }
    }

    private void OnQualityChanged(int index)
    {
        ApplyQuality(index);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int index)
    {
        ApplyResolution(index);
        PlayerPrefs.Save();
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        PlayerPrefs.SetInt(FullScreenKey, isFullscreen ? 1 : 0);

        if (resolutionDropdown != null)
            ApplyResolution(resolutionDropdown.value);

        PlayerPrefs.Save();
    }

    private void ApplyCurrentUISettings()
    {
        if (qualityDropdown != null)
            ApplyQuality(qualityDropdown.value);

        if (resolutionDropdown != null)
            ApplyResolution(resolutionDropdown.value);

        PlayerPrefs.Save();
    }

    private void ApplyQuality(int index)
    {
        if (QualitySettings.names == null || QualitySettings.names.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);

        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt(QualityKey, index);
    }

    private void ApplyResolution(int index)
    {
        if (resolutionOptions.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, resolutionOptions.Count - 1);

        ResolutionOption option = resolutionOptions[index];
        bool isFullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen;

        FullScreenMode mode = isFullscreen
            ? (useExclusiveFullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.FullScreenWindow)
            : FullScreenMode.Windowed;

        Screen.SetResolution(option.width, option.height, mode);

        PlayerPrefs.SetInt(ResolutionWidthKey, option.width);
        PlayerPrefs.SetInt(ResolutionHeightKey, option.height);
        PlayerPrefs.SetInt(FullScreenKey, isFullscreen ? 1 : 0);
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            if (resolutionOptions[i].width == width && resolutionOptions[i].height == height)
                return i;
        }

        return -1;
    }
}
