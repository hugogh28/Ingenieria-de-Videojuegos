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
    [Header("Rat Type")]
    [SerializeField] private RatType type;
    [SerializeField] private RatSubType subType;

    public RatData data;

    //[SerializeField] public float initialHealth; //Vida con la que inicia la rata
    [HideInInspector] public Animator animator; //Animator de la rata
    [HideInInspector] public NavMeshAgent navAgent; //NavMeshAgent de la rata
    //public float actionRange = 10f; //Rango de ataque de la rata
    //public float timer;

    [HideInInspector] public GameObject player; //Habría que revisar por si fuese posible aplicar solo el transform del jugador
    //public float detectionRange = 15f; //Rango (en units) de detección de la rata
    //public float delay = 1f; //Define el delay entre una acción y la siguiente, como bien puede ser curar, atacar o recargar
    [HideInInspector] public bool doneSomething = false; //Define si la rata ha efectuado una acción para aplicar delay
    
    [HideInInspector] public float distanceToPlayer; //Distancia al jugador
    //[HideInInspector] public float distanceToOtherRat;

    //[SerializeField] public float attackDamage = 5f; //Daño base que efectúa una rata al jugador
    //[SerializeField] public float criticProbability = 0.25f; //Probabilidad de efectuar daño crítico al jugador

    //public int pointsGivenAtDeath; //Los puntos que otorga una rata en su muerte

    //public string actionNextToPlayer; //El nombre de la animación de la rata que debe efectuar al estar en un rango de ataque

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
        data = RatDataFactory.GetRatData(type, subType);

        health = data.InitialHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        //timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        waveManager = FindFirstObjectByType<WaveManager>();
    }
    private void OnEnable()
    {
        Awake();

        if (data == null)
        {
            data = RatDataFactory.GetRatData(type, subType);
        }

        health = data.InitialHealth;
        doneSomething = false;
    }

    public void DetectPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.GetComponentInParent<Transform>().position);
        if (distanceToPlayer <= data.DetectionRange)
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
        if (distanceToPlayer <= data.ActionRange)
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
        yield return new WaitForSeconds(data.Delay);
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
                //animator.SetBool("idle", false); //Habrá que sustituir aquí por la animación idle
            }

            if (ShouldStop() == true && doneSomething != true)
            {
                navAgent.isStopped = true;
                animator.SetBool(data.ActionNextToPlayer, true);
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
        this.health = data.InitialHealth;
    }

    public IPoolableObject Clone()
    {
        return Instantiate(this);
    }

    public void Die()
    {
        float points = UnityEngine.Random.Range(data.PointsGivenAtDeath, data.PointsGivenAtDeath * 1.5f); //Se añade un randomizador de puntos
        player.GetComponent<PlayerController>().points += (int)points;

        waveManager.NotifyRatDied(this);

        Debug.Log("Die() llamado en " + gameObject.name);
        //animator.SetTrigger("die");
        ResetObject();
    }
}
