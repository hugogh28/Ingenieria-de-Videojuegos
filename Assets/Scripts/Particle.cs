using Unity.VisualScripting;
using UnityEngine;

/*public abstract class Particle : IPoolableObject
{
    public Transform pos;
    //public Vector3 color = new Vector3(255, Random.Range(0, 255), 0);
    //public GameObject particle;
    public IPoolableObject particle;

    private void Start()
    {
        particle.Active = false;
    }
    public Particle(IPrototype o)
    {
        o.Clone();
    }

    public abstract IPoolableObject Clone();
}*/

public interface IParticle : IPoolableObject
{
    public float amplitude { get; set; }
}