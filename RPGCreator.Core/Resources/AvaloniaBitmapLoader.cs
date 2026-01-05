using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Resources;

namespace RPGCreator.Core.Resources;

public class AvaloniaBitmapLoader : IResourceLoader 
{
    public object Load(string path)
    {
        return new Avalonia.Media.Imaging.Bitmap(path);
    }
}