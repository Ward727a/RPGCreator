using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class TileInstance : ITileInstance
{
    public ITileDef Definition { get; private set; }

    public TileInstance()
    {
    }

    public TileInstance(ITileDef tileDef)
    {
        Definition = tileDef;
    }


    public ITileInstance? GetDrawableTile(TileLayerDefinition? layer = null, System.Drawing.Point? position = null)
    {
        return GetCopy();
    }

    public ITileInstance GetCopy()
    {

        return new TileInstance(Definition);
    }

    public void Update(TimeSpan deltaTime)
    {
        throw new NotImplementedException();
    }

    public bool IsEqualTo(ITileInstance other)
    {
        if (other == null) return false;

        return Definition.IsEqualTo(other.Definition);
    }

    public void Clean()
    {
        // No resources to clean up for TileInstance
        // If there were any disposable resources, they would be disposed here.
    }

    public void ResetFrom(ITileDef def, params object[] parameters)
    {
        if (def == null)
        {
            throw new ArgumentNullException(nameof(def), "Tile definition cannot be null.");
        }

        // Reset the tile instance to the provided definition
        Definition = def;
    }
}