using UnityEngine;

public class ZoneUnlocker : MonoBehaviour, IUnlockableObject
{
    [SerializeField] public int pointToUnlock;

    private void OnMouseDown()
    {
        Unlock();
    }

    public void Unlock()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null || !playerObject.TryGetComponent(out PlayerController player))
        {
            return;
        }

        if (!player.TrySpendPoints(pointToUnlock))
        {
            return;
        }

        gameObject.SetActive(false);
    }
}
