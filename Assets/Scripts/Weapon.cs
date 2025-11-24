using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Gun Properties")]
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
    public Animator animator;

    [Header("Reloads")]
    public float reloadTime;
    public int magazineSize, bulletsLeft;

    [Header("Lanza Patatas")]
    public Transform potatoAmmo;
    float minAmmoLevel, maxAmmoLevel, currentPotatoLevel, drainPerBullet;
    private Coroutine reloadRoutine;

    [Header("Debug")]
    public bool isShooting;
    public bool readyToShoot, next;
    bool allowReset = true;
    public bool isReloading;
    public int burstBulletsLeft;

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

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }
    [Header("Weapon Type")]
    public ShootingMode currentShootingMode;

    public WeaponModel weaponModel;

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
        // SoundManager.Instance.fryingSoundPatata.Play();
    }

    // Update is called once per frame
    void Update()
    {
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

        if (AmmoManager.Instance.ammoDisplay != null)
        {
            AmmoManager.Instance.ammoDisplay.text = $"{bulletsLeft / bulletsPerBurst}/{magazineSize / bulletsPerBurst}";
        }
    }

    private void FireWeapon()
    {
        bulletsLeft--;
        
        muzzleEffect.GetComponent<ParticleSystem>().Play();

        animator.SetTrigger("recoil");

        DrainPotatos();

        if (next)
        {
            SoundManager.Instance.shootingSoundsLanzapatatas[0].Play();
        }
        else
        {
            SoundManager.Instance.shootingSoundsLanzapatatas[1].Play();
        }
        next = !next;

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

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
        SoundManager.Instance.reloadSoundLanzapatatas.Play();
        animator.SetTrigger("reload");

        ReloadPotatos();

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

        // Si ya había una corrutina de recarga, la paramos
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
