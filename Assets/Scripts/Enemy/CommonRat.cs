using System;
using UnityEngine;
using UnityEngine.AI;

public class CommonRat : BasicRat
{
    private void Start()
    {
        timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        transform = GetComponent<Transform>();
    }

    public void Melee()
    {
        if (distanceToPlayer <= attackRange)
        {
            animator.SetTrigger("attack");
            Debug.Log("meremato");
            if (RollDice(criticProbability) == true)
            {
                float criticImpact = UnityEngine.Random.Range(1f, 2f);
                attackDamage *= criticImpact;//Añadir indicador de crítico
            }
            else attackDamage = 5f;
            //Aplicar daño al jugador
        }
    }

    // Update is called once per frame
    public void Update()
    {
        timer += Time.deltaTime;

        if (navAgent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isWalking", true);
            Debug.Log("aaaaaaa");
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
