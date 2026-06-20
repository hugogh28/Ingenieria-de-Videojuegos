using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AjustesMenuUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;

    [Header("Textos")]
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private TMP_Text mouseSensitivityText;

    [Header("Toggles")]
    [SerializeField] private Toggle fullScreenToggle;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private bool applyResolutionImmediately = true;

    private readonly List<Vector2Int> resolutionValues = new List<Vector2Int>();
    private Ajustes ajustes;
    private bool initialized;

    private void Awake()
    {
        ajustes = Ajustes.EnsureExists();
        BuildQualityDropdown();
        BuildResolutionDropdown();
        SyncUIFromSettings();
        RegisterUIEvents();
        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }

        SyncUIFromSettings();
    }

    private void RegisterUIEvents()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (fullScreenToggle != null)
        {
            fullScreenToggle.onValueChanged.AddListener(OnFullScreenChanged);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
        }
    }

    private void BuildQualityDropdown()
    {
        if (qualityDropdown == null)
        {
            return;
        }

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        resolutionValues.Clear();
        resolutionDropdown.ClearOptions();

        Resolution[] resolutions = Screen.resolutions;

        foreach (Resolution resolution in resolutions)
        {
            Vector2Int value = new Vector2Int(resolution.width, resolution.height);

            if (!resolutionValues.Contains(value))
            {
                resolutionValues.Add(value);
            }
        }

        if (resolutionValues.Count == 0)
        {
            resolutionValues.Add(new Vector2Int(Screen.width, Screen.height));
        }

        List<string> options = new List<string>();
        foreach (Vector2Int value in resolutionValues)
        {
            options.Add(value.x + " x " + value.y);
        }

        resolutionDropdown.AddOptions(options);
    }

    private void SyncUIFromSettings()
    {
        if (ajustes == null)
        {
            ajustes = Ajustes.EnsureExists();
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(ajustes.MasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(ajustes.MusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(ajustes.SfxVolume);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.SetValueWithoutNotify(ajustes.MouseSensitivity);
        }

        if (fullScreenToggle != null)
        {
            fullScreenToggle.SetIsOnWithoutNotify(ajustes.FullScreen);
        }

        if (qualityDropdown != null && QualitySettings.names.Length > 0)
        {
            qualityDropdown.SetValueWithoutNotify(Mathf.Clamp(ajustes.QualityIndex, 0, QualitySettings.names.Length - 1));
        }

        if (resolutionDropdown != null)
        {
            int index = FindResolutionIndex(ajustes.ResolutionWidth, ajustes.ResolutionHeight);
            resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, Mathf.Max(0, resolutionValues.Count - 1)));
        }

        RefreshTexts();
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < resolutionValues.Count; i++)
        {
            if (resolutionValues[i].x == width && resolutionValues[i].y == height)
            {
                return i;
            }
        }

        for (int i = 0; i < resolutionValues.Count; i++)
        {
            if (resolutionValues[i].x == Screen.width && resolutionValues[i].y == Screen.height)
            {
                return i;
            }
        }

        return 0;
    }

    private void OnMasterVolumeChanged(float value)
    {
        ajustes.SetMasterVolume(value);
        RefreshTexts();
    }

    private void OnMusicVolumeChanged(float value)
    {
        ajustes.SetMusicVolume(value);
        RefreshTexts();
    }

    private void OnSfxVolumeChanged(float value)
    {
        ajustes.SetSfxVolume(value);
        RefreshTexts();
    }

    private void OnMouseSensitivityChanged(float value)
    {
        ajustes.SetMouseSensitivity(value);
        RefreshTexts();
    }

    private void OnFullScreenChanged(bool value)
    {
        ajustes.SetFullScreen(value);
    }

    private void OnQualityChanged(int index)
    {
        ajustes.SetQualityIndex(index);
    }

    private void OnResolutionDropdownChanged(int index)
    {
        if (applyResolutionImmediately)
        {
            ApplySelectedResolution();
        }
    }

    public void ApplySelectedResolution()
    {
        if (resolutionDropdown == null || resolutionValues.Count == 0)
        {
            return;
        }

        int index = Mathf.Clamp(resolutionDropdown.value, 0, resolutionValues.Count - 1);
        Vector2Int resolution = resolutionValues[index];
        ajustes.SetResolution(resolution.x, resolution.y);
    }

    public void ResetSettingsButton()
    {
        ajustes.ResetToDefaults();
        BuildResolutionDropdown();
        SyncUIFromSettings();
    }

    private void RefreshTexts()
    {
        SetPercentText(masterVolumeText, masterVolumeSlider != null ? masterVolumeSlider.value : ajustes.MasterVolume);
        SetPercentText(musicVolumeText, musicVolumeSlider != null ? musicVolumeSlider.value : ajustes.MusicVolume);
        SetPercentText(sfxVolumeText, sfxVolumeSlider != null ? sfxVolumeSlider.value : ajustes.SfxVolume);

        if (mouseSensitivityText != null)
        {
            float value = mouseSensitivitySlider != null ? mouseSensitivitySlider.value : ajustes.MouseSensitivity;
            mouseSensitivityText.text = Mathf.RoundToInt(value).ToString();
        }
    }

    private void SetPercentText(TMP_Text text, float value)
    {
        if (text == null)
        {
            return;
        }

        text.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
