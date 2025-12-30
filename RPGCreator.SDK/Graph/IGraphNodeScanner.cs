namespace RPGCreator.SDK.Graph;

public interface IGraphNodeScanner 
{
    void ScanAssembly(System.Reflection.Assembly assembly);

    void ScanCurrentAssembly();
}