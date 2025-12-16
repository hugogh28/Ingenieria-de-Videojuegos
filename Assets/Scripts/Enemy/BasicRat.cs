using UnityEngine;
using UnityEngine.AI;

public class BasicRat : MonoBehaviour
{
    [SerializeField] private float health = 100;
    private Animator animator;
    private NavMeshAgent navAgent;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

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

    public void Update() 
    {
        if (navAgent.velocity.magnitude > 0.1f) 
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}