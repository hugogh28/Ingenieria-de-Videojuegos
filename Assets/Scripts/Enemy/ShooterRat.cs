using UnityEngine;

public class ShooterRat : nBasicRatn
{
    public float shootinRange = 12f;
    public float bulletDamage = 20f;
    public float fireRate = 1f;
    public int ammo = 20;
    public float critic = 0.05f;

    //Podría ser interesante hacer un ataque especial, o que se meta más daño por un crítico a la rata

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        attackRange = shootinRange;
    }

    public void Shoot()
    {
        if (ShouldStop()==true)
        {
            animator.SetBool("isShooting", true);
            if (RollDice(critic) == true)//Hay cierta probabilidad de recibir un daño crítico
            {
                float criticImpact = Random.Range(1f, 1.5f);
                bulletDamage *= criticImpact;
            }
            else bulletDamage = 20f;
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
