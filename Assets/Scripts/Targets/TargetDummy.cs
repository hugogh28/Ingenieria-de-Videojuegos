using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetDummy : MonoBehaviour, IHealth
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 9999999f;
    [SerializeField] private bool resetHealthOnStart = true;
    [SerializeField] private bool resetHealthWhenEmpty = true;

    [Header("Damage Numbers")]
    [SerializeField] private DamageNumber damageNumberPrefab;
    [SerializeField] private Transform damageNumberSpawnPoint;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private Color normalDamageColor = new Color(1f, 0.93f, 0.35f);
    [SerializeField] private Color criticalDamageColor = new Color(1f, 0.2f, 0.12f);
    [SerializeField] private float normalNumberScale = 1f;
    [SerializeField] private float criticalNumberScale = 1.45f;
    [SerializeField] private TMP_FontAsset damageNumberFont;
    [SerializeField] private bool forceBoldDamageNumbers = true;
    [SerializeField] private int damageNumberSortingOrder = 100;
    [SerializeField] private Camera damageNumberCamera;
    [SerializeField] private string damageNumberLayerName = "DamageNumbers";

    [Header("Metal Hit Sound")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private List<AudioClip> metalHitClips = new List<AudioClip>();
    [SerializeField] private float hitVolume = 1f;
    [SerializeField] private Vector2 randomPitchRange = new Vector2(0.92f, 1.08f);
    [SerializeField] private bool spatialAudio = true;

    [Header("Debug")]
    [SerializeField] private bool debugDamage = false;

    public float health { get; set; }

    public float MaxHealth => maxHealth;

    private void Awake()
    {
        CacheReferences();

        if (resetHealthOnStart)
        {
            health = maxHealth;
        }
    }

    private void OnEnable()
    {
        CacheReferences();

        if (resetHealthOnStart)
        {
            health = maxHealth;
        }
    }

    private void CacheReferences()
    {
        if (damageNumberCamera == null)
        {
            damageNumberCamera = Camera.main;
        }

        if (hitAudioSource == null)
        {
            hitAudioSource = GetComponent<AudioSource>();
        }

        if (hitAudioSource == null)
        {
            hitAudioSource = gameObject.AddComponent<AudioSource>();
        }

        hitAudioSource.playOnAwake = false;
        hitAudioSource.spatialBlend = spatialAudio ? 1f : 0f;
    }

    public void TakeDamage(float dmg)
    {
        TakeDamage(dmg, false, GetDefaultHitPoint());
    }

    public void TakeDamage(float dmg, bool isCritical, Vector3 hitPoint)
    {
        if (dmg <= 0f)
        {
            return;
        }

        health -= dmg;

        SpawnDamageNumber(hitPoint, dmg, isCritical);
        PlayRandomMetalHitSound();

        if (debugDamage)
        {
            Debug.Log($"{name}: target recibe {dmg} de daño. Vida restante: {health}.", this);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public void ResetTargetHealth()
    {
        health = maxHealth;
    }

    public void Die()
    {
        if (debugDamage)
        {
            Debug.Log($"{name}: TargetDummy.Die() llamado.", this);
        }

        if (resetHealthWhenEmpty)
        {
            health = maxHealth;
        }
    }

    private void SpawnDamageNumber(Vector3 hitPoint, float damage, bool isCritical)
    {
        Vector3 spawnPosition = damageNumberSpawnPoint != null
            ? damageNumberSpawnPoint.position
            : hitPoint + damageNumberOffset;

        spawnPosition += Random.insideUnitSphere * 0.12f;

        DamageNumber number = damageNumberPrefab != null
            ? Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity)
            : CreateFallbackDamageNumber(spawnPosition);

        if (number == null)
        {
            return;
        }

        int roundedDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
        Color color = isCritical ? criticalDamageColor : normalDamageColor;
        float scale = isCritical ? criticalNumberScale : normalNumberScale;
        Camera cameraToFace = damageNumberCamera != null ? damageNumberCamera : Camera.main;

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
        GameObject numberObject = new GameObject("Target Damage Number");
        numberObject.transform.position = position;

        TextMeshPro text = numberObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 3.2f;
        text.enableWordWrapping = false;

        return numberObject.AddComponent<DamageNumber>();
    }

    private void PlayRandomMetalHitSound()
    {
        if (hitAudioSource == null)
        {
            CacheReferences();
        }

        if (hitAudioSource == null || metalHitClips == null || metalHitClips.Count == 0)
        {
            return;
        }

        AudioClip clip = metalHitClips[Random.Range(0, metalHitClips.Count)];

        if (clip == null)
        {
            return;
        }

        if (!hitAudioSource.gameObject.activeSelf)
        {
            hitAudioSource.gameObject.SetActive(true);
        }

        if (!hitAudioSource.enabled)
        {
            hitAudioSource.enabled = true;
        }

        float minPitch = Mathf.Min(randomPitchRange.x, randomPitchRange.y);
        float maxPitch = Mathf.Max(randomPitchRange.x, randomPitchRange.y);

        hitAudioSource.pitch = Random.Range(minPitch, maxPitch);
        hitAudioSource.PlayOneShot(clip, hitVolume);
    }

    private Vector3 GetDefaultHitPoint()
    {
        Collider targetCollider = GetComponentInChildren<Collider>();

        if (targetCollider != null)
        {
            return targetCollider.bounds.center;
        }

        Renderer targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            return targetRenderer.bounds.center;
        }

        return transform.position + Vector3.up;
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
}
