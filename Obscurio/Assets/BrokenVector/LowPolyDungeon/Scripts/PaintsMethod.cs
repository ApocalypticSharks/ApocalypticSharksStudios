using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PaintsMethod : NetworkBehaviour
{
    [SerializeField] private Transform pin;
    public void MovePin(Vector3 newPosition)
    {
        pin.position = newPosition;
    }
}
