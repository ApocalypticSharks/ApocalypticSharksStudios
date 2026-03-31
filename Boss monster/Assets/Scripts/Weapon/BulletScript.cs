using Unity.Netcode;
using UnityEngine;

public class BulletScript : NetworkBehaviour
{
    [SerializeField] private int bulletSpeed;
    public Vector2 target;
    public NetworkVariable<ulong> owner;
    RaycastHit hit;
    public NetworkVariable<int> damage;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        if (target != null) 
        {
            BulletMovement(target);
        }
    }

    public void BulletMovement(Vector2 target)
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, bulletSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        if (Vector2.Distance(target, transform.position) < 0.001)
            DestroyBulletRpc();
    }

    [Rpc(SendTo.Server)]
    public void DestroyBulletRpc()
    {
        Destroy(gameObject, 0.2f);
    }
}
