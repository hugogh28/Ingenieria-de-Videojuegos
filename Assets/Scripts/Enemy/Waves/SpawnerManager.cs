
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private float countdown;
    //[SerializeField] private GameObject spawnPoint;
    //[SerializeField] private float spawnRadius = 3f;

    public List<WaveManager> waves;

    public int currentWaveIndex = 0;

    public float spawnProbability = 1f;

    public List<GameObject> spawners;
    public List<GameObject> avalaibleSpawners;

    public float maxDistanceToSpawn = 50f;

    GameObject player;

    Vector3 playerPosition;


    //public GameObject[] allSpawners;

    //private bool readyToCountDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //readyToCountDown = true;

        player = GameObject.FindGameObjectWithTag("Player");
        //allSpawners = GameObject.FindGameObjectsWithTag("Spawner");
        spawners.Clear();
    }

    /*private nBasicRatn CreateRat(string name)
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
    }*/

    /*private void SetUpSpawners() 
    {
        for(int i = 0; i<spawners.Count; i++)
        {

        }
    }*/
    /// <summary>
    /// Podría hacerse el Pool no solo en las ratas, sino también, en el número de spawners activos
    /// De este modo ahorraremos recursos
    /// </summary>
    /*public void AddSpawnersToList()//Añadimos a la lista de spawners todos aquellos spawners que se encuentren activos en la escena
    {
        if(spawners != null) 
        { 
            spawners.Clear();
        }
        foreach(var spawner in allSpawners)
        {
            if (spawner.activeInHierarchy && !spawners.Contains(spawner)) 
            {
                //spawners.Clear();
                spawners.Add(spawner);
                Debug.Log(spawners);
            } 
        }
    }*/

    /*private void DeleteSpawnersFromList()//Eliminamos de la lista de spawners todos aquellos que se encuentren desactivados en la escena
    {
        foreach(GameObject spawner in spawners)
        {
            if (!spawner.activeInHierarchy) //Esto queda sujeto a revisión
            {
                spawners.Remove(spawner);
                Debug.Log(spawners);
            }
        }
    }*/

    private void TakeInCountSpawner() //Cada spawner que esté activo y dentro de un radio de 50 unidades con respecto al jugador será incluido para poder instanciar ratas
    {
        foreach (var spawner in spawners)
        {
            if (Vector3.Distance(playerPosition, spawner.transform.position) <= maxDistanceToSpawn && !avalaibleSpawners.Contains(spawner))
            {
                avalaibleSpawners.Add(spawner);
                spawnProbability = 1f / avalaibleSpawners.Count;
                Debug.Log(avalaibleSpawners);
            }
            else if(Vector3.Distance(playerPosition, spawner.transform.position) > maxDistanceToSpawn)
            {
                avalaibleSpawners.Remove(spawner);
                spawnProbability = 1f / avalaibleSpawners.Count;
                Debug.Log(avalaibleSpawners);
            }
        }
    }

    
    private void Update()
    {
        playerPosition = player.transform.position;
        TakeInCountSpawner();
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