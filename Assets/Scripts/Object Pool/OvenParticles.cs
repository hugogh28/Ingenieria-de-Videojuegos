using Unity.VisualScripting;
using UnityEngine;

public class OvenParticles : MonoBehaviour
{
    [SerializeField] private Transform initialPosition;
    [SerializeField] private float cooldown = 0.1f;
    [SerializeField] private Particles particles;

    ObjectPool objectPool;

    private void Start()
    {
        objectPool = new ObjectPool(particles, 200, false);
    }

    private void OnTriggerEnter(Collider other) //Por esto, puede ser que solo aparezcan una vez las partículas
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            objectPool.Get();
        }
        else return;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            for (int i = 0; i < objectPool.GetCount(); i++) //Debe haber una manera más eficiente de buscar en la lista que esta 
            {
                objectPool.Release(particles);
            }
        }
        else return;
    }
}
