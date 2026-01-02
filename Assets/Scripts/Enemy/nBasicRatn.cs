using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class nBasicRatn : MonoBehaviour
{
    [SerializeField] public float health = 100;
    public Animator animator;
    public NavMeshAgent navAgent;
    //private WaveSpawner waveSpawner;
    public float attackRange = 10f;
    public float timer;

    //#region IEnemyProtoype
    public Transform player;
    //private nBasicRatn rat;
    private GameObject[] rat;
    public float detectionRange = 15f;
    public float delay = 1f; //Define el delay entre una acción y la siguiente, como bien puede ser curar, atacar o recargar
    public List<GameObject> nearRats;
    //public float actionProbability = 0.5f; 
    //public float shootingRange = 7f;
    //public float fireRate = 1f;
    // #endregion


    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToOtherRat;

    private void Start()
    {
        nearRats.Clear();
        timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        rat = GameObject.FindGameObjectsWithTag("Rat");
    }

    public void DetectPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            navAgent.SetDestination(player.position);
        }else if(distanceToPlayer > detectionRange)
        {
            //Agregar aquí una serie de destinos que hagan a la rata divagar por la zona
        }
    }

    public void AddRatsToList(float detectionRange)
    {
        foreach (var rat in rat) 
        {
            distanceToOtherRat = Vector3.Distance(transform.position, rat.transform.position);
            if (distanceToOtherRat <= detectionRange && distanceToOtherRat > 0)
            {
                nearRats.Add(rat);
                Debug.Log(nearRats);
            }
        }
    }

    public void DeleteRatsFromList(float detectionRange)
    {
        foreach(var rat in nearRats)
        {
            if(distanceToOtherRat > detectionRange || rat == null || !rat.activeInHierarchy)
            {
                nearRats.Remove(rat);
                Debug.Log(nearRats);
            }
        }
    }
    public void TakeDamage(float dmg)
    {
        /*else
        {
            navAgent.isStopped = false;
        }*/
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

    public bool ShouldStop()
    {
        if (distanceToPlayer <= attackRange)
        {
            //navAgent.isStopped = true;
            return true;
        }
        else return false;//navAgent.isStopped = false;
    }

    public bool RollDice(float actionProbability)
    {
        if (timer % 5 == 0)//Cada vez que el timer pase por múltiplos exactos de 5 se comprobará si las ratas pueden hacer una acción especial
        {
            float random = Random.value;
            if (random <= actionProbability) return true;
            else return false;
        }
        return false;
    }

    public void Update() 
    {
        timer += Time.deltaTime;
        if (navAgent.velocity.magnitude > 0.1f) 
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        if (ShouldStop() == true) navAgent.isStopped = true;
        else navAgent.isStopped = false;
        //<< Interface >> 
        AddRatsToList(20f); //En una distancia de veinte unidades las ratas pueden interactuar entre ellas
        DeleteRatsFromList(20f);
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

    public void Create(nBasicRatn rat)
    {
        Instantiate(rat);
    }

    public void SetWaveSpawner(WaveSpawner spawner)
    {
        //Asignar aquí el spawner de la rata en función de la zona del jugador
    }
}