using UnityEngine;
public class ParticleData
{
    [SerializeField] public float amplitude = 0.5f;
    [SerializeField] public float YSpeed = 0.01f;
    [SerializeField] public float XSpeed = 0.02f;
    public float timeAlive = 2f;
    private float timer = 0f;

    public Vector3 color = new Vector3(255, 0, 0);
}
