using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationDriver : NetworkBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int ArmedHash = Animator.StringToHash("Armed");
    private static readonly int KnifeHash = Animator.StringToHash("Knife");

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private WeaponScript weapon;

    private Vector3 lastPosition;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody2D>();
        if (weapon == null)
            weapon = GetComponentInChildren<WeaponScript>(true);

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, GetMovementSpeed());

        bool hasWeapon = weapon != null && weapon.IsEquipped;
        bool hasKnife = hasWeapon && weapon.IsMeleeWeapon;
        animator.SetBool(ArmedHash, hasWeapon && !hasKnife);
        animator.SetBool(KnifeHash, hasKnife);
    }

    private void LateUpdate()
    {
        if (weapon != null && weapon.IsEquipped)
            weapon.RefreshEquippedVisual();
    }

    private float GetMovementSpeed()
    {
        float speed = 0f;
        if (playerRigidbody != null)
            speed = playerRigidbody.linearVelocity.magnitude;

        if (speed <= 0.01f && Time.deltaTime > 0f)
            speed = ((transform.position - lastPosition).magnitude / Time.deltaTime);

        lastPosition = transform.position;
        return speed;
    }
}
