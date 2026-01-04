using NUnit.Framework;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class SupportRat : nBasicRatn
{
    [SerializeField] SupportRat supportRatTank;
    [SerializeField] SupportRat supportRatNormal;

    public float healingRange = 20f;
    public float healingAmount;
    public float healingProbability = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        delay = 5f;
    }

    public SupportRat Clone(bool tank)
    {
        SupportRat rat = tank == true ? Instantiate(supportRatTank) : Instantiate(supportRatNormal); //Si requerimos de utilizar el patrón Prototype para crear una rata tanque, tan solo debemos habilitar al clonar tank = true
        return rat;
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
                nearRats[GetRat()].GetComponent<nBasicRatn>().health += healingAmount;
            }
            else //Con un 15% de probabilidad de elegir una rata random para curarla
            {
                nearRats[Random.Range(0, nearRats.Count() - 1)].GetComponent<nBasicRatn>().health += healingAmount;
            }
        }
    }

    public int GetRat()//Devuelve la rata con la menor vida
    {
        int index = 0;
        for (int i = 0; i < nearRats.Count(); i++) 
        {
            if (nearRats[i].GetComponent<nBasicRatn>().health < nearRats[index].GetComponent<nBasicRatn>().health) index = i;
        }
        return index;
    }
    
    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
