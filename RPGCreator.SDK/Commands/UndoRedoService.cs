using RPGCreator.SDK.Services.EngineService;

namespace RPGCreator.SDK.Commands;

public class UndoRedoService : IUndoRedoService
{
    private const int MaxCommandHistory = 100;
    
    public event Action? StateChanged;
    
    private readonly LinkedList<ICommand> _undoCommands = new LinkedList<ICommand>();
    private readonly LinkedList<ICommand> _redoCommands = new LinkedList<ICommand>();
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        
        AddUndoToStack(command);
        
        ClearRedoStack();
        StateChanged?.Invoke();
    }

    private void CleanCommand(ICommand command)
    {
        command.OnRemovedFromStack();
        command.Dispose();
    }
    
    private void AddUndoToStack(ICommand command)
    {
        _undoCommands.AddLast(command);

        if (_undoCommands.Count > MaxCommandHistory)
        {
            var oldest = _undoCommands.First;
            if (oldest != null)
            {
                CleanCommand(oldest.Value);
                _undoCommands.RemoveFirst();
            }
        }
    }

    private void AddRedoToStack(ICommand command)
    {
        _redoCommands.AddLast(command);

        if (_redoCommands.Count > MaxCommandHistory)
        {
            var oldest = _redoCommands.First;
            if (oldest != null)
            {
                CleanCommand(oldest.Value);
                _redoCommands.RemoveFirst();
            }
        }
    }

    private ICommand? PopUndoCommand()
    {
        var command = _undoCommands.Last;
        
        if (command != null)
        {
            _undoCommands.RemoveLast();
            return command.Value;
        }

        return null;
    }
    
    private ICommand? PopRedoCommand()
    {
        var command = _redoCommands.Last;
        
        if (command != null)
        {
            _redoCommands.RemoveLast();
            return command.Value;
        }
        return null;
    }

    public void UndoLastCommand()
    {
        if (_undoCommands.Count <= 0) return;
        ICommand? command = PopUndoCommand();

        if (command != null)
        {
            command.Undo();
            AddRedoToStack(command);
        }
        
        StateChanged?.Invoke();
    }
    
    public void RedoLastCommand()
    {
        if (_redoCommands.Count <= 0) return;
        ICommand? command = PopRedoCommand();

        if (command != null)
        {
            command.Execute();
            AddUndoToStack(command);
        }

        StateChanged?.Invoke();
    }

    private void ClearRedoStack()
    {
        foreach (var command in _redoCommands)
            CleanCommand(command);
        _redoCommands.Clear();
    }

    private void ClearUndoStack()
    {
        foreach (var command in _undoCommands)
            CleanCommand(command);
        _undoCommands.Clear();
    }
    
    public void ClearHistory()
    {
        ClearUndoStack();
        ClearRedoStack();
        
        StateChanged?.Invoke();
    }
    
    public string GetUndoCommandName() => 
        _undoCommands.Count > 0 ? _undoCommands.Last().Name : "No Undo Available";
    public string GetRedoCommandName() => 
        _redoCommands.Count > 0 ? _redoCommands.Last().Name : "No Redo Available";
    public bool CanUndo => _undoCommands.Count > 0;
    public bool CanRedo => _redoCommands.Count > 0;
}