using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class BasicRat : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public NavMeshAgent navAgent;
    
    [SerializeField] public float health = 100;
    [SerializeField] public float value = 10;
    public float attackRange = 10f;
    public float cooldown = 1f;
    public float attackDamage = 5f;
    public float criticProbability = 0.05f;

    [HideInInspector] public float timer;
    [HideInInspector] public Transform transform;
    [HideInInspector] public float distanceToPlayer;

    public void TakeDamage(float dmg)
    {
        // provisional
        health -= dmg;// ReactionManager.Instance.calcDamage(dmg);

        if (health <= 0)
        {
            animator.SetTrigger("die");
            Destroy(gameObject);
        }
        else
        {
            animator.SetTrigger("damage");
        }
    }

    public bool RollDice(float actionProbability)
    {
        if (timer % 5 == 0)//Cada vez que el timer pase por múltiplos exactos de 5 se comprobará si las ratas pueden hacer una acción especial (queda sujeto a revisión)
        {
            float random = Random.value;
            if (random <= actionProbability) return true;
            else return false;
        }
        return false;
    }
}