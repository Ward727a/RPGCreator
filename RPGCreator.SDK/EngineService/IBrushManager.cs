// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

/// <summary>
/// The brush manager service.<br/>
/// Manages brushes used for painting or drawing within the engine/editor.<br/>
/// Be aware that the service is different from the brush state.<br/>
/// <br/>
/// The brush state manage the 'memory' of the brush system, such as the currently selected brush, preview state, etc...<br/>
/// While the brush manager is responsible for 'using' the brush state and providing methods to manipulate brushes.
/// </summary>
public interface IBrushManager : IService
{
    /// <summary>
    /// The current state of the brush manager.<br/>
    /// This includes information such as the selected brush and preview state.<br/>
    /// <br/>
    /// In fact, this is just a reference to <see cref="EngineStates.BrushState"/>.<br/>
    /// </summary>
    IBrushState State { get; }

    /// <summary>
    /// Gets a brush by its URN.<br/>
    /// Returns null if the brush does not exist.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to retrieve.</param>
    /// <returns>
    /// The brush info if found; otherwise, null.
    /// </returns>
    public IBrushInfo? GetBrush(URN brushUrn);

    /// <summary>
    /// Tries to get a brush by its URN.<br/>
    /// Returns true if the brush exists, false otherwise.
    /// </summary>
    /// <param name="brushName">The URN of the brush to retrieve.</param>
    /// <param name="brush">The output brush info if found; otherwise, null.</param>
    /// <returns>
    /// True if the brush was found; otherwise, false.
    /// </returns>
    public bool TryGetBrush(URN brushName, [NotNullWhen(true)] out IBrushInfo? brush);
    
    /// <summary>
    /// Adds a brush to the manager.<br/>
    /// If a brush with the same URN already exists and <paramref name="overwriteIfExists"/> is false, the method does nothing.<br/>
    /// If <paramref name="overwriteIfExists"/> is true, the existing brush will be replaced.
    /// </summary>
    /// <param name="brush">The brush info to add.</param>
    /// <param name="overwriteIfExists">Whether to overwrite the existing brush if it already exists.</param>
    public void AddBrush(IBrushInfo brush, bool overwriteIfExists = false);
    
    /// <summary>
    /// Tries to add a brush to the manager.<br/>
    /// Returns true if the brush was added successfully, false if a brush with the same URN already exists.
    /// </summary>
    /// <param name="brush">The brush info to add.</param>
    /// <returns>
    /// True if the brush was added successfully; otherwise, false.
    /// </returns>
    public bool TryAddBrush(IBrushInfo brush);
    
    /// <summary>
    /// Removes a brush from the manager.
    /// </summary>
    /// <param name="brush">The brush info to remove.</param>
    public void RemoveBrush(IBrushInfo brush);
    
    /// <summary>
    /// Tries to remove a brush by its URN.<br/>
    /// Returns true if the brush was removed successfully, false if the brush does not exist.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to remove.</param>
    /// <returns>
    /// True if the brush was removed successfully; otherwise, false.
    /// </returns>
    public bool TryRemoveBrush(URN brushUrn);
    
    /// <summary>
    /// Checks if a brush exists by its URN.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to check.</param>
    /// <returns>
    /// True if the brush exists; otherwise, false.
    /// </returns>
    public bool HasBrush(URN brushUrn);
    
    /// <summary>
    /// Selects a brush by its URN.<br/>
    /// Returns true if the brush was selected successfully, false if the brush does not exist.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to select.</param>
    /// <returns>
    /// True if the brush was selected successfully; otherwise, false.
    /// </returns>
    public bool SelectBrush(URN brushUrn);
    
    /// <summary>
    /// Selects a brush directly.<br/>
    /// Sets the selected brush in the brush state to the provided brush.<br/>
    /// This forces the selection to the given brush without any checks.
    /// </summary>
    /// <param name="brush">The brush info to select.</param>
    public void SelectBrush(IBrushInfo brush);
    
    /// <summary>
    /// Gets the currently selected brush.<br/>
    /// Returns null if no brush is selected.
    /// </summary>
    /// <returns>
    /// The currently selected brush info if any; otherwise, null.
    /// </returns>
    public IBrushInfo? GetSelectedBrush();

    /// <summary>
    /// Verifies if a brush supports a specific feature type.<br/>
    /// This method checks if the brush identified by the given URN implements the specified brush feature interface.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to check.</param>
    /// <typeparam name="BrushFeature">The type of brush feature to check for.</typeparam>
    /// <returns>
    /// True if the brush supports the specified feature; otherwise, false.
    /// </returns>
    public bool IsBrushAbleTo<BrushFeature>(URN brushUrn) where BrushFeature : IBrushFeature;
    
    /// <summary>
    /// Retrieves all features of a specific type from a brush.<br/>
    /// This method returns an enumerable of brush features of the specified type that are implemented by the brush identified by the given URN.
    /// </summary>
    /// <param name="brushUrn">The URN of the brush to retrieve features from.</param>
    /// <returns>
    /// An enumerable of brush features of the specified type.
    /// </returns>
    public IEnumerable<IBrushFeature> GetBrushFeatures(URN brushUrn);
    
    /// <summary>
    /// Retrieves all registered brushes' URNs.<br/>
    /// Returns an enumerable of URNs representing all brushes managed by the brush manager.
    /// </summary>
    /// <returns>
    /// An enumerable of URNs for all registered brushes.
    /// </returns>
    public IEnumerable<URN> GetAllBrushes();
    
    /// <summary>
    /// Draws with the selected brush at the specified position.<br/>
    /// This method uses the current brush selected in the brush state to perform the drawing action.
    /// </summary>
    /// <param name="at">The position where to draw.</param>
    public void DrawAt(Vector2 at);
    
    /// <summary>
    /// Previews the brush at the specified position.<br/>
    /// This method shows a preview of the brush effect at the given position without actually applying it.
    /// </summary>
    /// <param name="at">The position where to preview the brush.</param>
    public void PreviewAt(Vector2 at);
    public void ClearPreview();
    public Vector2 NormalizedPositionToTile(Vector2 position);
}