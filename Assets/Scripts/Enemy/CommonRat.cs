using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CommonRat : nBasicRatn
{
    private void Start()
    {
        timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void Melee()
    {
        float dmg = attackDamage;
        if (RollDice(criticProbability) == true)
        {
            float criticImpact = UnityEngine.Random.Range(1f, 2f);
            dmg = attackDamage * criticImpact;//Añadir indicador de crítico
        }
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(dmg);
    }
}
