using Unity.Netcode;
using UnityEngine;

public class MeleeHitboxScript : NetworkBehaviour
{
    public NetworkVariable<ulong> owner;
    public NetworkVariable<int> damage;
    public void FinishMeleeAttack()
    {
        DestroyHitboxRpc();
    }

    [Rpc(SendTo.Server)]
    public void DestroyHitboxRpc()
    {
        Destroy(gameObject);
    }
}
