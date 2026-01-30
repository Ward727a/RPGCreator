namespace RPGCreator.SDK.Commands;

public interface ICommand
{
    string Name { get;}
    public void Execute();
    public void Undo();
}