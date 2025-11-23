using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{    
    public bool isShooting, readyToShoot, next;
    bool allowReset = true;
    public float shootingDelay = 2f;

    public int bulletsPerBurst = 1;
    public int burstBulletsLeft;

    public float spreadIntensity;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;

    public GameObject muzzleEffect;
    public Animator animator;

    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

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

    public ShootingMode currentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();

        bulletsLeft = magazineSize;

        SoundManager.Instance.fryingSoundPatata.Play();
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
