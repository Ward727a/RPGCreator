namespace RPGCreator.Core.Type.Assets.Actors;

public interface IInteractableActor : IActor
{
    
    public event Action? Interacted;

    public void Interact(IActor FromActor);
    public void InteractWith(IInteractableActor ToActor);

}