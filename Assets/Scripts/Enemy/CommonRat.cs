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
        if (RollDice(criticProbability) == true)
        {
            float criticImpact = UnityEngine.Random.Range(1f, 2f);
            int dmg = (int)(attackDamage * criticImpact);//Añadir indicador de crítico
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(dmg);
            //Para mayor feedback, se puede añadir un temblor a la cámara del jugador
            Debug.Log($"El jugador ha recibido {dmg} puntos de daño");
        }
        else
        {
            attackDamage = 5f;
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(attackDamage);
            //Para mayor feedback, se puede añadir un temblor a la cámara del jugador
            Debug.Log($"El jugador ha recibido {attackDamage} puntos de daño");
        }
    }
}
