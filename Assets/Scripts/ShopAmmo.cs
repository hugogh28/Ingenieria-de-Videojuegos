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
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points >= pointsNeeded)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points -= pointsNeeded;
            //Se indica que la nueva munición debe ser como máximo, la capacidad máxima
            int ammo = Mathf.Clamp(givenAmmo + WeaponManager.Instance.activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>().bulletsLeft, givenAmmo, WeaponManager.Instance.activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>().magazineSize);
            WeaponManager.Instance.activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>().bulletsLeft = ammo;
            //Si es un objeto que debe desaparecer se usará gameObject.SetActive(false);
        }
    }
}
