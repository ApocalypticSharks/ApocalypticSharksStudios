using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct NetworkInventoryItem : INetworkSerializable, IEquatable<NetworkInventoryItem>
{
    public FixedString64Bytes itemId;
    public int quantity;

    public NetworkInventoryItem(string id, int qty)
    {
        itemId = id;
        quantity = qty;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemId);
        serializer.SerializeValue(ref quantity);
    }

    public bool Equals(NetworkInventoryItem other)
    {
        return itemId.Equals(other.itemId) && quantity == other.quantity;
    }

    public override bool Equals(object obj)
    {
        return obj is NetworkInventoryItem other && Equals(other);
    }

    public override int GetHashCode()
    {
        return itemId.GetHashCode() ^ quantity;
    }
}
