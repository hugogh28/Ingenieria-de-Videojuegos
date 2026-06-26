using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AjustesAudioSource : MonoBehaviour
{
    public enum AudioChannel
    {
        Music,
        Sfx,
        MasterOnly
    }

    [SerializeField] private AudioChannel channel = AudioChannel.Sfx;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool captureInitialVolumeOnAwake = true;
    [SerializeField] private float baseVolume = 1f;

    private Ajustes settings;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (captureInitialVolumeOnAwake && audioSource != null)
        {
            baseVolume = audioSource.volume;
        }
    }

    private void OnEnable()
    {
        settings = Ajustes.EnsureExists();
        settings.Changed += ApplyVolume;
        ApplyVolume();
    }

    private void OnDisable()
    {
        if (settings != null)
        {
            settings.Changed -= ApplyVolume;
        }
    }

    public void ApplyVolume()
    {
        if (audioSource == null)
        {
            return;
        }

        if (settings == null)
        {
            settings = Ajustes.EnsureExists();
        }

        float channelVolume = 1f;

        switch (channel)
        {
            case AudioChannel.Music:
                channelVolume = settings.MusicVolume;
                break;
            case AudioChannel.Sfx:
                channelVolume = settings.SfxVolume;
                break;
            case AudioChannel.MasterOnly:
                channelVolume = 1f;
                break;
        }

        audioSource.volume = Mathf.Clamp01(baseVolume * channelVolume);
    }

    public void CaptureCurrentVolumeAsBase()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            baseVolume = audioSource.volume;
        }
    }
}
