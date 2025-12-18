using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class nBasicRatn : MonoBehaviour
{
    [SerializeField] private float health = 100;
    private Animator animator;
    private NavMeshAgent navAgent;
    private WaveSpawner waveSpawner;

    public Transform player;
    public float detectionRange = 15f;
    public float shootingRange = 7f;
    public float fireRate = 1f;

    

    [HideInInspector] public float distanceToPlayer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(float dmg)
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if(distanceToPlayer <= detectionRange )
        {
            navAgent.SetDestination(player.position);
        }

        /*if(distanceToPlayer <= shootingRange )
        {

        }*/
        else
        {
            navAgent.isStopped = false;
        }
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
        << Interface >> 
    }
    public void SmoothLookAt(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }


}