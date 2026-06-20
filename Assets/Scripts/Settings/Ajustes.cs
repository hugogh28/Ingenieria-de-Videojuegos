using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class Ajustes : MonoBehaviour
{
    public static Ajustes Instance { get; private set; }

    public event Action Changed;

    private const string MasterVolumeKey = "settings.masterVolume";
    private const string MusicVolumeKey = "settings.musicVolume";
    private const string SfxVolumeKey = "settings.sfxVolume";
    private const string MouseSensitivityKey = "settings.mouseSensitivity";
    private const string QualityKey = "settings.quality";
    private const string FullScreenKey = "settings.fullscreen";
    private const string ResolutionWidthKey = "settings.resolutionWidth";
    private const string ResolutionHeightKey = "settings.resolutionHeight";

    [Header("Valores por defecto")]
    [Range(0f, 1f)] [SerializeField] private float defaultMasterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultMusicVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float defaultSfxVolume = 1f;
    [SerializeField] private float defaultMouseSensitivity = 100f;
    [SerializeField] private bool defaultFullScreen = true;
    [Tooltip("-1 usa el nivel de calidad actual del proyecto.")]
    [SerializeField] private int defaultQualityIndex = -1;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private float mouseSensitivity;
    private int qualityIndex;
    private bool fullScreen;
    private int resolutionWidth;
    private int resolutionHeight;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;
    public float MouseSensitivity => mouseSensitivity;
    public int QualityIndex => qualityIndex;
    public bool FullScreen => fullScreen;
    public int ResolutionWidth => resolutionWidth;
    public int ResolutionHeight => resolutionHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureExists();
    }

    public static Ajustes EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Ajustes existing = FindFirstObjectByType<Ajustes>();
        if (existing != null)
        {
            return existing;
        }

        GameObject settingsObject = new GameObject("[Ajustes]");
        return settingsObject.AddComponent<Ajustes>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ApplyAll(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyAll(true);
    }

    private void Load()
    {
        int defaultQuality = defaultQualityIndex >= 0 ? defaultQualityIndex : QualitySettings.GetQualityLevel();
        Resolution currentResolution = Screen.currentResolution;

        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaultMasterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
        mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, defaultMouseSensitivity);
        qualityIndex = PlayerPrefs.GetInt(QualityKey, defaultQuality);
        fullScreen = PlayerPrefs.GetInt(FullScreenKey, defaultFullScreen ? 1 : 0) == 1;
        resolutionWidth = PlayerPrefs.GetInt(ResolutionWidthKey, currentResolution.width);
        resolutionHeight = PlayerPrefs.GetInt(ResolutionHeightKey, currentResolution.height);

        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        mouseSensitivity = Mathf.Max(1f, mouseSensitivity);
        qualityIndex = Mathf.Clamp(qualityIndex, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        resolutionWidth = Mathf.Max(1, resolutionWidth);
        resolutionHeight = Mathf.Max(1, resolutionHeight);
    }

    public void ApplyAll(bool notify = true)
    {
        AudioListener.volume = masterVolume;

        if (QualitySettings.names.Length > 0)
        {
            qualityIndex = Mathf.Clamp(qualityIndex, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }

        Screen.SetResolution(resolutionWidth, resolutionHeight, fullScreen);

        if (notify)
        {
            Changed?.Invoke();
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        AudioListener.volume = masterVolume;
        SaveAndNotify();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        SaveAndNotify();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveAndNotify();
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = Mathf.Max(1f, value);
        SaveAndNotify();
    }

    public void SetQualityIndex(int index)
    {
        if (QualitySettings.names.Length <= 0)
        {
            return;
        }

        qualityIndex = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(qualityIndex, true);
        SaveAndNotify();
    }

    public void SetFullScreen(bool value)
    {
        fullScreen = value;
        Screen.SetResolution(resolutionWidth, resolutionHeight, fullScreen);
        SaveAndNotify();
    }

    public void SetResolution(int width, int height)
    {
        resolutionWidth = Mathf.Max(1, width);
        resolutionHeight = Mathf.Max(1, height);
        Screen.SetResolution(resolutionWidth, resolutionHeight, fullScreen);
        SaveAndNotify();
    }

    public void ResetToDefaults()
    {
        masterVolume = Mathf.Clamp01(defaultMasterVolume);
        musicVolume = Mathf.Clamp01(defaultMusicVolume);
        sfxVolume = Mathf.Clamp01(defaultSfxVolume);
        mouseSensitivity = Mathf.Max(1f, defaultMouseSensitivity);
        qualityIndex = defaultQualityIndex >= 0 ? defaultQualityIndex : QualitySettings.GetQualityLevel();
        fullScreen = defaultFullScreen;

        Resolution currentResolution = Screen.currentResolution;
        resolutionWidth = currentResolution.width;
        resolutionHeight = currentResolution.height;

        Save();
        ApplyAll(true);
    }

    private void SaveAndNotify()
    {
        Save();
        Changed?.Invoke();
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensitivity);
        PlayerPrefs.SetInt(QualityKey, qualityIndex);
        PlayerPrefs.SetInt(FullScreenKey, fullScreen ? 1 : 0);
        PlayerPrefs.SetInt(ResolutionWidthKey, resolutionWidth);
        PlayerPrefs.SetInt(ResolutionHeightKey, resolutionHeight);
        PlayerPrefs.Save();
    }
}
