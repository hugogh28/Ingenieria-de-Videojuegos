using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SpawnerManager : MonoBehaviour
{
    [SerializeField] private float countdown;

    [SerializeField] WaveManager waveManager;

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

    public GameObject textEndWave;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        timer = 15f;
        player = GameObject.FindGameObjectWithTag("Player");
        spawners.Clear();
        StartCoroutine(Wave()); //La primera oleada se crea nada más inicializarse la escena
    }

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

        if (unusedSpawners.Count() == 0) return null; //Si todos los spawners se encuentran activos se devuelve null

        int random = UnityEngine.Random.Range(0, unusedSpawners.Count());

        unusedSpawners[random].spawnerIsActive = true;

        StartCoroutine(AllowSpawn(unusedSpawners[random], 1.99f)); //Se lanza una corutina como "timer" del spawner

        return unusedSpawners[random].gameObject;
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
                waveManager.ratsPerWave[i].gameObject.SetActive(true);

                NavMeshHit hit;
                if(NavMesh.SamplePosition(o.transform.position, out hit, 2f, NavMesh.AllAreas)) //En caso de salir en un spawner que no está en una NavMesh, se buscará la posición más cercana a una, para que el component NavMeshAgent, no de errores
                {
                    waveManager.ratsPerWave[i].navAgent.Warp(hit.position);
                }

                var rat = waveManager.ratsPerWave[i];
                Debug.Log(
                    $"Rat index {i} | ID {rat.GetInstanceID()} | Pos {rat.transform.position}"
                );

                yield return new WaitForSeconds(0.5f);
            }
            else if(o == null)//Si todos los spawns se encuentran activos se reintenta spawnear la rata tras transcurrir el tiempo definido, por ejemplo, 2 segundos
            {
                Debug.Log($"No hay spawner disponible para rata {i}, reintentando...");
                yield return new WaitForSeconds(timeToCheck);
                i--;
            }
        }
        Debug.Log("Wave terminó de spawnear");
    }

    public void CheckIfWaveIsOver()
    {
        Debug.Log($"CheckIfWaveIsOver | waveIsActive: {waveIsActive} | ratas activas: {waveManager.ratsPerWave.Count(r => r != null && r.Active)}");


        if (waveManager.ratsPerWave.Count(r => r != null && r.Active) > 0)
        {
            return;
        }
        else if (waveManager.ratsPerWave.Count(r => r != null && r.Active) <= 0)
        {
            StartCoroutine(ShowOnEndOfWave());
            waveManager.ratsPerWave.Clear();
            waveIsActive = true;
            waveManager.WaveCreation();
            StartCoroutine(Wave());
        }
    }

    private IEnumerator ShowOnEndOfWave() //Función, cuya finalidad es la de mostrar un letrero que indique el final de una ronda
    {
        textEndWave.SetActive(true);
        yield return new WaitForSeconds(4);
        textEndWave.SetActive(false);
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