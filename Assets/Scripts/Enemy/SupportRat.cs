using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class SupportRat : nBasicRatn
{
    WaveManager waveManager;

    private List<nBasicRatn> ratsToHeal;

    public float healingRange = 20f;
    public float healingAmount;
    public float healingProbability = 0.85f;

    public void Heal() //Cuando la rata realiza la animación de cura, busca una entre las cercanas, si la probabilidad está a favor de la que tenga menor vida, curará a esa, sino, escogerá una aleatoria
    {
        ratsToHeal.Clear();
        healingAmount = Random.Range(20, 40);

        float healing = Mathf.Clamp(healingAmount + ratsToHeal[0].health, healingAmount, ratsToHeal[0].initialHealth);

        OrderRats();

        if(RollDice(healingProbability) == true)
        {
            ratsToHeal[0].health = healing;
        }

        ratsToHeal[Random.Range(0, ratsToHeal.Count-1)].health = healing;
    }

    public void OrderRats()
    {
        foreach(var rat in waveManager.ratsPerWave) //Se buscan las ratas que tenga la support dentro de su rango de acción
        {
            if(Vector3.Distance(transform.position, rat.transform.position) <= actionRange)
            {
                ratsToHeal.Add(rat);
            }
        }
        ratsToHeal.OrderBy(r => r.health); //Se ordenan por su vida
    }
}
