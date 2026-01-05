using System.Numerics;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Assets.Actors;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.Types.Editor.Visual;

public class EditorEntityVisual : ILayerElem, IDisposable
{
    private ImageCache _imageCache => EngineCore.Instance.Managers.ImageCache;
    
    /// <summary>
    /// Entity being represented visually in the editor.
    /// </summary>
    public IEntityDefinition EntityDefinition { get; private set; }
    /// <summary>
    /// Position of the entity on the map editor grid.
    /// </summary>
    public Vector2 Position { get; set; }

    public UnifiedImage? PreviewImage { get; private set; }

    public EditorEntityVisual(IEntityDefinition entityDefinition, Point position)
    {
        EntityDefinition = entityDefinition;
        Position = position;
        ResolvePreview();
    }

    private void ResolvePreview()
    {
        if (string.IsNullOrWhiteSpace(EntityDefinition.SpritePath)) return;

        if (PreviewImage != null)
        {
            _imageCache.Release(PreviewImage.FilePath);
            PreviewImage.Dispose();
        }

        PreviewImage = _imageCache.CreateOrGet(EntityDefinition.SpritePath);
    }

    public void RenderMonoGame(SpriteBatchExtend spriteBatch, Point offset, Microsoft.Xna.Framework.Color? tint = null)
    {
        if (PreviewImage == null) return;

        var drawPosition = new Microsoft.Xna.Framework.Vector2(
            Position.X + offset.X,
            Position.Y + offset.Y
        );

        if (!PreviewImage.HasGraphicsDevice())
            PreviewImage.SetGraphicsDevice(spriteBatch.GraphicsDevice);
        
        var drawColor = tint ?? Microsoft.Xna.Framework.Color.White;
        
        spriteBatch.Draw(PreviewImage.Game, drawPosition, drawColor);
    }
    
    public void Dispose()
    {
        if (PreviewImage == null) return;
        
        _imageCache.Release(PreviewImage.FilePath);
        PreviewImage = null;
    }
}