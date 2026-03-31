using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{

    public float Value;
    public void GetDamage(int damage)
    {
        Value -= damage;
    }
}
