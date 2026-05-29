using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class nBasicRatn : MonoBehaviour, IPoolableObject
{
    [SerializeField] public float health; //Quizá sea necesario asignárselo en el start (o aquí mismo)
    [SerializeField] private float initialHealth;
    public Animator animator;
    public NavMeshAgent navAgent;
    public float attackRange = 10f;
    public float timer;

    [HideInInspector] public GameObject player; //Habría que revisar por si fuese posible aplicar solo el transform del jugador
    private GameObject[] rat;
    public float detectionRange = 15f;
    public float delay = 1f; //Define el delay entre una acción y la siguiente, como bien puede ser curar, atacar o recargar
    public List<GameObject> nearRats;
    
    SpawnerManager spawnerManager;

    public Transform position;

    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToOtherRat;

    public bool Active 
    { 
        get; 
        set; 
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        this.Active = false;
        initialHealth = health;
        nearRats.Clear();
        timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void DetectPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, /*spawnerManager.playerPosition*/ player.GetComponentInParent<Transform>().position);
        if (distanceToPlayer <= detectionRange)
        {
            navAgent.SetDestination(/*spawnerManager.playerPosition*/ player.GetComponent<Transform>().position);
        }
        else if (distanceToPlayer > detectionRange)
        {
            //Agregar aquí una serie de destinos que hagan a la rata divagar por la zona
        }
    }


    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            
            Die();
        }
        else
        {
            //animator.SetTrigger("damage");
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
        if (UnityEngine.Random.value < actionProbability) //Si el valor obtenido en Random.value es menor que la probabilidad dada, se cumplirá la condición
        {
            return true;
        }
        else
        {
            return false;
        }   
    }

    public void Update()
    {
        try
        {
            if (navAgent.velocity.magnitude > 0.1f)
            {
                //animator.SetBool("isWalking", true);
            }
            else
            {
                //animator.SetBool("isWalking", false); //Habrá que sustituir aquí por la animación idle
            }

            if (health <= 0)
            {
                Die();
                return;
            }

            if (ShouldStop() == true)
            {
                navAgent.isStopped = true;
            }
            else
            {
                navAgent.isStopped = false;
            }
        }catch(Exception e)
        {
            Debug.LogError("Error en Update de " + gameObject.name + ": " + e.Message);
        }
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

    public void ResetObject()
    {
        this.Active = false;
        this.gameObject.SetActive(false);
        this.health = initialHealth;
    }

    public IPoolableObject Clone()
    {
        return Instantiate(this);
    }

    public void Die()
    {
        Debug.Log("Die() llamado en " + gameObject.name);
        //animator.SetTrigger("die");
        ResetObject();
    }
}
