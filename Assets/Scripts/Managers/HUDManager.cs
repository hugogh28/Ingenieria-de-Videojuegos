using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;

    [Header("Weapon")]
    public Image activeWeaponUI;
    public Image unActiveWeaponUI;

    [Header("Flavour")]
    public Image flavourUI;

    [Header("Other")]
    public Sprite emptySlot;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponent<Weapon>();
        Weapon unActiveWeapon = GetUnActiveWeaponSlot().GetComponentInChildren<Weapon>();

        if (activeWeapon)
        {
            magazineAmmoUI.text = $"{activeWeapon.bulletsLeft / activeWeapon.bulletsPerBurst}";
            totalAmmoUI.text = $"{activeWeapon.magazineSize / activeWeapon.bulletsPerBurst}";

            Weapon.WeaponModel model = activeWeapon.weaponModel;
            ammoTypeUI.sprite = GetAmmoSprite(model);

            activeWeaponUI.sprite = GetWeaponSprite(model);

            if (unActiveWeapon)
            {
                unActiveWeaponUI.sprite = GetWeaponSprite(unActiveWeapon.weaponModel);
            }
        }
        else
        {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";

            ammoTypeUI.sprite = emptySlot;

            activeWeaponUI.sprite = emptySlot;
            unActiveWeaponUI.sprite = emptySlot;
        }
    }

    private string GetWeaponName(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return "Lanzapatatas";

            default:
                return null;
        }
    }


    private Sprite GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return Instantiate(Resources.Load<GameObject>("LanzaPatatas_Ammo")).GetComponent<SpriteRenderer>().sprite;
            /*
            case Weapon.WeaponModel.CañonSalado:
                return Instantiate(Resources.Load<GameObject>("CañonSalado_Ammo")).GetComponent<SpriteRenderer>().sprite;

            case Weapon.WeaponModel.FuriaFrijol:
                return Instantiate(Resources.Load<GameObject>("FuriaFrijol_Ammo")).GetComponent<SpriteRenderer>().sprite;
            */
            default:
                return null;
        }
    }

    private GameObject GetUnActiveWeaponSlot()
    {
        foreach (var weaponSlot in WeaponManager.Instance.weaponSlots)
        {
            if (weaponSlot != WeaponManager.Instance.activeWeaponSlot)
            {
                return weaponSlot;
            }
        }
        return null;
    }

    private Sprite GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return Instantiate(Resources.Load<GameObject>("LanzaPatatas_Weapon")).GetComponent<SpriteRenderer>().sprite;
            /*
            case Weapon.WeaponModel.CañonSalado:
                return Instantiate(Resources.Load<GameObject>("CañonSalado_Weapon")).GetComponent<SpriteRenderer>().sprite;

            case Weapon.WeaponModel.FuriaFrijol:
                return Instantiate(Resources.Load<GameObject>("FuriaFrijol_Weapon")).GetComponent<SpriteRenderer>().sprite;
            */
            default:
                return null;
        }
    }
}
