using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CommonRat : BasicRat
{
    public void Melee()
    {
        float dmg = data.AttackDamage;
        if (RollDice(data.CriticProbability) == true)
        {
            float criticImpact = UnityEngine.Random.Range(1f, 2f);
            dmg = data.AttackDamage * criticImpact;//Añadir indicador de crítico
        }
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().TakeDamage(dmg);
    }
}
