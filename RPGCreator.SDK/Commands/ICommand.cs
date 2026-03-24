namespace RPGCreator.SDK.Commands;

public interface ICommand : IDisposable
{
    string Name { get;}
    public void Execute();
    public void Undo();
    public void OnRemovedFromStack();
}