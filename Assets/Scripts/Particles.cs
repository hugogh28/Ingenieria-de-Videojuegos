using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;

public class Particles : IPoolableObject
{
    public bool Active { get; set; }

    [SerializeField] private float amplitude = 0.5f; 
    [SerializeField] private float YSpeed = 0.01f;
    [SerializeField] private float XSpeed = 0.02f;
    private float offSet = 0f;
    private Vector3 color = new Vector3(255, Random.Range(0,255), 0);
    private float timeAlive = 2f;
    private Vector3 pos = new Vector3(Random.Range(0, 20), Random.Range(0, 2), Random.Range(0,40));
    private float timer = 0f;

    public Particles(float YSpeed, float XSpeed, float timeAlive, Vector3 color, Vector3 pos)
    {
        this.YSpeed = YSpeed;
        this.XSpeed = XSpeed;
        this.color = color;
        this.timeAlive = timeAlive;
        this.Active = false;
        this.pos = pos; //Posición aleatoria dentro de coordenadas locales del objeto padre
    }

    public void Reset()
    {
        this.timer = 0f;
        this.Active = false;
        this.offSet = 0f;
        this.timeAlive = 2f;
        this.XSpeed = 0f;
        this.pos = new Vector3(Random.Range(0,20),Random.Range(0,2),Random.Range(0,40)); //Posición aleatoria dentro de coordenadas locales del objeto padre
        this.color = new Vector3(255, Random.Range(0,255), 0);
    }

    public IPoolableObject Clone()
    {
        return new Particles(XSpeed, YSpeed, timeAlive, color, pos);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;
        this.timer += Time.deltaTime;
        offSet = amplitude*Mathf.Sin(timer*XSpeed);
        this.pos.x += offSet;
        this.pos.y += this.YSpeed;
        if(this.timer >= timeAlive)
        {
            this.Reset();
        }
    }
}
