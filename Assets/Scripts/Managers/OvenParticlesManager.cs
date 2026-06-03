using Unity.VisualScripting;
using UnityEngine;

public class OvenParticlesManager : MonoBehaviour
{
    [SerializeField] private Transform initialPosition;
    [SerializeField] private float cooldown = 0.1f;
    //[SerializeField] private Particles particles;

    [SerializeField] private GameObject particles;
    [SerializeField] private int nPartsType = 300;

    [SerializeField] private Particles particle;

    public Particles[] poolOfParticles = new Particles[300];

    [SerializeField] private GameObject parentOfParticles;

    //ObjectPool objectPool;

    private void Start()
    {
        //objectPool = new ObjectPool(particles, 200, false);
        for(int i = 0; i < nPartsType; i++)
        {
            poolOfParticles[i] = (Particles)particle.Clone();
            poolOfParticles[i].transform.SetParent(parentOfParticles.transform);
        }
    }

    private void OnTriggerEnter(Collider other) //Por esto, puede ser que solo aparezcan una vez las partículas
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            parentOfParticles.SetActive(true);
        }
        else return;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            /*for (int i = 0; i < objectPool.GetCount(); i++) //Debe haber una manera más eficiente de buscar en la lista que esta 
            {
                objectPool.Release(particles);
            }*/
            parentOfParticles.SetActive(false);
        }
        else return;
    }
}
