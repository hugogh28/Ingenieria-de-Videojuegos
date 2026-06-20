using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private List<AudioClip> moveWeaponSounds;
    [SerializeField] private AudioClip potatoRefillSound;
    [SerializeField] private AudioClip pressureSound;
    private AudioSource _source;

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
                    foreach (Transform child3 in child2)
                    {
                        child3.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
                        foreach (Transform child4 in child3)
                        {
                            child4.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
                            foreach (Transform child5 in child4)
                            {
                                child5.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
                            }
                        }
                    }
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

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false)
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
        bulletsLeft--;

        SetAnimatorTrigger(animator, shootTriggerName);
        playerController?.TriggerHandsAnimation(handsShootTriggerName);

        if (weaponModel == WeaponModel.Lanzapatatas || weaponModel == WeaponModel.CañonSalado)
        {
            DrainPotatos();
        }

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        Bullet bul = bullet.GetComponent<Bullet>();
        bul.dmg = weaponDamage;
        bul.criticalChance = criticalChance;
        bul.criticalMultiplier = criticalMultiplier;
        bul.pointsGivenOnRatHit = pointsGivenOnRatHit;

        bullet.transform.forward = shootingDirection;

        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
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
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
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
        if (potatosParticleSystem == null) GetComponentInChildren<ParticleSystem>().Play();
        else potatosParticleSystem.Play();
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
        if (_source == null) return;
        _source.PlayOneShot(potatoRefillSound);
    }

    public void PlayWeaponMoveSound()
    {
        if (_source == null) return;
        int rnd = Random.Range(0, moveWeaponSounds.Count - 1);
        _source.PlayOneShot(moveWeaponSounds[rnd]);
    }

    public void PlayPressureSound()
    {
        if (_source == null) return;
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
