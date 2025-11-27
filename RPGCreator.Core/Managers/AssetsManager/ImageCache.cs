using RPGCreator.Core.Types;

namespace RPGCreator.Core.Managers.AssetsManager;

public class ImageCache
{
    private readonly Dictionary<string, UnifiedImage> images = new();
    private readonly Dictionary<string, int> refCounts = new();

    public UnifiedImage CreateOrGet(string imagePath)
    {
        if (!images.ContainsKey(imagePath))
        {
            var img = new UnifiedImage(imagePath);
            images[imagePath] = img;
        }
        refCounts[imagePath] = refCounts.GetValueOrDefault(imagePath, 0) + 1;
        return images[imagePath];
    }
    
    public void Release(string imagePath)
    {
        if (refCounts.ContainsKey(imagePath))
        {
            refCounts[imagePath]--;
            if (refCounts[imagePath] <= 0)
            {
                if(images.TryGetValue(imagePath, out var img))
                {
                    img.Dispose();
                }
                
                refCounts.Remove(imagePath);
                images.Remove(imagePath);
            }
        }
    }
}