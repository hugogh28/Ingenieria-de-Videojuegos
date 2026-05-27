using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class nBasicRatn : MonoBehaviour, IPoolableObject
{
    [SerializeField] public float initialHealth; //Quizá sea necesario asignárselo en el start (o aquí mismo)
    private float health;
    public Animator animator;
    public NavMeshAgent navAgent;
    //private WaveSpawner waveSpawner;
    public float attackRange = 10f;
    public float timer;

    //#region IEnemyProtoype
    //GameObject p;
    //public Transform player;
    //private nBasicRatn rat;
    private GameObject[] rat;
    public float detectionRange = 15f;
    public float delay = 1f; //Define el delay entre una acción y la siguiente, como bien puede ser curar, atacar o recargar
    public List<GameObject> nearRats;
    //public float actionProbability = 0.5f; 
    //public float shootingRange = 7f;
    //public float fireRate = 1f;
    // #endregion
    SpawnerManager spawnerManager;

    public Transform position;

    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToOtherRat;

    public bool Active 
    { 
        get; 
        set; 
    }

    private void Start()
    {
        this.Active = false;
        health = initialHealth;
        nearRats.Clear();
        timer = 0;
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    public void DetectPlayer()
    {
        //return WaitForSeconds()
        distanceToPlayer = Vector3.Distance(transform.position, spawnerManager.playerPosition);
        if (distanceToPlayer <= detectionRange)
        {
            navAgent.SetDestination(spawnerManager.playerPosition);
        }
        else if (distanceToPlayer > detectionRange)
        {
            //Agregar aquí una serie de destinos que hagan a la rata divagar por la zona
        }
    }


    public void TakeDamage(float dmg)
    {
        health -= dmg;// ReactionManager.Instance.calcDamage(dmg);

        if (health <= 0)
        {
            
            Die();
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

    public bool RollDice(float actionProbability) //Hay que reestructurar esto, para evitar gasto computacional por timers en las ratas
    {
        if (timer % 5 == 0)//Cada vez que el timer pase por múltiplos exactos de 5 se comprobará si las ratas pueden hacer una acción especial (queda sujeto a revisión)
        {
            float random = Random.value;
            if (random <= actionProbability) return true;
            else return false;
        }
        return false;
    }

    public void Update()
    {
        rat = GameObject.FindGameObjectsWithTag("Rat"); //Cambiar al Start(), esto es un gasto computacional innecesariamente grande
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

    public void SetWaveSpawner(SpawnerManager spawner)
    {
        //Asignar aquí el spawner de la rata en función de la zona del jugador
    }

    public void ResetObject()
    {
        this.Active = false;
        this.GetComponent<GameObject>().SetActive(false);
        this.health = initialHealth;
    }

    public IPoolableObject Clone()
    {
        return Instantiate(this); //Revisa, puede ser que de problemas
    }

    public void Die()
    {
        animator.SetTrigger("die");
        ResetObject();
    }
}
