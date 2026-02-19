using System.ComponentModel;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;

namespace RPGCreator.SDK.RuntimeService;

/// <summary>
/// A snapshot of data about a layer.
/// </summary>
/// <param name="LayerIndex">The index of the layer.</param>
/// <param name="LayerName">The name of the layer.</param>
/// <param name="VisibleByDefault">Is the layer visible by default?</param>
public record struct LayerData(int LayerIndex, string LayerName, bool VisibleByDefault);

public interface ILayerService : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable, IService
{
    Action<int>? OnLayerSelected { get; set; }
    
    /// <summary>
    /// Is there a selected layer?
    /// </summary>
    bool HasSelectedLayer { get; }

    /// <summary>
    /// Can a layer be selected?<br/>
    /// If false, no layer selecting actions should be allowed (in the case where no layers exist, or <see cref="IMapService.HasLoadedMap"/> is false).
    /// </summary>
    bool CanSelectLayer { get; }
    
    /// <summary>
    /// The index of the currently selected layer.
    /// </summary>
    int CurrentLayerIndex { get; }
    /// <summary>
    /// The total number of layers available. (Starts at 1 => So if there is 0 layers, this will return 0, if there is 1 layer, this will return 1, etc...)
    /// </summary>
    int LayerCount { get; }
    
    /// <summary>
    /// Returns the index of the previous layer.<br/>
    /// If the current layer is the first one, it will return the index of the first layer.
    /// </summary>
    public int PreviousLayerIndex => Math.Max(CurrentLayerIndex - 1, GetFirstLayerIndex());
    
    /// <summary>
    /// Returns the index of the next layer.<br/>
    /// If the current layer is the last one, it will return the index of the last layer.
    /// </summary>
    public int NextLayerIndex => Math.Min(CurrentLayerIndex + 1, GetLastLayerIndex());
    
    /// <summary>
    /// Selects the layer at the given index, or last layer if the index is out of range.<br/>
    /// In the case where the index is out of range, it will also log a warning message.
    /// </summary>
    /// <param name="layerIndex">The layer index to select.</param>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no layers can be loaded/selected (ex: No Map Loaded).</exception>
    void SelectLayer(int layerIndex);
    
    /// <summary>
    /// Selects the next layer (higher index) if possible.<br/>
    /// If the current layer is the last one, it will do nothing.
    /// </summary>
    public void SelectNextLayer() => SelectLayer(NextLayerIndex);
    
    /// <summary>
    /// Selects the previous layer (lower index) if possible.<br/>
    /// If the current layer is the first one, it will do nothing.
    /// </summary>
    public void SelectPreviousLayer() => SelectLayer(PreviousLayerIndex);
    
    /// <summary>
    /// Returns the index of the first layer.
    /// </summary>
    /// <returns>The index of the first layer.</returns>
    public int GetFirstLayerIndex() => 0;
    
    /// <summary>
    /// Returns the index of the last layer.
    /// </summary>
    /// <returns>The index of the last layer.</returns>
    public int GetLastLayerIndex() => LayerCount - 1;
    
    /// <summary>
    /// Tries to add a new layer with the given definition.<br/>
    /// Returns true if the layer was added successfully, false otherwise.
    /// </summary>
    /// <param name="layerDef">The definition of the layer to add.</param>
    /// <returns>
    /// True if the layer was added successfully, false otherwise.
    /// </returns>
    bool TryAddLayer(BaseLayerDef layerDef);
    
    /// <summary>
    /// Tries to remove the layer at the given index.<br/>
    /// Returns true if the layer was removed successfully, false otherwise.
    /// </summary>
    /// <param name="layerIndex">The index of the layer to remove.</param>
    /// <returns>
    /// True if the layer was removed successfully, false otherwise.
    /// </returns>
    bool TryRemoveLayer(int layerIndex);
    
    /// <summary>
    /// Returns the data of the layer at the given index or last layer data if the index is out of range.<br/>
    /// In the case where the index is out of range, it will also log a warning message.
    /// </summary>
    /// <param name="layerIndex">The index of the layer to get the data from.</param>
    /// <returns>The data of the layer at the given index, or last layer data if the index is out of range.</returns>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no layers can be loaded/selected (ex: No Map Loaded).</exception>
    LayerData GetLayerData(int layerIndex);

    /// <summary>
    /// Returns the definition of the currently selected layer.
    /// </summary>
    /// <returns>The definition of the currently selected layer.</returns>
    BaseLayerDef GetSelectedLayer();
}