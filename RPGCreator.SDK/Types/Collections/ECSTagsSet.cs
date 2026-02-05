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

using System.Runtime.CompilerServices;

namespace RPGCreator.SDK.Types.Collections;

public sealed class ECSTagsSet : ISparseSet
{
    public bool IsTag => true;
    private int[] sparse;
    private int[] entities;
    private int count;

    public int Count => count;
    public ReadOnlySpan<int> EntitiesSpan => new ReadOnlySpan<int>(entities, 0, count);

    public ECSTagsSet(int capacity = 16)
    {
        sparse = new int[capacity];
        entities = new int[capacity];
        Array.Fill(sparse, -1);
        count = 0;
    }

    public void Add(int entityId)
    {
        EnsureCapacity(entityId);
        if (Has(entityId)) return;

        if (count == entities.Length) 
            Grow();

        sparse[entityId] = count;
        entities[count] = entityId;
        count++;
    }

    public void Remove(int entityId)
    {
        if (!Has(entityId)) return;

        int index = sparse[entityId];
        int lastIndex = count - 1;
        int lastEntity = entities[lastIndex];

        entities[index] = lastEntity;
        sparse[lastEntity] = index;

        sparse[entityId] = -1;
        count--;
    }

    public bool Contains(int entityId) => Has(entityId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(int entityId)
    {
        return entityId >= 0 && entityId < sparse.Length && sparse[entityId] != -1;
    }
    
    public IEnumerable<int> ActiveElements()
    {
        for (int i = 0; i < count; i++)
            yield return entities[i];
    }

    private void EnsureCapacity(int entityId)
    {
        if (entityId < sparse.Length) return;

        int oldSize = sparse.Length;
        int newSize = oldSize == 0 ? 16 : oldSize;
        while (newSize <= entityId) newSize *= 2;

        Array.Resize(ref sparse, newSize);
        Array.Fill(sparse, -1, oldSize, newSize - oldSize);
    }

    private void Grow()
    {
        int newSize = entities.Length == 0 ? 16 : entities.Length * 2;
        Array.Resize(ref entities, newSize);
    }
}