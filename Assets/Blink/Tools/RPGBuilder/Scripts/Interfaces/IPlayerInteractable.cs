
public interface IPlayerInteractable
{
    void Interact();
    void ShowInteractableUI();
    string getInteractableName();
    bool isReadyToInteract();

    RPGCombatDATA.INTERACTABLE_TYPE getInteractableType();
}
