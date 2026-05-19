namespace RPGCreator.EngineLib.Types.Assets.Entities;

public interface IInteractableActor
{
    
    public event Action? Interacted;

    public void Interact(Actor FromActor);
    public void InteractWith(IInteractableActor ToActor);

}