using UnityEngine;

public class ShopHealth : ShopItem, IUnlockableObject
{
    public float givenHealth; // Vida que proporciona el objeto comprable

    public void OnMouseDown()
    {
        Unlock();
    }

    public void Unlock()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        if (player.points >= pointsNeeded)
        {
            player.points -= pointsNeeded;
            player.Heal(givenHealth);
            // Si es un objeto que debe desaparecer, se usará gameObject.SetActive(false);
        }
    }
}
