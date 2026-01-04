using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WaveManager : MonoBehaviour
{
    //public WaveManager Instance {  get; set; }
    //SpawnerManager spawnerManager;

    public List<nBasicRatn> ratsPerWave;

    ShooterRat shooter; 
    SupportRat support;
    CommonRat common;

    float shooterProb;
    float supportProb;
    float commonProb;
    float tankProb;


    int currentWave;
    private float random;
    int numRats;
    int increment;
    [SerializeField] float incrementMultiplier = 1.8f; //Multiplicador del incremento
    [SerializeField] int maxRatsPerWave = 50;//Número máximo de ratas que puede haber por oleada

    private void Start()
    {
        currentWave = 0;
        commonProb = 1f;
        shooterProb = 0f;
        supportProb = 0f;
        numRats = 5;
        increment = 0;
        tankProb = 0f;
    }
    
    private void IncrementDifficulty()
    {
        //ratsPerWave.Clear(); //Llamar a borrar la lista de ratas cuando se haya terminado la oleada

        shooterProb = 0.02f * currentWave; //Por cada oleada, se incrementa un 2% la probabilidad de que aparezca una rata que dispara
        shooterProb = Mathf.Clamp(shooterProb,0f,0.4f); //Se limita la probabilidad de spawn de ratas que disparan a un 40%
        supportProb = 0.01f * currentWave; //Por cada oleada, se incrementa un 1% la probabilidad de que aparezca una rata que cura
        supportProb = Mathf.Clamp(supportProb, 0f, 0.2f);//Se limita la probabilidad de ratas que curan a un 20%
        //commonProb = 1f - shooterProb - supportProb; //Por cada oleada, se disminuye la probabilidad de que aparezca una rata normal de forma inversamente proporcional a la probabilidad de las otras

        tankProb = 0.05f * currentWave; //Se aumenta la probabilidad de spawn de ratas tanque un 5% por cada oleada
        tankProb = Mathf.Clamp(tankProb, 0f, 1f); //Se limita a un 100% la probabilidad de spawn de ratas tanque

        increment = Mathf.RoundToInt(currentWave * incrementMultiplier); //Se incrementa el número de ratas que aparecen por cada oleada
        numRats = Mathf.Clamp(increment,5,maxRatsPerWave); //Se limita el número de ratas que pueden aparecer a un máximo de 50
    }

    public void WaveCreation() //Creación de una oleada semialeatoria, con enemigos variados y de habilidades y ataques distintos
    {
        for (int i = 0; i < numRats; i++) 
        {
            random = Random.value;
            if (random < supportProb)
            {
                random = Random.value;
                if (random <= tankProb) ratsPerWave.Add(support.Clone(true));
                else ratsPerWave.Add(support.Clone(false));
            }
            else if (random < supportProb + shooterProb)
            {
                random = Random.value;
                if (random <= tankProb) ratsPerWave.Add(shooter.Clone(true));
                ratsPerWave.Add(shooter.Clone(false));
            }
            else
            {
                random = Random.value;
                if (random <= tankProb) ratsPerWave.Add(common.Clone(true));
                ratsPerWave.Add(common.Clone(false));
            }
        }
        currentWave++;
        IncrementDifficulty();
    }

    private void Update()
    {
        //ratsPerWave.RemoveAll(r => r == null); //Eliminamos todas las ratas que hayan muerto
    }
}
