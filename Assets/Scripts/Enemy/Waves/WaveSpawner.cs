
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;
    //[SerializeField] private GameObject spawnPoint;
    //[SerializeField] private float spawnRadius = 3f;

    public int currentWaveIndex = 0;

    public float spawnProbability;

    public List<GameObject> spawners;
    public List<nBasicRatn> ratsPerWave;

    GameObject player;

    GameObject[] allSpawners;

    private bool readyToCountDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        readyToCountDown = true;
        spawnProbability = 1f/spawners.Count;

        allSpawners = GameObject.FindGameObjectsWithTag("Spawner");
        spawners.Clear();
    }

    private nBasicRatn CreateRat(string name)
    {
        nBasicRatn rat;

        if(name == "SupportRat")
        {
            rat = new SupportRat();
        }else if(name == "CommonRat")
        {
            rat = new CommonRat();
        }else if(name == "ShooterRat")
        {
            rat = new ShooterRat();
        }

        return rat;
    }

    private void SetUpSpawners() 
    {
        for(int i = 0; i<spawners.Count; i++)
        {

        }
    }
    /// <summary>
    /// Podría hacerse el Pool no solo en las ratas, sino también, en el número de spawners activos
    /// De este modo ahorraremos recursos
    /// </summary>
    private void AddSpawnersToList()//Añadimos a la lista de spawners todos aquellos spawners que se encuentren activos en la escena
    {
        spawners.Clear();

        foreach(GameObject spawner in allSpawners)
        {
            if (spawner.activeInHierarchy /*Esto sujeto a revisión*/ && !spawners.Contains(spawner)) 
            {
                spawners.Add(spawner);
                Debug.Log(spawners);
            } 
        }
    }

    private void DeleteSpawnersFromList()//Eliminamos de la lista de spawners todos aquellos que se encuentren desactivados en la escena
    {
        foreach(GameObject spawner in spawners)
        {
            if (!spawner.activeInHierarchy) //Esto queda sujeto a revisión
            {
                spawners.Remove(spawner);
                Debug.Log(spawners);
            }
        }
    }

    private void ChooseSpawner() //Elegimos los spawners mejor posicionados con respecto al jugador y a la zona en la que se encuentra
    {
        foreach(GameObject spawner in spawners)
        {
            //if(player) //Si el jugador está dentro de una zona, se le asignará a dicho spawn una mayor probabilidad de ser escogido
        }
    }

    private void OnTriggerEnter(Collider other) //Detectamos si el jugador está dentro de alguna de las zonas de spawn
    {
        //if(player)
    }

    private void Update()
    {
        
    }
}
    /*
    // Update is called once per frame
    private void Update()
    {
        if(currentWaveIndex >=  )
        {
            //This is where it will be defined what will happen when the player ends all of the waves
        }

        if (readyToCountDown)
        {
            countdown -= Time.deltaTime;
        }

        if(countdown <= 0)
        {
            readyToCountDown = false;

            countdown = ;

            StartCoroutine(SpawnWave());
        }

        if ()
        {

        }
    }

    private IEnumerator SpawnWave()
    {
        if(currentWaveIndex <)
        {
            for (int i = 0;i < countdown; i++)
            {
                Vector3 randomOffSet = UnityEngine.Random.insideUnitSphere;

                randomOffSet.y = 0f;

                randomOffSet *= spawnRadius;

                Vector3 spawnPosition = spawnPoint.transform.position + randomOffSet;

                EnemyAIWave enemy = Instatiate();

                enemy.SetWaveSpawner(this);

                GameObject playerObj = GameObject.FindWithTag("Player");

                if(playerObj != null)
                {
                    enemy.player = playerObj.transform;

                    
                }
            }
        }
    }
}
*/