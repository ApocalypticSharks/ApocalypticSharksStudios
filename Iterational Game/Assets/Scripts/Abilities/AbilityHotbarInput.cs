using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityHotbarInput : MonoBehaviour
{
    [SerializeField] private AbilityHotbar hotbar;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        if (hotbar == null)
        {
            hotbar = GetComponent<AbilityHotbar>();
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.UseSkill1.performed += OnUseSkill1;
        inputActions.Player.UseSkill2.performed += OnUseSkill2;
        inputActions.Player.UseSkill3.performed += OnUseSkill3;
        inputActions.Player.UseSkill4.performed += OnUseSkill4;
        inputActions.Player.UseSkill5.performed += OnUseSkill5;
        inputActions.Player.UseSkill6.performed += OnUseSkill6;
        inputActions.Player.UseSkill7.performed += OnUseSkill7;
        inputActions.Player.UseSkill8.performed += OnUseSkill8;
        inputActions.Player.UseSkill9.performed += OnUseSkill9;
        inputActions.Player.UseSkill10.performed += OnUseSkill10;
    }

    private void OnDisable()
    {
        inputActions.Player.UseSkill1.performed -= OnUseSkill1;
        inputActions.Player.UseSkill2.performed -= OnUseSkill2;
        inputActions.Player.UseSkill3.performed -= OnUseSkill3;
        inputActions.Player.UseSkill4.performed -= OnUseSkill4;
        inputActions.Player.UseSkill5.performed -= OnUseSkill5;
        inputActions.Player.UseSkill6.performed -= OnUseSkill6;
        inputActions.Player.UseSkill7.performed -= OnUseSkill7;
        inputActions.Player.UseSkill8.performed -= OnUseSkill8;
        inputActions.Player.UseSkill9.performed -= OnUseSkill9;
        inputActions.Player.UseSkill10.performed -= OnUseSkill10;
        inputActions.Disable();
    }

    private void OnUseSkill1(InputAction.CallbackContext context) => hotbar?.UseSlot(0);
    private void OnUseSkill2(InputAction.CallbackContext context) => hotbar?.UseSlot(1);
    private void OnUseSkill3(InputAction.CallbackContext context) => hotbar?.UseSlot(2);
    private void OnUseSkill4(InputAction.CallbackContext context) => hotbar?.UseSlot(3);
    private void OnUseSkill5(InputAction.CallbackContext context) => hotbar?.UseSlot(4);
    private void OnUseSkill6(InputAction.CallbackContext context) => hotbar?.UseSlot(5);
    private void OnUseSkill7(InputAction.CallbackContext context) => hotbar?.UseSlot(6);
    private void OnUseSkill8(InputAction.CallbackContext context) => hotbar?.UseSlot(7);
    private void OnUseSkill9(InputAction.CallbackContext context) => hotbar?.UseSlot(8);
    private void OnUseSkill10(InputAction.CallbackContext context) => hotbar?.UseSlot(9);
}
