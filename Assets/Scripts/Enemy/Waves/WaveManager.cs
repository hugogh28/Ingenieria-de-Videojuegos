using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WaveManager : MonoBehaviour
{
    public WaveManager Instance {  get; set; }

    public List<nBasicRatn> ratsPerWave;

    public ShooterRat shooter;
    public SupportRat support;
    public CommonRat common;

    public float shooterProb = 0.5f;
    public float supportProb = 0.5f;
    public float commonProb = 0.5f;

    int numberOfRats;

    int currentWave;
    float random;

    private void Start()
    {
        currentWave = 1;
        commonProb = 0.9f;
        shooterProb = 0.1f;
        supportProb = 0f;
    }

    public void IncrementDifficulty()
    {
        commonProb = 0.5f + 0.5f / currentWave;
        shooterProb = 0f + 0.02f*currentWave; //Limitar a, por ejemplo, un máximo de 60% de probabilidades de aparecer
        supportProb = 0f + 0.01f*currentWave; //Limitar a, por ejemplo, un máximo de 30/40% de probabilidades de aparecer
    }

    public void WaveCreation() //Creación de una oleada semialeatoria, con enemigos variados y de habilidades y ataques distintos
    {
        //Añadir un randomizador de número de ratas por oleada que aumente con el tiempo, pero que llegue a cierto tope
        random = Random.Range(0f, 1f);
        if(random<=shooterProb)
        {
            ratsPerWave.Add(shooter);
        }
        random = Random.Range(0f, 1f);
        if (random <= supportProb)
        {
            ratsPerWave.Add(support);
        }
        random= Random.Range(0f, 1f);
        if (random<=commonProb)
        {
            ratsPerWave.Add(common);
        }
        currentWave++;
        IncrementDifficulty();
    }

    private void Update()
    {
        
    }
}
