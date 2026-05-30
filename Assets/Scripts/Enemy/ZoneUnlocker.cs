using UnityEngine;

public class ZoneUnlocker : MonoBehaviour
{
    [SerializeField] public int pointToUnlock;

    private void Start()
    {
        
    }

    private void OnMouseDown()
    {
        if(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().points >= pointToUnlock)
        {
            gameObject.SetActive(false);
        }
    }
}
