using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;
using System.ComponentModel.Design;
using System.Collections;

public class BasicRat : MonoBehaviour, IPoolableObject, IHealth
{
    [SerializeField] public float initialHealth; //Vida con la que inicia la rata
    [HideInInspector] public Animator animator; //Animator de la rata
    [HideInInspector] public NavMeshAgent navAgent; //NavMeshAgent de la rata
    public float actionRange = 10f; //Rango de ataque de la rata
    //public float timer;

    [HideInInspector] public GameObject player; //Habría que revisar por si fuese posible aplicar solo el transform del jugador
    public float detectionRange = 15f; //Rango (en units) de detección de la rata
    public float delay = 1f; //Define el delay entre una acción y la siguiente, como bien puede ser curar, atacar o recargar
    [HideInInspector] public bool doneSomething = false; //Define si la rata ha efectuado una acción para aplicar delay
    
    [HideInInspector] public float distanceToPlayer; //Distancia al jugador
    [HideInInspector] public float distanceToOtherRat;

    [SerializeField] public float attackDamage = 5f; //Daño base que efectúa una rata al jugador
    [SerializeField] public float criticProbability = 0.25f; //Probabilidad de efectuar daño crítico al jugador

    public int pointsGivenAtDeath; //Los puntos que otorga una rata en su muerte

    public string actionNextToPlayer; //El nombre de la animación de la rata que debe efectuar al estar en un rango de ataque

    [HideInInspector] public WaveManager waveManager;

    public float health
    {
        get;
        set;
    }

    public bool Active 
    { 
        get; 
        set; 
    }



    private void Awake()
    {
        health = initialHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        //timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        waveManager = FindFirstObjectByType<WaveManager>();
    }
    private void OnEnable()
    {
        Awake();
    }

    public void DetectPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.GetComponentInParent<Transform>().position);
        if (distanceToPlayer <= detectionRange)
        {
            SmoothLookAt(player.transform);
            navAgent.SetDestination(player.GetComponent<Transform>().position);
        }
        /*else if (distanceToPlayer > detectionRange)
        {
            //Agregar aquí una serie de destinos que hagan a la rata divagar por la zona
        }*/
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
        if (distanceToPlayer <= actionRange)
        {
            return true;
        }
        else return false;
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

    public IEnumerator HasDoneSomething()
    {
        doneSomething = true;
        yield return new WaitForSeconds(delay);
        doneSomething = false;
    }

    public void Update()
    {
        try
        {
            ShouldStop();
            DetectPlayer();
            if (navAgent.velocity.magnitude > 0.1f)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                //animator.SetBool("isWalking", false); //Habrá que sustituir aquí por la animación idle
            }

            if (/*navAgent.isOnNavMesh && */ShouldStop() == true && doneSomething != true)
            {
                navAgent.isStopped = true;
                animator.SetBool(actionNextToPlayer, true);
            }
            else /*if(navAgent.isOnNavMesh)*/
            {
                navAgent.isStopped = false;
            }
        }catch(Exception e)
        {
            Debug.LogError("Error en Update de " + gameObject.name + ": " + e.Message);
        }
    }

    //Para testeo
    private void OnMouseDown()
    {
        health -= 50;
        if (health <= 0) Die();
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

    public void ResetObject()
    {
        //this.Active = false;
        this.gameObject.SetActive(false);
        this.health = initialHealth;
    }

    public IPoolableObject Clone()
    {
        return Instantiate(this);
    }

    public void Die()
    {
        float points = UnityEngine.Random.Range(pointsGivenAtDeath, pointsGivenAtDeath * 1.5f); //Se añade un randomizador de puntos
        player.GetComponent<PlayerController>().points += (int)points;

        waveManager.NotifyRatDied(this);

        Debug.Log("Die() llamado en " + gameObject.name);
        //animator.SetTrigger("die");
        ResetObject();
    }
}
