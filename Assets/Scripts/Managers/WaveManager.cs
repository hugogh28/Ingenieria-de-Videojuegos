using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public List<BasicRat> ratsPerWave = new List<BasicRat>();

    [SerializeField] RatManager rat;

    float shooterProb;
    float supportProb;
    //float commonProb;
    float tankProb;

    //Estos contadores se corresponden a las posiciones del array de RatManager
    private int commonNormal = 0;
    private int commonTank = 50;
    private int supportNormal = 100;
    private int supportTank = 150;
    private int shooterNormal = 200;
    private int shooterTank = 250;


    int currentWave;
    private float random;
    int numRats;
    int increment;
    [SerializeField] float incrementMultiplier = 1.8f; //Multiplicador del incremento
    [SerializeField] int maxRatsPerWave = 50;//Número máximo de ratas que puede haber por oleada
    [SerializeField] int wavesToWin = 10; //Número de oleadas que hay que superar para ganar

    public int activeRats { get; private set; }
    public int CurrentWave => currentWave;
    public int WavesToWin => wavesToWin;
    public bool VictoryReached { get; private set; }
    public bool waveDirtyFlag { get; private set; }
    public event Action<int> WaveChanged;
    public event Action VictoryReachedEvent;

    private void Start()
    {
        currentWave = 0;
        //commonProb = 1f;
        shooterProb = 0f;
        supportProb = 0f;
        numRats = 5;
        increment = 0;
        tankProb = 0f;

        WaveCreation();
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
        ResetVariables(); //Se resetean los contadores de cada tipo de rata para evitar salirse del pool

        ratsPerWave.Clear(); //Se limpia la lista de ratas de la oleada

        activeRats = 0;
        waveDirtyFlag = true;

        for (int i = 0; i < numRats; i++)  //Se llena la lista de nuevo, con las ratas del pool
        {
            random = UnityEngine.Random.value;
            if (random < supportProb) //Si random es menor que supportProb, se escogerá una rata support
            {
                AddRatToWave(tankProb, ref supportTank, ref supportNormal);
            }
            else if (random < supportProb + shooterProb) //Si random es menor a supportProb + shooterProb, se escogerá una rata shooter
            {
                AddRatToWave(tankProb, ref shooterTank, ref shooterNormal);
            }
            else //Si no es ni support, ni shooter, la rata será normal
            {
                AddRatToWave(tankProb, ref commonTank, ref commonNormal);
            }
        }
        currentWave++;
        WaveChanged?.Invoke(currentWave);
        IncrementDifficulty();
    }

    private void AddRatToWave(float givenValue, ref int idxTank, ref int idxNormal) //Se pasan los valores por referencia para modificar las variables que reciba el método
    {
        random = UnityEngine.Random.value;
        if (random < givenValue) //Si random es menor o igual a tankProb, aparece una rata tanque
        {
            ratsPerWave.Add(rat.poolOfRats[idxTank]);
            rat.poolOfRats[idxTank].Active = true;
            idxTank++;
        }
        else
        {
            ratsPerWave.Add(rat.poolOfRats[idxNormal]);
            rat.poolOfRats[idxNormal].Active = true;
            idxNormal++;
        }
        activeRats++;
        waveDirtyFlag = true;
    }

    public void NotifyRatDied(BasicRat rat)
    {
        if(rat.Active != true)
        {
            return;
        }

        rat.Active = false;
        activeRats--;
        waveDirtyFlag = true;
    }

    public bool CanTriggerVictory()
    {
        return !VictoryReached && currentWave >= wavesToWin && activeRats <= 0;
    }

    public void TriggerVictory()
    {
        if (VictoryReached)
        {
            return;
        }

        VictoryReached = true;
        waveDirtyFlag = true;
        VictoryReachedEvent?.Invoke();
    }

    public void ResetDirtyFlag()
    {
        waveDirtyFlag = false;
    }

    private void ResetVariables() //Para reiniciar los contadores de la lista de ratas
    {
        commonNormal = 0;
        commonTank = 50;
        supportNormal = 100;
        supportTank = 150;
        shooterNormal = 200;
        shooterTank = 250;
    }

    private void Update()
    {
        //ratsPerWave.RemoveAll(r => r == null); //Eliminamos todas las ratas que hayan muerto
    }
}
