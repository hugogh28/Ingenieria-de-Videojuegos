using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PointsDoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Cost")]
    [SerializeField] private int openCost = 1000;
    [SerializeField] private string actionText = "abrir puerta";

    [Header("Effects")]
    [SerializeField] private GameObject openEffectPrefab;
    [SerializeField] private Vector3 effectOffset;
    [SerializeField] private AudioClip rubbleSound;
    [SerializeField] private float destroyDelay = 1.5f;
    [SerializeField] private float effectDestroyDelay = 4f;

    [Header("Debug")]
    [SerializeField] private bool debugInteraction = false;

    private bool isOpen;

    public string InteractionActionText => $"{actionText} ({openCost} pts)";

    private void Awake()
    {
        Outline outline = GetComponentInChildren<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public bool CanInteract(PlayerController player)
    {
        return !isOpen && player != null;
    }

    public void Interact(PlayerController player)
    {
        if (isOpen || player == null)
        {
            return;
        }

        if (!player.TrySpendPoints(openCost))
        {
            if (debugInteraction)
            {
                Debug.Log($"{name}: el jugador necesita {openCost} puntos para abrir la puerta.", this);
            }

            return;
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;
        SpawnOpenEffect();
        PlayRubbleSound();
        DisableDoor();
        Destroy(gameObject, destroyDelay);
    }

    private void SpawnOpenEffect()
    {
        if (openEffectPrefab == null)
        {
            return;
        }

        GameObject effect = Instantiate(
            openEffectPrefab,
            transform.position + effectOffset,
            openEffectPrefab.transform.rotation
        );

        Destroy(effect, effectDestroyDelay);
    }

    private void PlayRubbleSound()
    {
        if (rubbleSound == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(rubbleSound, transform.position);
    }

    private void DisableDoor()
    {
        foreach (Renderer doorRenderer in GetComponentsInChildren<Renderer>())
        {
            doorRenderer.enabled = false;
        }

        foreach (Collider doorCollider in GetComponentsInChildren<Collider>())
        {
            doorCollider.enabled = false;
        }
    }
}
