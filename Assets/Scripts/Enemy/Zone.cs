using UnityEngine;

public class Zone : MonoBehaviour
{
    public SpawnerManager wave;

    public void OnTriggerEnter(Collider other) //Detectamos si el jugador está dentro de alguna de las zonas de spawn
    {
        if (!other.CompareTag("Player")) return;
        foreach (Transform t in transform) //Se activa cada hijo del objeto y si este no estaba en la lista de spawners, es incluido en la misma
        { 
            t.gameObject.SetActive(true);
            if(wave.spawners != null && !wave.spawners.Contains(t.GetComponent<Spawner>()) ) wave.spawners.Add(t.GetComponent<Spawner>());
        }
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }
}
