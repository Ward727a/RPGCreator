using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers;

public interface IMapLayerDef<TLayerElement> : IMapLayerDef
{
    event EventHandler<(Vector2, TLayerElement)>? ElementAdded;
    event EventHandler<(Vector2,TLayerElement?)>? ElementRemoved;
    ReadOnlyDictionary<Vector2, TLayerElement> Elements { get; }

    /// <summary>
    /// Add a new element of type <see cref="TLayerElement"/> to the layer at the specified location.
    /// </summary>
    /// <param name="element">An element of type <see cref="TLayerElement"/> to add.</param>
    /// <param name="location">The location where the element should be added.</param>
    public void AddElement(TLayerElement element, Vector2 location);
    /// <summary>
    /// Try to add a new element of type <see cref="TLayerElement"/> to the layer at the specified location.<br/>
    /// This method will return false if the element could not be added, for example, if the location is already occupied by another element.<br/>
    /// If the element is successfully added, it will trigger the <see cref="ElementAdded"/> event with the location of the added element.
    /// </summary>
    /// <param name="element">An element of type <see cref="TLayerElement"/> to add.</param>
    /// <param name="location">The location where the element should be added.</param>
    /// <returns>
    /// True if the element was successfully added; otherwise, false.
    /// </returns>
    public bool TryAddElement(TLayerElement element, Vector2 location);
    /// <summary>
    /// Remove an element of type <see cref="TLayerElement"/> from the layer at the specified location.<br/>
    /// This method will trigger the <see cref="ElementRemoved"/> event with the location and the removed element.<br/>
    /// If the location does not contain an element, it will return null.
    /// </summary>
    /// <param name="location">The location of the element to remove.</param>
    /// <returns>
    /// The removed element if it exists at the specified location; otherwise, null.
    /// </returns>
    public TLayerElement? RemoveElement(Vector2 location);
    /// <summary>
    /// Try to remove an element of type <see cref="TLayerElement"/> from the layer at the specified location.<br/>
    /// This method will return false if the element could not be removed, for example, if the location does not contain an element.<br/>
    /// If the element is successfully removed, it will trigger the <see cref="ElementRemoved"/> event with the location and the removed element.
    /// </summary>
    /// <param name="location">The location of the element to remove.</param>
    /// <param name="removedElement">When this method returns, contains the removed element if it exists at the specified location; otherwise, null.</param>
    /// <returns>
    /// True if the element was successfully removed; otherwise, false.
    /// </returns>
    public bool TryRemoveElement(Vector2 location,[NotNullWhen(true)] out TLayerElement? removedElement);
    /// <summary>
    /// Try to remove a specific element of type <see cref="TLayerElement"/> from the layer.<br/>
    /// This method will return false if the element could not be removed, for example, if the element does not exist in the layer.<br/>
    /// If the element is successfully removed, it will trigger the <see cref="ElementRemoved"/> event with the location of the removed element.<br/>
    /// The location of the removed element will be returned in the <paramref name="removedLocation"/> parameter.<br/>
    /// If the element is not found, <paramref name="removedLocation"/> will be set to null.
    /// </summary>
    /// <param name="element">An element of type <see cref="TLayerElement"/> to remove.</param>
    /// <param name="removedLocation">The location of the removed element if it exists; otherwise, null.</param>
    /// <returns>
    /// True if the element was successfully removed; otherwise, false.
    /// </returns>
    /// <remarks>
    /// Depending on the implementation, this method could be less efficient than removing by location, as it may require iterating through all elements to find the specified element.<br/>
    /// Therefore, it is recommended to use this method only when you have a reference to the element you want to remove, rather than its location.<br/>
    /// If you need to remove an element by its location, use the <see cref="TryRemoveElement(Point, out TLayerElement?)"/> method instead.
    /// </remarks>
    public bool TryRemoveElement(TLayerElement element, [NotNullWhen(true)] out Vector2? removedLocation);
    /// <summary>
    /// Retrieve an element of type <see cref="TLayerElement"/> at the specified location.<br/>
    /// If the location does not contain an element, it will return null.
    /// </summary>
    /// <param name="location">The location of the element to retrieve.</param>
    /// <returns>
    /// The element if it exists at the specified location; otherwise, null.
    /// </returns>
    public TLayerElement? GetElement(Vector2 location);
    /// <summary>
    /// Try to retrieve an element of type <see cref="TLayerElement"/> at the specified location.<br/>
    /// This method will return false if the element does not exist at the specified location.<br/>
    /// If the element exists, it will be returned in the <paramref name="element"/> parameter.<br/>
    /// If the element does not exist, <paramref name="element"/> will be set to null.
    /// </summary>
    /// <param name="location">The location of the element to retrieve.</param>
    /// <param name="element">The element if it exists at the specified location; otherwise, null.</param>
    /// <returns>
    /// True if the element was found; otherwise, false.
    /// </returns>
    public bool TryGetElement(Vector2 location, [NotNullWhen(true)] out TLayerElement? element);
    /// <summary>
    /// Check if the layer contains an element at the specified location.<br/>
    /// This method will return true if there is an element of type <see cref="TLayerElement"/> at the specified location.<br/>
    /// If there is no element at the specified location, it will return false.<br/>
    /// </summary>
    /// <param name="location">The location to check for an element.</param>
    /// <returns>
    /// True if there is an element at the specified location; otherwise, false.
    /// </returns>
    public bool HasElement(Vector2 location);
    
    public void ClearElements();
}

public interface IMapLayerDef : IHasUniqueId, ISerializable, IDeserializable
{
    string Name { get; set; }
    int ZIndex { get; }
    bool VisibleByDefault { get; set; }
}