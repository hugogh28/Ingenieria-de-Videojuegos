using UnityEngine;

public class ShopAmmo : ShopItem, IUnlockableObject
{
    public int givenAmmo;

    public void OnMouseDown()
    {
        Unlock();
    }

    public void Unlock()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null || !playerObject.TryGetComponent(out PlayerController player))
        {
            return;
        }

        if (!player.TrySpendPoints(pointsNeeded))
        {
            return;
        }

        if (WeaponManager.Instance == null || WeaponManager.Instance.activeWeaponSlot == null)
        {
            return;
        }

        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();

        if (activeWeapon == null)
        {
            return;
        }

        // Se indica que la nueva munición debe ser como máximo la capacidad máxima.
        int ammo = Mathf.Clamp(
            givenAmmo + activeWeapon.bulletsLeft,
            givenAmmo,
            activeWeapon.magazineSize
        );

        activeWeapon.bulletsLeft = ammo;
    }
}
