namespace RPGCreator.SDK.Commands;

public interface ICommand
{
    public void Execute();
    public void Undo();
    string Name { get;}
}