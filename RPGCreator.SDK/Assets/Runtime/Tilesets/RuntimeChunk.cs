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

using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.SDK.Assets.Runtime.Tilesets;

[System.Runtime.CompilerServices.InlineArray(LayerChunk.ChunkSize * LayerChunk.ChunkSize)] // 1024 by default
public struct ChunkDataBuffer<T> where T : struct
{
    private T _element;
}

[System.Runtime.CompilerServices.InlineArray(16)]
public struct PresenceMaskBuffer
{
    private ulong _element;
}

public struct RuntimeChunk<T>() : ISlabItem where T : struct, ILayerElem
{
    public int? BlockPointerIndex { get; set; }

    public ChunkDataBuffer<T> Data = new();
    public PresenceMaskBuffer PresenceMask = new();

    public T this[int tileXLocalToChunk, int tileYLocalToChunk]
    {
        get
        {
#if DEBUG
            if (tileXLocalToChunk < 0 || tileXLocalToChunk >= 32 || tileYLocalToChunk < 0 || tileYLocalToChunk >= 32)
                throw new IndexOutOfRangeException(
                    $"Local coordinates ({tileXLocalToChunk},{tileYLocalToChunk}) out of range for 32x32 chunk.");
#endif
            return Data[tileYLocalToChunk * LayerChunk.ChunkSize + tileXLocalToChunk];
        }
        set => Data[tileYLocalToChunk * LayerChunk.ChunkSize + tileXLocalToChunk] = value;
    }

    public T this[int index]
    {
        get => Data[index];
        set
        {
#if DEBUG
            if (index < 0 || index >= LayerChunk.ChunkSize * LayerChunk.ChunkSize)
                throw new IndexOutOfRangeException($"Index {index} is out of range for this chunk.");
#endif
            Data[index] = value;
        }
    }
}