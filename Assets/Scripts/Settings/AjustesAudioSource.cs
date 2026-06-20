using UnityEngine;

public class AjustesAudioSource : MonoBehaviour
{
    public enum AudioChannel
    {
        Music,
        Sfx
    }

    [SerializeField] private AudioChannel channel = AudioChannel.Sfx;
    [SerializeField] private AudioSource[] audioSources;
    [Range(0f, 1f)] [SerializeField] private float baseVolume = 1f;
    [SerializeField] private bool includeChildren = true;

    private Ajustes ajustes;

    private void Awake()
    {
        if (audioSources == null || audioSources.Length == 0)
        {
            audioSources = includeChildren
                ? GetComponentsInChildren<AudioSource>(true)
                : GetComponents<AudioSource>();
        }
    }

    private void OnEnable()
    {
        ajustes = Ajustes.EnsureExists();
        ajustes.Changed += ApplyVolume;
        ApplyVolume();
    }

    private void OnDisable()
    {
        if (ajustes != null)
        {
            ajustes.Changed -= ApplyVolume;
        }
    }

    public void ApplyVolume()
    {
        if (ajustes == null)
        {
            ajustes = Ajustes.EnsureExists();
        }

        float channelVolume = channel == AudioChannel.Music
            ? ajustes.MusicVolume
            : ajustes.SfxVolume;

        if (audioSources == null)
        {
            return;
        }

        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.volume = baseVolume * channelVolume;
            }
        }
    }
}
