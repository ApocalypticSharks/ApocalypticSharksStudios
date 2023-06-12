using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMethods : NetworkBehaviour
{
    public event Action<string> statePlayerAction;
    public void ChangeStatePlayerAction(string state)
    {
        statePlayerAction?.Invoke(state);
    }
}
