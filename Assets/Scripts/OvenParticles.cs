using Unity.VisualScripting;
using UnityEngine;

public class OvenParticles : MonoBehaviour
{
    [SerializeField] private Transform initialPosition;
    [SerializeField] private float cooldown = 0.1f;

    ObjectPool ObjectPool;

    private void Start()
    {
        
    }
}
