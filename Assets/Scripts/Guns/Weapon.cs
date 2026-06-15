using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;

    [Header("Gun Properties")]
    public float weaponDamage = 20;
    [Range(0f, 1f)] public float criticalChance = 0.15f;
    [Min(1f)] public float criticalMultiplier = 1.75f;
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

    [Header("Reload")]
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    
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

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();

        bulletsLeft = magazineSize;

        if (potatoAmmo != null)
        {
            maxAmmoLevel = potatoAmmo.localPosition.y;
            minAmmoLevel = maxAmmoLevel - 0.05f;

            currentPotatoLevel = maxAmmoLevel;
            drainPerBullet = (maxAmmoLevel - minAmmoLevel) / magazineSize;
        }
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

            GetComponent<Outline>().enabled = false;

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

    private void FireWeapon()
    {
        bulletsLeft--;
        
        muzzleEffect.GetComponent<ParticleSystem>().Play();

        animator.SetTrigger("recoil");

        if (weaponModel == WeaponModel.Lanzapatatas || weaponModel == WeaponModel.CañonSalado)
        {
            DrainPotatos();
        }

        SoundManager.Instance.PlayShootingSound(weaponModel);
        GameFeelManager.Instance.PlayShotFeedback();

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        Bullet bul = bullet.GetComponent<Bullet>();
        bul.dmg = weaponDamage;
        bul.criticalChance = criticalChance;
        bul.criticalMultiplier = criticalMultiplier;

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

        animator.SetTrigger("reload");

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
}
