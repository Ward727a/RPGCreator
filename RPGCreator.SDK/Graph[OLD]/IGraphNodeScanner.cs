namespace RPGCreator.SDK.Graph;

public interface IGraphNodeScanner  : IService
{
    void ScanAssembly(System.Reflection.Assembly assembly);

    void ScanCurrentAssembly();
}