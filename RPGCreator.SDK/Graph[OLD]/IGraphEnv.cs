namespace RPGCreator.SDK.Graph;

public interface IGraphEnv
{

    bool SetRegister<T>(int index, T value);
    T GetRegister<T>(int index);
    object? GetVariable(string name);
    void SetVariable(string name, object? value);
    T? GetVariable<T>(string name);
    static abstract void AddVM(string path, object? value);
    static abstract void AddVMs(params (string path, object? value)[] values);
    public object? GetVM(string path);
}