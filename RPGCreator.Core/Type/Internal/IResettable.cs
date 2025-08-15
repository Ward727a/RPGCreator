namespace RPGCreator.Core.Type.Internal;

public interface IResettable<in TDef>
{
    void ResetFrom(TDef def);
}