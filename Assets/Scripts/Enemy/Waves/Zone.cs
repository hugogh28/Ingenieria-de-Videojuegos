using UnityEngine;

public class Zone : MonoBehaviour
{
    public SpawnerManager wave;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void OnTriggerEnter(Collider other) //Detectamos si el jugador está dentro de alguna de las zonas de spawn
    {
        if (!other.CompareTag("Player")) return;
        foreach (Transform t in transform) 
        { 
            t.gameObject.SetActive(true);
            if(wave.spawners != null && !wave.spawners.Contains(t.gameObject) ) wave.spawners.Add(t.gameObject);
        }
        gameObject.GetComponent<BoxCollider>().enabled = false;
        //wave.AddSpawnersToList();
    }
    
    // Update is called once per frame
    void Update()
    {
    }
}
