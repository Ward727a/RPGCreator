namespace RPGCreator.Core.Managers.CommandManager;

public interface ICommand
{
    public void Execute();
    public void Undo();
    string Name { get;}
}