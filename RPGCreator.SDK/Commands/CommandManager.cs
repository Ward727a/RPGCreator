using RPGCreator.SDK.EngineService;

namespace RPGCreator.SDK.Commands;

public class CommandManager : ICommandManager
{
    public event Action? StateChanged;
    
    private readonly Stack<ICommand> _undoCommands = new Stack<ICommand>(100);
    private readonly Stack<ICommand> _redoCommands = new Stack<ICommand>(100);
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoCommands.Push(command);
        _redoCommands.Clear();
        StateChanged?.Invoke();
    }

    public void UndoLastCommand()
    {
        if (_undoCommands.Count <= 0) return;
        ICommand command = _undoCommands.Pop();
        _redoCommands.Push(command);
        command.Undo();
        StateChanged?.Invoke();
    }
    
    public void RedoLastCommand()
    {
        if (_redoCommands.Count <= 0) return;
        ICommand command = _redoCommands.Pop();
        command.Execute();
        _undoCommands.Push(command);
        StateChanged?.Invoke();
    }
    
    public void ClearHistory()
    {
        _undoCommands.Clear();
        _redoCommands.Clear();
        StateChanged?.Invoke();
    }
    
    public string GetUndoCommandName() => 
        _undoCommands.Count > 0 ? _undoCommands.Peek().Name : "No Undo Available";
    public string GetRedoCommandName() => 
        _redoCommands.Count > 0 ? _redoCommands.Peek().Name : "No Redo Available";
    public bool CanUndo => _undoCommands.Count > 0;
    public bool CanRedo => _redoCommands.Count > 0;
}