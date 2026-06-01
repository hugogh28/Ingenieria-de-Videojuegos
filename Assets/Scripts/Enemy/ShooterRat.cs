using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ShooterRat : nBasicRatn
{
    private bool hasFired = false;

    private void Start()
    {
        timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void Shoot()
    {
        if(RollDice(criticProbability) == true)
        {
            float criticImpact = Random.Range(1f, 1.5f);
            float dmg = attackDamage * criticImpact;
            //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(dmg);
            //Trazar rayo desde el arma de la rata hacia el jugador para que este pueda recibir daño
        }
        else
        {
            attackDamage = 20f;
            //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(attackDamage);
            //Trazar rayo desde el arma de la rata hacia el jugador para que este pueda recibir daño
        }
    }
}
