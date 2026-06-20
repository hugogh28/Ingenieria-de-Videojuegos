using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour, IInteractable
{
    public bool isActiveWeapon;

    [Header("Gun Properties")]
    public float weaponDamage = 20;
    [Range(0f, 1f)] public float criticalChance = 0.15f;
    [Min(1f)] public float criticalMultiplier = 1.75f;
    public int pointsGivenOnRatHit = 10;
    public float shootingDelay = 2f;
    public int bulletsPerBurst = 1;
    public float spreadIntensity;

    [Header("Fire Delay")]
    [SerializeField, Min(0f)] private float fireDelay = 0f;

    [Header("Raycast Damage")]
    [SerializeField] private LayerMask shotMask = ~0;
    [SerializeField] private float shotRange = 100f;
    [SerializeField] private QueryTriggerInteraction shotTriggerInteraction = QueryTriggerInteraction.Ignore;
    [SerializeField] private bool debugShotRay = false;
    [SerializeField] private bool spawnRaycastImpact = false;

    [Header("Bullet Properties")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;

    [Header("Visuals")]
    public GameObject muzzleEffect;
    internal Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";
    [SerializeField] private string reloadTriggerName = "Reload";
    [SerializeField] private string handsShootTriggerName = "Shoot";
    [SerializeField] private string handsReloadTriggerName = "Reload";
    private PlayerController playerController;

    [Header("Reload")]
    public float reloadTime;
    public int magazineSize, bulletsLeft;

    [Header("Reload Bucket")]
    [SerializeField] private Renderer[] bucketRenderers;

    private float minAmmoLevel, maxAmmoLevel, currentPotatoLevel, drainPerBullet;
    private Coroutine reloadRoutine;
    private Coroutine fireRoutine;

    [Header("Weapon Type")]
    public WeaponModel weaponModel;
    public enum WeaponModel
    {
        Lanzapatatas,
        CañonSalado,
        FuriaFrijol,
        DulceFuria,
        Rociasalsa,
        TorrenteAgrio,
        Cruzadientes,
        Picaflechas,
        Aguarrafaga,
        Nitrosifon
    }

    public ShootingMode currentShootingMode;
    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    [Header("Hand Transform")]
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    [Header("Debug")]
    public bool isShooting;
    public bool readyToShoot, next;
    bool allowReset = true;
    public bool isReloading;
    public int burstBulletsLeft;
    public Transform potatoAmmo;

    [Header("Animation Event Particles")]
    [SerializeField] private Transform smokeSpawnPoint;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private ParticleSystem potatosParticleSystem;

    [Header("Sounds")]
    private AudioSource _source;
    [SerializeField] private List<AudioClip> moveWeaponSounds;
    [SerializeField] private AudioClip potatoRefillSound;
    [SerializeField] private AudioClip pressureSound;

    public string InteractionActionText => $"recoger {GetInteractionWeaponName()}";

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();
        playerController = FindFirstObjectByType<PlayerController>();
        _source = GetComponent<AudioSource>();

        bulletsLeft = magazineSize;

        if (potatoAmmo != null)
        {
            maxAmmoLevel = potatoAmmo.localPosition.y;
            minAmmoLevel = maxAmmoLevel - 0.05f;

            currentPotatoLevel = maxAmmoLevel;
            drainPerBullet = (maxAmmoLevel - minAmmoLevel) / magazineSize;
        }

        HideBucket();
        // SoundManager.Instance.PlayIdleSound(weaponModel);
    }

    void Update()
    {
        if (isActiveWeapon)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
                foreach (Transform child2 in child)
                {
                    child2.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
                }
            }

            Outline outline = GetComponent<Outline>();

            if (outline != null)
            {
                outline.enabled = false;
            }

            if (currentShootingMode == ShootingMode.Auto)
            {
                isShooting = Input.GetKey(KeyCode.Mouse0);
            }
            else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
            {
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false && readyToShoot)
            {
                Reload();
            }

            if (readyToShoot && isShooting == false && isReloading == false && bulletsLeft <= 0)
            {
                Reload();
            }

            if (readyToShoot && isShooting && bulletsLeft > 0 && isReloading == false)
            {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

    public bool CanInteract(PlayerController player)
    {
        return !isActiveWeapon && WeaponManager.Instance != null;
    }

    public void Interact(PlayerController player)
    {
        if (!CanInteract(player))
        {
            return;
        }

        WeaponManager.Instance.PickupWeapon(gameObject);
    }

    private string GetInteractionWeaponName()
    {
        switch (weaponModel)
        {
            case WeaponModel.Lanzapatatas:
                return "Lanzapatatas";
            case WeaponModel.CañonSalado:
                return "Cañon Salado";
            case WeaponModel.FuriaFrijol:
                return "Furia Frijol";
            case WeaponModel.DulceFuria:
                return "Dulce Furia";
            case WeaponModel.Rociasalsa:
                return "Rociasalsa";
            case WeaponModel.TorrenteAgrio:
                return "Torrente Agrio";
            case WeaponModel.Cruzadientes:
                return "Cruzadientes";
            case WeaponModel.Picaflechas:
                return "Picaflechas";
            case WeaponModel.Aguarrafaga:
                return "Aguarrafaga";
            case WeaponModel.Nitrosifon:
                return "Nitrosifon";
            default:
                return "arma";
        }
    }

    private void FireWeapon()
    {
        if (!CanStartFireSequence())
        {
            return;
        }

        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
        }

        fireRoutine = StartCoroutine(FireSequenceRoutine());
    }

    private IEnumerator FireSequenceRoutine()
    {
        readyToShoot = false;
        allowReset = false;

        int shotsToFire = currentShootingMode == ShootingMode.Burst
            ? Mathf.Max(1, bulletsPerBurst)
            : 1;

        burstBulletsLeft = shotsToFire;

        for (int i = 0; i < shotsToFire; i++)
        {
            if (!CanPerformShot())
            {
                break;
            }

            SetAnimatorTrigger(animator, shootTriggerName);
            playerController?.TriggerHandsAnimation(handsShootTriggerName);

            if (fireDelay > 0f)
            {
                yield return new WaitForSeconds(fireDelay);
            }

            if (!CanPerformShot())
            {
                break;
            }

            PerformShot();
            burstBulletsLeft = Mathf.Max(0, burstBulletsLeft - 1);

            bool shouldContinueBurst = currentShootingMode == ShootingMode.Burst
                && i < shotsToFire - 1
                && bulletsLeft > 0;

            if (!shouldContinueBurst)
            {
                break;
            }

            float timeUntilNextTrigger = Mathf.Max(0f, shootingDelay - fireDelay);

            if (timeUntilNextTrigger > 0f)
            {
                yield return new WaitForSeconds(timeUntilNextTrigger);
            }
        }

        float resetDelay = Mathf.Max(0f, shootingDelay - fireDelay);

        if (resetDelay > 0f)
        {
            yield return new WaitForSeconds(resetDelay);
        }

        ResetShot();
        fireRoutine = null;
    }

    private bool CanStartFireSequence()
    {
        return isActiveWeapon
            && readyToShoot
            && !isReloading
            && bulletsLeft > 0
            && bulletSpawn != null;
    }

    private bool CanPerformShot()
    {
        return isActiveWeapon
            && !isReloading
            && bulletsLeft > 0
            && bulletSpawn != null;
    }

    private void PerformShot()
    {
        bulletsLeft--;

        if (weaponModel == WeaponModel.Lanzapatatas || weaponModel == WeaponModel.CañonSalado)
        {
            DrainPotatos();
        }

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        ApplyRaycastDamage(shootingDirection);
        SpawnVisualBullet(shootingDirection);
    }

    private void ApplyRaycastDamage(Vector3 shootingDirection)
    {
        if (bulletSpawn == null)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            bulletSpawn.position,
            shootingDirection,
            shotRange,
            shotMask,
            shotTriggerInteraction
        );

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreHit(hit.collider))
            {
                continue;
            }

            if (debugShotRay)
            {
                Debug.DrawLine(bulletSpawn.position, hit.point, Color.red, 1f);
            }

            BasicRat rat = hit.collider.GetComponentInParent<BasicRat>();

            if (rat != null)
            {
                bool isCritical = UnityEngine.Random.value < criticalChance;
                float finalDamage = isCritical ? weaponDamage * criticalMultiplier : weaponDamage;

                rat.TakeDamage(finalDamage, isCritical, hit.point);
                GivePointsForRatHit();
                return;
            }

            ShatterDestruction shatter = hit.collider.GetComponentInParent<ShatterDestruction>();

            if (shatter != null)
            {
                shatter.Shatter();
            }

            if (spawnRaycastImpact)
            {
                CreateRaycastImpactEffect(hit);
            }

            return;
        }

        if (debugShotRay)
        {
            Debug.DrawRay(bulletSpawn.position, shootingDirection * shotRange, Color.red, 1f);
        }
    }

    private void SpawnVisualBullet(Vector3 shootingDirection)
    {
        if (bulletPrefab == null || bulletSpawn == null)
        {
            return;
        }

        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            Quaternion.LookRotation(shootingDirection)
        );

        Bullet bulletDamage = bullet.GetComponent<Bullet>();

        if (bulletDamage != null)
        {
            bulletDamage.dmg = 0f;
            bulletDamage.criticalChance = 0f;
            bulletDamage.criticalMultiplier = 1f;
            bulletDamage.pointsGivenOnRatHit = 0;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        }

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));
    }

    private void GivePointsForRatHit()
    {
        if (pointsGivenOnRatHit <= 0)
        {
            return;
        }

        PlayerController player = playerController;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerController>();

                if (player == null)
                {
                    player = playerObject.GetComponentInParent<PlayerController>();
                }

                if (player == null)
                {
                    player = playerObject.GetComponentInChildren<PlayerController>();
                }
            }
        }

        if (player != null)
        {
            player.AddPoints(pointsGivenOnRatHit);
        }
    }

    private void CreateRaycastImpactEffect(RaycastHit hit)
    {
        if (GlobalReferences.Instance == null || GlobalReferences.Instance.bulletImpactEffectPrefab == null)
        {
            return;
        }

        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffectPrefab,
            hit.point,
            Quaternion.LookRotation(hit.normal)
        );

        hole.transform.SetParent(hit.collider.transform);
    }

    private bool ShouldIgnoreHit(Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return true;
        }

        if (hitCollider.transform.IsChildOf(transform))
        {
            return true;
        }

        if (playerController != null && hitCollider.transform.IsChildOf(playerController.transform))
        {
            return true;
        }

        return false;
    }

    private void Reload()
    {
        if (!readyToShoot || isReloading)
        {
            return;
        }

        isReloading = true;
        SoundManager.Instance.PlayReloadSound(weaponModel);

        ShowBucket();
        SetAnimatorTrigger(animator, reloadTriggerName);
        playerController?.TriggerHandsAnimation(handsReloadTriggerName);

        if (weaponModel == WeaponModel.Lanzapatatas || weaponModel == WeaponModel.CañonSalado)
        {
            ReloadPotatos();
        }

        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        bulletsLeft = magazineSize;
        isReloading = false;
        HideBucket();
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    // potatoLevel
    private void DrainPotatos()
    {
        if (potatoAmmo == null) return;

        float progress = 1f - ((float)bulletsLeft / (float)magazineSize);

        animator.Play("DecreasePotato", 2, progress);
    }

    private void ReloadPotatos()
    {
        if (potatoAmmo == null) return;

        if (reloadRoutine != null)
            StopCoroutine(reloadRoutine);

        reloadRoutine = StartCoroutine(ReloadPotatoesRoutine());
    }

    private IEnumerator ReloadPotatoesRoutine()
    {
        float startProgress = 1f - ((float)bulletsLeft / magazineSize);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / reloadTime;

            float progress = Mathf.Lerp(startProgress, 0f, t);
            animator.Play("DecreasePotato", 2, progress);

            yield return null;
        }

        bulletsLeft = magazineSize;
        DrainPotatos();
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null || bulletSpawn == null)
        {
            return transform.forward;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, shotRange, shotMask, shotTriggerInteraction);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Vector3 targetPoint = ray.GetPoint(shotRange);

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreHit(hit.collider))
            {
                continue;
            }

            targetPoint = hit.point;
            break;
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        Vector3 spread = mainCamera.transform.right * x + mainCamera.transform.up * y;

        return direction + spread;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bullet != null)
        {
            Destroy(bullet);
        }
    }

    public void ShowBucket()
    {
        SetBucketVisible(true);
    }

    public void HideBucket()
    {
        SetBucketVisible(false);
    }

    private void SetBucketVisible(bool visible)
    {
        if (bucketRenderers == null)
        {
            return;
        }

        foreach (Renderer bucketRenderer in bucketRenderers)
        {
            if (bucketRenderer != null)
            {
                bucketRenderer.enabled = visible;
            }
        }
    }

    public void SpawnFireSmokeParticles()
    {
        SpawnSmokePrefabAtTransform(smokePrefab, smokeSpawnPoint);
    }

    public void SpawnReloadPotatoParticles()
    {
        ParticleSystem particleSystem = potatosParticleSystem;

        if (particleSystem == null)
        {
            particleSystem = GetComponentInChildren<ParticleSystem>();
        }

        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    public void SpawnMuzzleFlash()
    {
        SpawnSmokePrefabAtTransform(muzzleEffect, bulletSpawn);
    }

    public void PlayShootSound()
    {
        SoundManager.Instance.PlayShootingSound(weaponModel);
        GameFeelManager.Instance.PlayShotFeedback();
    }

    public void PlayPotatoSound()
    {
        if (_source == null || potatoRefillSound == null)
        {
            return;
        }

        _source.PlayOneShot(potatoRefillSound);
    }

    public void PlayWeaponMoveSound()
    {
        if (_source == null || moveWeaponSounds == null || moveWeaponSounds.Count == 0)
        {
            return;
        }

        int rnd = UnityEngine.Random.Range(0, moveWeaponSounds.Count);
        _source.PlayOneShot(moveWeaponSounds[rnd]);
    }

    public void PlayPressureSound()
    {
        if (_source == null || pressureSound == null)
        {
            return;
        }

        _source.PlayOneShot(pressureSound);
    }

    private void SpawnSmokePrefabAtTransform(GameObject smokePrefab, Transform spawnPoint)
    {
        if (smokePrefab == null || spawnPoint == null)
        {
            return;
        }

        GameObject particleInstance = Instantiate(smokePrefab, spawnPoint.position, spawnPoint.rotation);
        ParticleSystem particleSystem = particleInstance.GetComponentInChildren<ParticleSystem>();

        if (particleSystem == null)
        {
            Destroy(particleInstance, 5f);
            return;
        }

        ParticleSystem.MainModule main = particleSystem.main;
        Destroy(particleInstance, main.duration + main.startLifetime.constantMax);
    }

    private void SetAnimatorTrigger(Animator targetAnimator, string triggerName)
    {
        if (targetAnimator == null || string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        targetAnimator.SetTrigger(triggerName);
    }
}
