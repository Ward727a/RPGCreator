using System.Runtime.CompilerServices;

namespace RPGCreator.SDK.Logging;

/// <summary>
/// Gets a logger that automatically scopes log messages to the class and method they were logged from.<br/>
/// Message will be logged in the format: [AssemblyName.ClassName.MethodName] Message
/// </summary>
public class ScopedLogger
{
    private readonly string _className;
    private readonly string _asmName;

    private string _customPrefix;
    
    public ScopedLogger(string fullName)
    {
        _className = fullName.Split('.').Last();
        if(string.IsNullOrEmpty(_className))
            _className = fullName;
        _asmName = "UnknownAssembly";
    }
    
    public ScopedLogger(string fullName, string asmName)
    {
        _className = fullName.Split('.').Last();
        if(string.IsNullOrEmpty(_className))
            _className = fullName;
        _asmName = asmName;
    }

    /// <summary>
    /// Adds a custom prefix to all log messages.<br/>
    /// This can be useful to add context to the log messages.<br/>
    /// For example, you can use it to indicate the current operation or state of the class.<br/>
    /// <br/>
    /// Note: The prefix will be added before the message, after the [AssemblyName.ClassName.MethodName] part.<br/>
    /// Like: [AssemblyName.ClassName.MethodName] CustomPrefix Message.
    /// Note2: If the provided prefix does not end with a space, one will be added automatically if the prefix is not empty.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public ScopedLogger SetCustomPrefix(string prefix = "")
    {
        if(prefix != "" && !prefix.EndsWith(" "))
            prefix += " ";
            
        _customPrefix = prefix;
        return this;
    }
    
    /// <summary>
    /// In the case where you need to put a value in 'args' you need to follow this:<br/>
    /// - First situation "You have 1 arg to put inside the 'args' parameter":<br/>
    /// >>> Easy! Just before your arg, put a 'args:' like: Info("Message {arg}", args: myArg);<br/>
    /// - Second situation "You have 2 or + to put inside the 'args' parameter":<br/>
    /// >>> Just before your args, put a 'args: [{ARG1}, {ARG2}, ...]' like: <br/>
    /// >>> Info("Message {arg1}, {arg2}", args: [MyArg, SecondArg]);
    ///</summary>
    public void Warning(string message, [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        Logger.Warning($"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    /// <summary>
    /// In the case where you need to put a value in 'args' you need to follow this:<br/>
    /// - First situation "You have 1 arg to put inside the 'args' parameter":<br/>
    /// >>> Easy! Just before your arg, put a 'args:' like: Info("Message {arg}", args: myArg);<br/>
    /// - Second situation "You have 2 or + to put inside the 'args' parameter":<br/>
    /// >>> Just before your args, put a 'args: [{ARG1}, {ARG2}, ...]' like: <br/>
    /// >>> Info("Message {arg1}, {arg2}", args: [MyArg, SecondArg]);
    ///</summary>
    public void Info(string message, [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        Logger.Info($"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    /// <summary>
    /// In the case where you need to put a value in 'args' you need to follow this:<br/>
    /// - First situation "You have 1 arg to put inside the 'args' parameter":<br/>
    /// >>> Easy! Just before your arg, put a 'args:' like: Info("Message {arg}", args: myArg);<br/>
    /// - Second situation "You have 2 or + to put inside the 'args' parameter":<br/>
    /// >>> Just before your args, put a 'args: [{ARG1}, {ARG2}, ...]' like: <br/>
    /// >>> Info("Message {arg1}, {arg2}", args: [MyArg, SecondArg]);
    ///</summary>
    public void Error(string message, [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        Logger.Error($"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    public void Error(Exception exception, string message = "", [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        if(string.IsNullOrEmpty(message))
            Logger.Error(exception, $"[{_asmName}.{_className}.{method}] {_customPrefix}An exception occurred.", args);
        else
            Logger.Error(exception, $"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    /// <summary>
    /// In the case where you need to put a value in 'args' you need to follow this:<br/>
    /// - First situation "You have 1 arg to put inside the 'args' parameter":<br/>
    /// >>> Easy! Just before your arg, put a 'args:' like: Info("Message {arg}", args: myArg);<br/>
    /// - Second situation "You have 2 or + to put inside the 'args' parameter":<br/>
    /// >>> Just before your args, put a 'args: [{ARG1}, {ARG2}, ...]' like: <br/>
    /// >>> Info("Message {arg1}, {arg2}", args: [MyArg, SecondArg]);
    ///</summary>
    public void Debug(string message, [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        Logger.Debug($"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    /// <summary>
    /// In the case where you need to put a value in 'args' you need to follow this:<br/>
    /// - First situation "You have 1 arg to put inside the 'args' parameter":<br/>
    /// >>> Easy! Just before your arg, put a 'args:' like: Info("Message {arg}", args: myArg);<br/>
    /// - Second situation "You have 2 or + to put inside the 'args' parameter":<br/>
    /// >>> Just before your args, put a 'args: [{ARG1}, {ARG2}, ...]' like: <br/>
    /// >>> Info("Message {arg1}, {arg2}", args: [MyArg, SecondArg]);
    ///</summary>
    public void Trace(string message, [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        Logger.Trace($"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    /// <summary>
    /// In the case where you need to put a value in 'args' you need to follow this:<br/>
    /// - First situation "You have 1 arg to put inside the 'args' parameter":<br/>
    /// >>> Easy! Just before your arg, put a 'args:' like: Info("Message {arg}", args: myArg);<br/>
    /// - Second situation "You have 2 or + to put inside the 'args' parameter":<br/>
    /// >>> Just before your args, put a 'args: [{ARG1}, {ARG2}, ...]' like: <br/>
    /// >>> Info("Message {arg1}, {arg2}", args: [MyArg, SecondArg]);
    ///</summary>
    public void Critical(string message, [CallerMemberName] string method = "", params object[] args)
    {
        SimplifyMethodName(ref method);
        Logger.Critical($"[{_asmName}.{_className}.{method}] {_customPrefix}{message}", args);
    }
    
    private void SimplifyMethodName(ref string method)
    {
        if (method == ".ctor")
            method = "Constructor";
    }
}