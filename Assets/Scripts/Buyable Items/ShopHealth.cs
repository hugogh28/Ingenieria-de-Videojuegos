using UnityEngine;

public class ShopHealth : ShopItem, IUnlockableObject
{
    public float givenHealth; //Vida que proporciona el objeto comprable

    public void OnMouseDown()
    {
        Unlock();
    }

    public void Unlock()
    {
        if(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points >= pointsNeeded)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points -= pointsNeeded;
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().health += givenHealth;
            //Si es un objeto que debe desaparecer, se usará gameObject.SetActive(false);
        }
    }
}
