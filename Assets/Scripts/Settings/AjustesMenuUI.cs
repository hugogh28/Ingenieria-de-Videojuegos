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

    [Header("Textos de valor")]
    [SerializeField] private TMP_Text masterVolumeValueText;
    [SerializeField] private TMP_Text musicVolumeValueText;
    [SerializeField] private TMP_Text sfxVolumeValueText;
    [SerializeField] private TMP_Text mouseSensitivityValueText;

    [Header("Pantalla")]
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private bool applyResolutionImmediately = true;

    private readonly List<ResolutionOption> resolutionOptions = new List<ResolutionOption>();
    private Ajustes settings;
    private bool updatingUI;

    private struct ResolutionOption
    {
        public int Width;
        public int Height;
        public string Label;
    }

    private void OnEnable()
    {
        settings = Ajustes.EnsureExists();
        settings.Changed += RefreshFromSettings;

        ConfigureSliders();
        BuildQualityDropdown();
        BuildResolutionDropdown();
        HookListeners();
        RefreshFromSettings();
    }

    private void OnDisable()
    {
        UnhookListeners();

        if (settings != null)
        {
            settings.Changed -= RefreshFromSettings;
        }
    }

    private void ConfigureSliders()
    {
        Configure01Slider(masterVolumeSlider);
        Configure01Slider(musicVolumeSlider);
        Configure01Slider(sfxVolumeSlider);
        Configure01Slider(mouseSensitivitySlider);
    }

    private void Configure01Slider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void HookListeners()
    {
        UnhookListeners();

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        if (fullScreenToggle != null) fullScreenToggle.onValueChanged.AddListener(OnFullScreenChanged);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void UnhookListeners()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
        if (fullScreenToggle != null) fullScreenToggle.onValueChanged.RemoveListener(OnFullScreenChanged);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
    }

    private void BuildQualityDropdown()
    {
        if (qualityDropdown == null)
        {
            return;
        }

        qualityDropdown.ClearOptions();

        string[] qualityNames = QualitySettings.names;
        List<string> options = new List<string>();

        if (qualityNames != null && qualityNames.Length > 0)
        {
            options.AddRange(qualityNames);
            qualityDropdown.interactable = true;
        }
        else
        {
            options.Add("Sin niveles de calidad");
            qualityDropdown.interactable = false;
        }

        qualityDropdown.AddOptions(options);
    }

    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        resolutionOptions.Clear();
        resolutionDropdown.ClearOptions();

        Resolution[] unityResolutions = Screen.resolutions;
        HashSet<string> used = new HashSet<string>();

        for (int i = 0; i < unityResolutions.Length; i++)
        {
            AddResolutionOption(unityResolutions[i].width, unityResolutions[i].height, used);
        }

        if (resolutionOptions.Count == 0)
        {
            AddResolutionOption(Screen.currentResolution.width, Screen.currentResolution.height, used);
        }

        List<string> labels = new List<string>();
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            labels.Add(resolutionOptions[i].Label);
        }

        resolutionDropdown.AddOptions(labels);
    }

    private void AddResolutionOption(int width, int height, HashSet<string> used)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        string key = width + "x" + height;
        if (used.Contains(key))
        {
            return;
        }

        used.Add(key);
        resolutionOptions.Add(new ResolutionOption
        {
            Width = width,
            Height = height,
            Label = width + " x " + height
        });
    }

    private void RefreshFromSettings()
    {
        if (settings == null)
        {
            settings = Ajustes.EnsureExists();
        }

        updatingUI = true;

        SetSliderWithoutNotify(masterVolumeSlider, settings.MasterVolume);
        SetSliderWithoutNotify(musicVolumeSlider, settings.MusicVolume);
        SetSliderWithoutNotify(sfxVolumeSlider, settings.SfxVolume);
        SetSliderWithoutNotify(mouseSensitivitySlider, settings.MouseSensitivity01);

        UpdateValueText(masterVolumeValueText, settings.MasterVolume);
        UpdateValueText(musicVolumeValueText, settings.MusicVolume);
        UpdateValueText(sfxVolumeValueText, settings.SfxVolume);
        UpdateValueText(mouseSensitivityValueText, settings.MouseSensitivity01);

        if (fullScreenToggle != null)
        {
            fullScreenToggle.SetIsOnWithoutNotify(settings.FullScreen);
        }

        if (qualityDropdown != null && QualitySettings.names.Length > 0)
        {
            int index = Mathf.Clamp(settings.QualityIndex, 0, QualitySettings.names.Length - 1);
            qualityDropdown.SetValueWithoutNotify(index);
            qualityDropdown.RefreshShownValue();
        }

        if (resolutionDropdown != null && resolutionOptions.Count > 0)
        {
            int index = GetResolutionIndex(settings.ResolutionWidth, settings.ResolutionHeight);
            resolutionDropdown.SetValueWithoutNotify(index);
            resolutionDropdown.RefreshShownValue();
        }

        updatingUI = false;
    }

    private int GetResolutionIndex(int width, int height)
    {
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            if (resolutionOptions[i].Width == width && resolutionOptions[i].Height == height)
            {
                return i;
            }
        }

        return Mathf.Clamp(resolutionOptions.Count - 1, 0, Mathf.Max(0, resolutionOptions.Count - 1));
    }

    private void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void UpdateValueText(TMP_Text text, float value)
    {
        if (text == null)
        {
            return;
        }

        text.text = Mathf.Clamp01(value).ToString("0.0000");
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (updatingUI) return;

        settings.SetMasterVolume(value);
        UpdateValueText(masterVolumeValueText, value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (updatingUI) return;

        settings.SetMusicVolume(value);
        UpdateValueText(musicVolumeValueText, value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (updatingUI) return;

        settings.SetSfxVolume(value);
        UpdateValueText(sfxVolumeValueText, value);
    }

    private void OnMouseSensitivityChanged(float value)
    {
        if (updatingUI) return;

        settings.SetMouseSensitivity01(value);
        UpdateValueText(mouseSensitivityValueText, value);
    }

    private void OnFullScreenChanged(bool value)
    {
        if (updatingUI) return;

        settings.SetFullScreen(value);
    }

    private void OnQualityChanged(int index)
    {
        if (updatingUI) return;

        settings.SetQualityIndex(index);
    }

    private void OnResolutionChanged(int index)
    {
        if (updatingUI || !applyResolutionImmediately)
        {
            return;
        }

        ApplyResolutionIndex(index);
    }

    public void ApplySelectedResolution()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        ApplyResolutionIndex(resolutionDropdown.value);
    }

    private void ApplyResolutionIndex(int index)
    {
        if (settings == null || resolutionOptions.Count <= 0)
        {
            return;
        }

        index = Mathf.Clamp(index, 0, resolutionOptions.Count - 1);
        ResolutionOption option = resolutionOptions[index];
        settings.SetResolution(option.Width, option.Height);
    }

    public void ResetSettingsButton()
    {
        if (settings == null)
        {
            settings = Ajustes.EnsureExists();
        }

        settings.ResetToDefaults();
        BuildQualityDropdown();
        BuildResolutionDropdown();
        RefreshFromSettings();
    }
}
