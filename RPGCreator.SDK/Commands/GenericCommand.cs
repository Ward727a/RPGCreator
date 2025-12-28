namespace RPGCreator.SDK.Commands;

public class GenericCommand : ICommand
{

    private readonly Action _executeAction;
    private readonly Action _undoAction;
    private readonly string _name;
    public string Name { get => _name; }
    
    public GenericCommand(Action executeAction, Action undoAction, string name = "Unknown Command")
    {
        _executeAction = executeAction;
        _undoAction = undoAction;
        _name = name;
    }
    
    public void Execute()
    {
        _executeAction();
    }

    public void Undo()
    {
        _undoAction();
    }

    public override string ToString() => _name;
}