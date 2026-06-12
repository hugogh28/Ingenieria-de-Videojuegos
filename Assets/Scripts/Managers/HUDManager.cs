using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI;
    public TextMeshProUGUI totalAmmoUI;
    public Image ammoTypeUI;
    public Image ammoIconUI;

    [Header("Weapon")]
    public TextMeshProUGUI weaponNameUI;
    public Image weaponIconUI;

    [Header("Health")]
    public Image healthBar;

    [Header("Other")]
    public Sprite emptySlot;
    
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

    void Update()
    {
        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();

        if (activeWeapon)
        {
            magazineAmmoUI.text = $"{activeWeapon.bulletsLeft / activeWeapon.bulletsPerBurst}";
            totalAmmoUI.text = $"{activeWeapon.magazineSize / activeWeapon.bulletsPerBurst}";

            Weapon.WeaponModel model = activeWeapon.weaponModel;

            weaponIconUI.sprite = GetWeaponSprite(model);
            weaponNameUI.text = GetWeaponName(model);

            ammoIconUI.sprite = GetAmmoSprite(model);
            ammoTypeUI.sprite = GetTypeSprite(model);
        }
        else
        {
            magazineAmmoUI.text = "";
            totalAmmoUI.text = "";
            weaponNameUI.text = "";

            weaponIconUI.sprite = emptySlot;
            
            ammoTypeUI.sprite = emptySlot;
            ammoIconUI.sprite = emptySlot;
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
                return Resources.Load<GameObject>("LanzaPatatas_Ammo").GetComponent<SpriteRenderer>().sprite;
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

    private Sprite GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return Resources.Load<GameObject>("LanzaPatatas_Weapon").GetComponent<SpriteRenderer>().sprite;
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

    private Sprite GetTypeSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return Resources.Load<GameObject>("LanzaPatatas_Type").GetComponent<SpriteRenderer>().sprite;
            /*
            case Weapon.WeaponModel.CañonSalado:
                return Instantiate(Resources.Load<GameObject>("CañonSalado_Type")).GetComponent<SpriteRenderer>().sprite;

            case Weapon.WeaponModel.FuriaFrijol:
                return Instantiate(Resources.Load<GameObject>("FuriaFrijol_Type")).GetComponent<SpriteRenderer>().sprite;
            */
            default:
                return null;
        }
    }
}
