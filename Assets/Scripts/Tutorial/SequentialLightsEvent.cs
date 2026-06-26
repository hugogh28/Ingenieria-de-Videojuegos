using System.Collections;
using UnityEngine;

/// <summary>
/// Secuencia de encendido de luces para Unity.
/// 
/// Flujo:
/// 1. Recibe evento EncenderLuces().
/// 2. Enciende el primer set de luces.
/// 3. Reproduce el sonido del primer set.
/// 4. Espera delayBetweenLightSets segundos.
/// 5. Enciende el segundo set de luces.
/// 6. Reproduce el sonido del segundo set.
/// 7. Al terminar, activa una lista de AudioSources.
/// 
/// Pensado para conectar desde:
/// - UnityEvent
/// - Botón
/// - Interactable / Ineractable
/// - Animation Event
/// </summary>
public class SequentialLightsEvent : MonoBehaviour
{
    [Header("Primer set de luces")]
    [SerializeField] private Light[] firstLightSet;
    [SerializeField] private GameObject[] firstExtraObjectsToEnable;
    [SerializeField] private AudioSource firstLightSound;

    [Header("Segundo set de luces")]
    [SerializeField] private Light[] secondLightSet;
    [SerializeField] private GameObject[] secondExtraObjectsToEnable;
    [SerializeField] private AudioSource secondLightSound;

    [Header("Tiempos")]
    [SerializeField] private float delayBetweenLightSets = 2f;
    [SerializeField] private float delayBeforeFinalAudioSources = 0f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("AudioSources al final")]
    [Tooltip("AudioSources que se activan al final de la secuencia. Pueden tener el componente disabled o el GameObject inactive.")]
    [SerializeField] private AudioSource[] audioSourcesToEnableAtEnd;

    [SerializeField] private bool playFinalAudioSourcesWhenEnabled = true;

    [Header("Estado inicial")]
    [SerializeField] private bool turnEverythingOffOnStart = true;
    [SerializeField] private bool stopSoundsOnStart = true;

    [Header("Repetición")]
    [SerializeField] private bool allowRepeat = false;
    [SerializeField] private bool ignoreWhileRunning = true;

    private Coroutine sequenceCoroutine;
    private bool hasCompleted;

    private void Start()
    {
        if (!turnEverythingOffOnStart)
            return;

        SetLightsEnabled(firstLightSet, false);
        SetLightsEnabled(secondLightSet, false);

        SetGameObjectsActive(firstExtraObjectsToEnable, false);
        SetGameObjectsActive(secondExtraObjectsToEnable, false);

        if (stopSoundsOnStart)
        {
            StopAudioSource(firstLightSound);
            StopAudioSource(secondLightSound);

            for (int i = 0; i < audioSourcesToEnableAtEnd.Length; i++)
                StopAudioSource(audioSourcesToEnableAtEnd[i]);
        }

        DisableFinalAudioSources();
    }

    /// <summary>
    /// Método principal para conectar en eventos.
    /// </summary>
    public void EncenderLuces()
    {
        PlaySequence();
    }

    /// <summary>
    /// Alias por si prefieres nombre en inglés desde UnityEvent.
    /// </summary>
    public void PlaySequence()
    {
        if (sequenceCoroutine != null && ignoreWhileRunning)
            return;

        if (hasCompleted && !allowRepeat)
            return;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(LightSequenceCoroutine());
    }

    /// <summary>
    /// Permite reiniciar el sistema y volver a ejecutar la secuencia.
    /// </summary>
    public void ResetSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        hasCompleted = false;

        SetLightsEnabled(firstLightSet, false);
        SetLightsEnabled(secondLightSet, false);

        SetGameObjectsActive(firstExtraObjectsToEnable, false);
        SetGameObjectsActive(secondExtraObjectsToEnable, false);

        StopAudioSource(firstLightSound);
        StopAudioSource(secondLightSound);

        DisableFinalAudioSources();
    }

    private IEnumerator LightSequenceCoroutine()
    {
        // Primer set
        SetGameObjectsActive(firstExtraObjectsToEnable, true);
        SetLightsEnabled(firstLightSet, true);
        PlayAudioSource(firstLightSound);

        yield return Wait(delayBetweenLightSets);

        // Segundo set
        SetGameObjectsActive(secondExtraObjectsToEnable, true);
        SetLightsEnabled(secondLightSet, true);
        PlayAudioSource(secondLightSound);

        if (delayBeforeFinalAudioSources > 0f)
            yield return Wait(delayBeforeFinalAudioSources);

        EnableFinalAudioSources();

        hasCompleted = true;
        sequenceCoroutine = null;
    }

    private IEnumerator Wait(float seconds)
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }

    private void SetLightsEnabled(Light[] lights, bool enabled)
    {
        if (lights == null)
            return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null)
                continue;

            if (!lights[i].gameObject.activeSelf && enabled)
                lights[i].gameObject.SetActive(true);

            lights[i].enabled = enabled;
        }
    }

    private void SetGameObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
                continue;

            objects[i].SetActive(active);
        }
    }

    private void PlayAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        if (!source.gameObject.activeSelf)
            source.gameObject.SetActive(true);

        if (!source.enabled)
            source.enabled = true;

        source.Stop();
        source.Play();
    }

    private void StopAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        if (!source.gameObject.activeSelf)
            return;

        if (!source.enabled)
            source.enabled = true;

        source.Stop();
    }

    private void DisableFinalAudioSources()
    {
        if (audioSourcesToEnableAtEnd == null)
            return;

        for (int i = 0; i < audioSourcesToEnableAtEnd.Length; i++)
        {
            AudioSource source = audioSourcesToEnableAtEnd[i];

            if (source == null)
                continue;

            if (source.gameObject.activeSelf)
                source.Stop();

            source.enabled = false;
        }
    }

    private void EnableFinalAudioSources()
    {
        if (audioSourcesToEnableAtEnd == null)
            return;

        for (int i = 0; i < audioSourcesToEnableAtEnd.Length; i++)
        {
            AudioSource source = audioSourcesToEnableAtEnd[i];

            if (source == null)
                continue;

            if (!source.gameObject.activeSelf)
                source.gameObject.SetActive(true);

            source.enabled = true;

            if (playFinalAudioSourcesWhenEnabled)
            {
                source.Stop();
                source.Play();
            }
        }
    }
}
