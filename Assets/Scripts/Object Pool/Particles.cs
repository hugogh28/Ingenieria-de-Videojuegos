using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;

public class Particles : MonoBehaviour, IPoolableObject
{
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float YSpeed = 0.01f;
    [SerializeField] private float XSpeed = 0.02f;
    private float offSet = 0f;
    public bool Active
    {
        get { return Active; }
        set { Active = value; }
    }
    private Vector3 color = new Vector3(255, Random.Range(0, 255), 0);
    public float timeAlive = 2f;
    private Vector3 pos = new Vector3(Random.Range(0, 20), Random.Range(0, 2), Random.Range(0, 40)); //Asegurar que esto está dentro de las coordenadas locales 
    private float timer = 0f;

    void Start()
    {
        this.Active = false;
        this.gameObject.SetActive(false);
    }

    public Particles(bool active ,float YSpeed, float XSpeed, float timeAlive, Vector3 color, Vector3 pos)
    {
        this.YSpeed = YSpeed;
        this.XSpeed = XSpeed;
        this.color = color;
        this.timeAlive = timeAlive;
        this.Active = active;
        this.gameObject.SetActive(false);
        this.pos = pos; //Posición aleatoria dentro de coordenadas locales del objeto padre
    }

    public void Reset()
    {
        this.timer = 0f;
        this.gameObject.SetActive(false);
        this.offSet = 0f;
        this.timeAlive = 2f; //Quizás haga falta eliminar esto
        this.XSpeed = 0f;
        this.Active = false;
        this.pos = new Vector3(Random.Range(0, 20), Random.Range(0, 2), Random.Range(0, 40)); //Posición aleatoria dentro de coordenadas locales del objeto padre
        this.color = new Vector3(255, Random.Range(0, 255), 0);
    }

    public IPoolableObject Clone()
    {
        return new Particles(Active, XSpeed, YSpeed, timeAlive, color, pos);
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.gameObject.activeSelf) return;
        this.timer += Time.deltaTime;
        offSet = amplitude * Mathf.Sin(timer * XSpeed);
        this.pos.x += offSet;
        this.pos.y += this.YSpeed;
        if (timeAlive - timer <= 0)
        {
            this.Reset();
        }
    }
}
