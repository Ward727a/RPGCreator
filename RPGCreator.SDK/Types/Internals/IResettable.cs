namespace RPGCreator.SDK.Types.Internals;

public interface IResettable<in TDef>
{
    // <summary>
    // Resets the instance's state based on the provided definition.
    // We can also give it a list of parameters to customize the reset process.
    // </summary>
    void ResetFrom(TDef def, params object[] parameters)
    {
    }
    
    void Reset(params object[] parameters)
    {
    }
}