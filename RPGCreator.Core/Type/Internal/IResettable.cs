namespace RPGCreator.Core.Type.Internal;

public interface IResettable<in TDef>
{
    // <summary>
    // Resets the instance's state based on the provided definition.
    // We can also give it a list of parameters to customize the reset process.
    // </summary>
    void ResetFrom(TDef def, params object[] parameters);
}