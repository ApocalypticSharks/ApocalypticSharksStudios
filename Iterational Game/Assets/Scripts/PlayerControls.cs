using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask interactionLayer = Physics2D.DefaultRaycastLayers;
    [SerializeField] private float targetSearchRange = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera worldCamera;
    private InputSystem_Actions inputSystem_Actions;
    private Vector2 moveInput; 
    private Rigidbody2D rb;
    private Attack attack;

    private void Awake()
    {
        inputSystem_Actions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        attack = GetComponent<Attack>();
    }

    private void OnEnable()
    {
        inputSystem_Actions.Enable();
        inputSystem_Actions.Player.Move.performed += OnMove;
        inputSystem_Actions.Player.Move.canceled += OnMove;
        inputSystem_Actions.Player.Target.performed += OnTarget;
        inputSystem_Actions.Player.Attack.performed += OnAttack;
    }
    private void OnDisable()
    {
        inputSystem_Actions.Player.Move.performed -= OnMove;
        inputSystem_Actions.Player.Move.canceled -= OnMove;
        inputSystem_Actions.Player.Target.performed -= OnTarget;
        inputSystem_Actions.Player.Attack.performed -= OnAttack;
        inputSystem_Actions.Disable();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryInteractUnderCursor();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void OnTarget(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        SelectNextTarget();
    }

    private void SelectNextTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            targetSearchRange,
            targetLayer
        );
        Transform bestTarget = null;
        float bestDistance = float.MaxValue;
        bool currentTargetStillVisible = false;
        foreach (Collider2D enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;
            Health enemyHealth = enemyTransform.GetComponent<Health>();

            if (enemyHealth != null && enemyHealth.IsDead)
            {
                continue;
            }

            if (enemyTransform == attack.GetTarget())
            {
                currentTargetStillVisible = true;
                continue;
            }
            float distance = (enemyTransform.position - transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = enemyTransform;
            }
        }
        if (bestTarget != null)
        {
            attack.SetTarget(bestTarget);
            Debug.Log("New target: " + bestTarget.name);
            return;
        }
        attack.SetTarget(null);
        Debug.Log("No target");
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }
        attack.AttackTarget();
    }

    private void TryInteractUnderCursor()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
        if (cameraToUse == null)
        {
            Debug.LogWarning("No world camera assigned for interaction");
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector2 mouseWorldPosition = cameraToUse.ScreenToWorldPoint(mouseScreenPosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPosition, interactionLayer);

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                continue;
            }

            interactable.Interact(gameObject);
            return;
        }
    }
}
