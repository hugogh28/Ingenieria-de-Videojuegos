
//using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private float countdown;
    //[SerializeField] private GameObject spawnPoint;
    //[SerializeField] private float spawnRadius = 3f;

    [SerializeField] WaveManager waveManager;

    //public List<WaveManager> waves;

    public int currentWaveIndex = 0;

    public float spawnProbability = 1f;

    public List<Spawner> spawners;
    public List<Spawner> avalaibleSpawners;

    public float maxDistanceToSpawn = 50f;

    public float timeToCheck = 2f;

    GameObject player;

    public Vector3 spawnPosition;

    public Vector3 playerPosition;
    bool waveIsActive = false;

    float timer = 5f;

    //public GameObject[] allSpawners;

    //private bool readyToCountDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //readyToCountDown = true;
        timer = 15f;
        player = GameObject.FindGameObjectWithTag("Player");
        //allSpawners = GameObject.FindGameObjectsWithTag("Spawner");
        spawners.Clear();
        StartCoroutine(Wave());
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
            float distance = Vector3.Distance(playerPosition, spawner.transform.position);
            if (distance <= maxDistanceToSpawn && !avalaibleSpawners.Contains(spawner))
            {
                avalaibleSpawners.Add(spawner);
            }
            else if(distance > maxDistanceToSpawn)
            {
                avalaibleSpawners.Remove(spawner);
            }
        }
    }

    private GameObject ChooseSpawner() //Se elige un spawner semialeatoriamente para que cada rata pueda spawnear 
    {
        List<Spawner> unusedSpawners = avalaibleSpawners.Where(s => s.spawnerIsActive == false).ToList();

        //Debug.Log($"Total spawners: {spawners.Count} | " + $"Available: {avalaibleSpawners.Count} | " + $"Unused: {unusedSpawners.Count}");


        if (unusedSpawners.Count()==0) return null; //Si todos los spawners se encuentran activos se devuelve null

        int random = UnityEngine.Random.Range(0, unusedSpawners.Count());

        unusedSpawners[random].spawnerIsActive = true;

        StartCoroutine(AllowSpawn(unusedSpawners[random], 1.99f));

        return unusedSpawners[random].gameObject;

        /*if (avalaibleSpawners[random].spawnerIsActive == true) //Si no todos los spawners están activos, pero el aleatorio sí lo está se buscará en la lista al primero que no lo esté
        {
            int index = avalaibleSpawners.FindIndex(s => s.spawnerIsActive == false);
            StartCoroutine(AllowSpawn(avalaibleSpawners[index], 2.5f));
            return avalaibleSpawners[index].spawner;
        }
        else
        {
            StartCoroutine(AllowSpawn(avalaibleSpawners[random], 2.5f));

            return avalaibleSpawners[random].spawner;
        }*/
    }

    IEnumerator AllowSpawn(Spawner spawner, float time)
    {
        yield return new WaitForSeconds(time);
        spawner.spawnerIsActive = false;
    }

    private IEnumerator Wave() //Se organiza el spawn de las ratas
    {
        yield return new WaitForSeconds(15f);//Antes de comenzar una oleada, el jugador tendrá 15 segundos para decidir qué puede hacer
        for (int i = 0; i < waveManager.ratsPerWave.Count; i++)
        {
            GameObject o = ChooseSpawner();
            if (o != null)
            {
                waveManager.ratsPerWave[i].transform.position = o.transform.position; //Si no todos los spawns se encuentran activos se elige el spawner de la rata
                waveManager.ratsPerWave[i].gameObject.SetActive(true);

                /*NavMeshHit hit;
                if(NavMesh.SamplePosition(o.transform.position, out hit, 2f, NavMesh.AllAreas))
                {
                    waveManager.ratsPerWave[i].navAgent.Warp(hit.position);
                }*/

                var rat = waveManager.ratsPerWave[i];
                Debug.Log(
                    $"Rat index {i} | ID {rat.GetInstanceID()} | Pos {rat.transform.position}"
                );

                yield return new WaitForSeconds(0.5f);
            }
            else if(o == null)//Si todos los spawns se encuentran activos se reintenta spawnear la rata tras transcurrir el tiempo definido, por ejemplo, 2 segundos
            {
                yield return new WaitForSeconds(timeToCheck);
                i--;
            }
        }

        waveIsActive = false;
    }

    public void CheckIfWaveIsOver() //DA ERROR PARA LA PRIMERA OLEADA PORQUE LA LISTA NO ESTÁ CREADA
    {
        if (waveIsActive) return;

        if (waveManager.ratsPerWave.All(r => r == null || !r.gameObject.activeSelf))
        {
            waveManager.ratsPerWave.Clear();
            waveIsActive = true;
            waveManager.WaveCreation();
            StartCoroutine(Wave());
        }
    }

    private void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                playerPosition = player.transform.position;
                TakeInCountSpawner(); 
                CheckIfWaveIsOver();
                timer = 5f;
            }
        }
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