public interface IInteractable
{
    string InteractionActionText { get; }
    bool CanInteract(PlayerController player);
    void Interact(PlayerController player);
}
