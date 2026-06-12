using System.Collections.Generic;
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

    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (WeaponManager.Instance == null || WeaponManager.Instance.activeWeaponSlot == null)
        {
            ClearWeaponUI();
            return;
        }

        Weapon activeWeapon = WeaponManager.Instance.activeWeaponSlot.GetComponentInChildren<Weapon>();

        if (activeWeapon != null)
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
            ClearWeaponUI();
        }
    }

    private void ClearWeaponUI()
    {
        magazineAmmoUI.text = "";
        totalAmmoUI.text = "";
        weaponNameUI.text = "";

        weaponIconUI.sprite = emptySlot;
        ammoTypeUI.sprite = emptySlot;
        ammoIconUI.sprite = emptySlot;
    }

    private string GetWeaponName(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return "Lanzapatatas";

            default:
                return "";
        }
    }

    private Sprite GetAmmoSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return LoadSpriteFromPrefab("LanzaPatatas_Ammo");

            default:
                return emptySlot;
        }
    }

    private Sprite GetWeaponSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return LoadSpriteFromPrefab("LanzaPatatas_Weapon");

            default:
                return emptySlot;
        }
    }

    private Sprite GetTypeSprite(Weapon.WeaponModel model)
    {
        switch (model)
        {
            case Weapon.WeaponModel.Lanzapatatas:
                return LoadSpriteFromPrefab("LanzaPatatas_Type");

            default:
                return emptySlot;
        }
    }

    private Sprite LoadSpriteFromPrefab(string resourceName)
    {
        if (spriteCache.TryGetValue(resourceName, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        GameObject prefab = Resources.Load<GameObject>(resourceName);

        if (prefab == null)
        {
            Debug.LogWarning($"No existe el prefab Resources/{resourceName}.", this);
            return emptySlot;
        }

        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"El prefab Resources/{resourceName} no tiene SpriteRenderer.", this);
            return emptySlot;
        }

        spriteCache[resourceName] = spriteRenderer.sprite;
        return spriteRenderer.sprite;
    }
}
