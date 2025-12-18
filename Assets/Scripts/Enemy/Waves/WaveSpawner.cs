using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private float spawnRadius = 3f;

    public int currentWaveIndex = 0;

    private bool readyToCountDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        readyToCountDown = true;

        for (int i = 0; i < countdown; i++)
        {
            
        }
    }

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
