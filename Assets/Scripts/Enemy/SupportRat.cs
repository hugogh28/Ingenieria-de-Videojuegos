using NUnit.Framework;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class SupportRat : nBasicRatn
{
    /*
    public float healingRange = 20f;
    public float healingAmount;
    public float healingProbability = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cooldown = 5f;
    }

    public void Heal()
    {
        //Definir aquí que en base a una distancia, la rata support debe curar a una aliada suya
        if (RollDice(healingProbability) == true)
        {
            healingAmount = Random.Range(20, 40);
            animator.SetBool("isHealing", true);
            //Elige la rata con la vida más baja (pero con cierta probabilidad de escoger a otra)
            float random = Random.value;
            if (random <= 0.85)
            {
                nearRats[GetRat()].GetComponent<BasicRat>().health += healingAmount;
            }
            else //Con un 15% de probabilidad de elegir una rata random para curarla
            {
                nearRats[Random.Range(0, nearRats.Count())].GetComponent<BasicRat>().health += healingAmount;
            }
        }
    }

    public int GetRat()//Devuelve la rata con la menor vida
    {
        int index = 0;
        for (int i = 0; i < nearRats.Count(); i++) 
        {
            if (nearRats[i].GetComponent<BasicRat>().health < nearRats[index].GetComponent<BasicRat>().health) index = i;
        }
        return index;
    }

        public void AddRatsToList(float detectionRange)
    {
        foreach (var rat in rat)
        {
            distanceToOtherRat = Vector3.Distance(transform.position, rat.transform.position);
            if (distanceToOtherRat <= detectionRange && !nearRats.Contains(rat))
            {
                nearRats.Add(rat);
                Debug.Log(nearRats);
            }
        }
    }

    public void DeleteRatsFromList(float detectionRange)
    {
        foreach (var rat in nearRats)
        {
            if (distanceToOtherRat > detectionRange || rat == null || !rat.activeInHierarchy)
            {
                nearRats.Remove(rat);
                Debug.Log(nearRats);
            }
        }
    }
    
    // Update is called once per frame
    /*void Update()
    {
        AddRatsToList(20f); //En una distancia de veinte unidades las ratas pueden interactuar entre ellas
        DeleteRatsFromList(20f);
    }*/
}
