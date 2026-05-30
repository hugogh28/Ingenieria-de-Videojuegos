using UnityEngine;

public class ZoneUnlocker : MonoBehaviour, IUnlockableObject
{
    [SerializeField] public int pointToUnlock;

    private void Start()
    {
        
    }

    private void OnMouseDown() //Cuando el jugador haga click en pantalla sobre el obstáculo, si este tiene puntos suficientes, podrá desbloquear la nueva zona
    {
        Unlock();
    }

    public void Unlock()
    {
        if(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points >= pointToUnlock)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points -= pointToUnlock;
            gameObject.SetActive(false);
        }
    }
}
