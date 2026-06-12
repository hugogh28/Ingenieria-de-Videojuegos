using UnityEngine;

public class ShopHealth : ShopItem, IUnlockableObject
{
    public float givenHealth;

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

        player.Heal(givenHealth);
    }
}
