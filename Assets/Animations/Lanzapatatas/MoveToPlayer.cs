using UnityEngine;
using UnityEngine.AI;

public class MoveToPlayer : MonoBehaviour
{
    private NavMeshAgent navAgent;

    private Transform transform;
    private float distanceToPlayer;
    private float attackRange;
    
    public float detectionRange;
    public Transform playerPosition;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        transform = navAgent.transform;
        attackRange = GetComponent<BasicRat>().data.ActionRange;
    }

    void Update()
    {
        if (ShouldStop()) return;

        distanceToPlayer = Vector3.Distance(transform.position, playerPosition.position);
        if (distanceToPlayer <= detectionRange)
        {
            navAgent.SetDestination(playerPosition.position);
        }
        else if (distanceToPlayer > detectionRange)
        {
            //Agregar aquí una serie de destinos que hagan a la rata divagar por la zona
        }
    }

    public bool ShouldStop()
    {
        if (distanceToPlayer <= attackRange)
        {
            //navAgent.isStopped = true;
            return true;
        }
        else return false;//navAgent.isStopped = false; 
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