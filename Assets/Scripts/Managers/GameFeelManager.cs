using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameFeelManager : MonoBehaviour
{
    private static GameFeelManager instance;

    public static GameFeelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameFeelManager>();
            }

            if (instance == null)
            {
                GameObject managerObject = new GameObject("GameFeelManager");
                instance = managerObject.AddComponent<GameFeelManager>();
            }

            return instance;
        }
    }

    [Header("Damage Numbers")]
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Color normalDamageColor = new Color(1f, 0.93f, 0.35f);
    [SerializeField] private Color criticalDamageColor = new Color(1f, 0.2f, 0.12f);
    [SerializeField] private float normalNumberScale = 1f;
    [SerializeField] private float criticalNumberScale = 1.45f;
    [SerializeField] private TMP_FontAsset damageNumberFont;
    [SerializeField] private bool forceBoldDamageNumbers = true;
    [SerializeField] private int damageNumberSortingOrder = 100;

    [Header("Damage Number Camera")]
    [SerializeField] private Camera damageNumberCamera;
    [SerializeField] private string damageNumberLayerName = "DamageNumbers";

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float shotShakeIntensity = 0.025f;
    [SerializeField] private float shotShakeDuration = 0.055f;
    [SerializeField] private float hitShakeIntensity = 0.045f;
    [SerializeField] private float hitShakeDuration = 0.08f;
    [SerializeField] private float criticalShakeIntensity = 0.075f;
    [SerializeField] private float criticalShakeDuration = 0.11f;
    [SerializeField] private float deathShakeIntensity = 0.12f;
    [SerializeField] private float deathShakeDuration = 0.18f;

    [Header("Audio")]
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private List<AudioClip> ratHitClips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> ratCriticalClips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> ratDeathClips = new List<AudioClip>();
    [SerializeField] private float normalHitPitch = 1f;
    [SerializeField] private float criticalHitPitch = 1.18f;
    [SerializeField] private float deathPitch = 0.9f;
    [SerializeField] private bool useProceduralFallbackSounds = true;

    [Header("Death Explosion")]
    [SerializeField] private ParticleSystem ratDeathExplosionPrefab;
    [SerializeField] private bool preserveExplosionPrefabRotation = true;
    [SerializeField] private Vector3 deathExplosionRotationOffset;
    [SerializeField] private int fallbackExplosionParticles = 26;
    [SerializeField] private Color fallbackExplosionColor = new Color(1f, 0.22f, 0.08f);

    private CameraShake cameraShake;
    private AudioClip fallbackHitClip;
    private AudioClip fallbackCriticalClip;
    private AudioClip fallbackDeathClip;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ResolveReferences();
    }

    public void PlayShotFeedback()
    {
        ShakeCamera(shotShakeIntensity, shotShakeDuration);
    }

    public void PlayRatHitFeedback(Vector3 hitPoint, float damage, bool isCritical)
    {
        SpawnDamageNumber(hitPoint, damage, isCritical);

        float intensity = isCritical ? criticalShakeIntensity : hitShakeIntensity;
        float duration = isCritical ? criticalShakeDuration : hitShakeDuration;
        ShakeCamera(intensity, duration);

        PlayRandomClip(
            isCritical && ratCriticalClips.Count > 0 ? ratCriticalClips : ratHitClips,
            isCritical ? criticalHitPitch : normalHitPitch,
            isCritical ? fallbackCriticalClip : fallbackHitClip
        );
    }

    public void PlayRatDeathFeedback(Vector3 deathPosition)
    {
        ShakeCamera(deathShakeIntensity, deathShakeDuration);
        PlayRandomClip(ratDeathClips, deathPitch, fallbackDeathClip);
        SpawnRatDeathExplosion(deathPosition);
    }

    private void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (damageNumberCamera == null)
        {
            damageNumberCamera = targetCamera;
        }

        if (targetCamera != null)
        {
            cameraShake = targetCamera.GetComponent<CameraShake>();

            if (cameraShake == null)
            {
                cameraShake = targetCamera.gameObject.AddComponent<CameraShake>();
            }
        }

        if (feedbackAudioSource == null)
        {
            feedbackAudioSource = GetComponent<AudioSource>();
        }

        if (feedbackAudioSource == null)
        {
            feedbackAudioSource = gameObject.AddComponent<AudioSource>();
            feedbackAudioSource.spatialBlend = 0f;
            feedbackAudioSource.playOnAwake = false;
        }

        if (useProceduralFallbackSounds)
        {
            fallbackHitClip = CreateProceduralClip("Rat Hit Pop", 520f, 180f, 0.08f, 0.35f);
            fallbackCriticalClip = CreateProceduralClip("Rat Critical Crack", 920f, 260f, 0.11f, 0.45f);
            fallbackDeathClip = CreateProceduralClip("Rat Death Burst", 180f, 55f, 0.18f, 0.55f);
        }
    }

    private void SpawnDamageNumber(Vector3 hitPoint, float damage, bool isCritical)
    {
        DamageNumber number = damageNumberPrefab != null
            ? Instantiate(damageNumberPrefab, hitPoint + Random.insideUnitSphere * 0.12f, Quaternion.identity)
            : CreateFallbackDamageNumber(hitPoint + Random.insideUnitSphere * 0.12f);

        if (number == null)
        {
            return;
        }

        int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
        Color color = isCritical ? criticalDamageColor : normalDamageColor;
        float scale = isCritical ? criticalNumberScale : normalNumberScale;
        Camera cameraToFace = damageNumberCamera != null ? damageNumberCamera : targetCamera;

        TrySetDamageNumberLayer(number.gameObject);

        number.Show(
            roundedDamage,
            isCritical,
            color,
            scale,
            cameraToFace,
            damageNumberFont,
            forceBoldDamageNumbers,
            damageNumberSortingOrder
        );
    }

    private DamageNumber CreateFallbackDamageNumber(Vector3 position)
    {
        GameObject numberObject = new GameObject("Damage Number");
        numberObject.transform.position = position;

        TextMeshPro text = numberObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 3.2f;
        text.enableWordWrapping = false;

        return numberObject.AddComponent<DamageNumber>();
    }

    private void TrySetDamageNumberLayer(GameObject numberObject)
    {
        if (numberObject == null || string.IsNullOrWhiteSpace(damageNumberLayerName))
        {
            return;
        }

        int layer = LayerMask.NameToLayer(damageNumberLayerName);

        if (layer < 0)
        {
            Debug.LogWarning($"GameFeelManager: no existe la layer '{damageNumberLayerName}'. Crea esa layer en Unity para renderizar los numeros con una camara separada.", this);
            return;
        }

        SetLayerRecursively(numberObject, layer);
    }

    private void SetLayerRecursively(GameObject targetObject, int layer)
    {
        targetObject.layer = layer;

        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void ShakeCamera(float intensity, float duration)
    {
        if (cameraShake == null)
        {
            ResolveReferences();
        }

        cameraShake?.Shake(intensity, duration);
    }

    private void PlayRandomClip(List<AudioClip> clips, float pitch, AudioClip fallbackClip)
    {
        if (feedbackAudioSource == null)
        {
            return;
        }

        AudioClip clip = clips != null && clips.Count > 0
            ? clips[Random.Range(0, clips.Count)]
            : fallbackClip;

        if (clip == null)
        {
            return;
        }

        feedbackAudioSource.pitch = pitch;
        feedbackAudioSource.PlayOneShot(clip);
    }

    private AudioClip CreateProceduralClip(string clipName, float startFrequency, float endFrequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
        float[] data = new float[samples];
        float phase = 0f;

        for (int i = 0; i < samples; i++)
        {
            float t = samples <= 1 ? 1f : i / (samples - 1f);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
            float envelope = Mathf.Pow(1f - t, 2f);
            phase += Mathf.PI * 2f * frequency / sampleRate;
            data[i] = Mathf.Sin(phase) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void SpawnRatDeathExplosion(Vector3 position)
    {
        if (ratDeathExplosionPrefab != null)
        {
            Quaternion prefabRotation = preserveExplosionPrefabRotation
                ? ratDeathExplosionPrefab.transform.rotation
                : Quaternion.identity;
            Quaternion finalRotation = prefabRotation * Quaternion.Euler(deathExplosionRotationOffset);

            ParticleSystem explosion = Instantiate(ratDeathExplosionPrefab, position, finalRotation);
            Destroy(explosion.gameObject, 3f);
            return;
        }

        StartCoroutine(FallbackExplosion(position));
    }

    private IEnumerator FallbackExplosion(Vector3 position)
    {
        GameObject explosionObject = new GameObject("Rat Death Explosion");
        explosionObject.transform.position = position;
        explosionObject.transform.rotation = Quaternion.Euler(deathExplosionRotationOffset);

        ParticleSystem explosion = explosionObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = explosion.main;
        main.startLifetime = 0.45f;
        main.startSpeed = 4.5f;
        main.startSize = 0.18f;
        main.startColor = fallbackExplosionColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = explosion.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = explosion.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, (short)Mathf.Max(1, fallbackExplosionParticles));
        emission.SetBursts(new[] { burst });

        explosion.Play();

        yield return new WaitForSeconds(1.2f);
        Destroy(explosionObject);
    }
}
