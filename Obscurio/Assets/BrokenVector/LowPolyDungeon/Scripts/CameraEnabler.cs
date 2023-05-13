using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraEnabler : NetworkBehaviour
{
    [SerializeField] private Camera camera;
    private void Start()
    {
        if (IsLocalPlayer)
        {
            camera.enabled = true;
        }
    }
}
