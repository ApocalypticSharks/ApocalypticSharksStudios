using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventorySelection : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;

    private WeaponScript weapon;
    private ReliquaryCarrier reliquaryCarrier;

    private static readonly Key[] SlotKeys =
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4,
        Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8
    };

    private int selectedIndex = -1;

    public int SelectedIndex => selectedIndex;

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        weapon = GetComponentInChildren<WeaponScript>(true);
        reliquaryCarrier = GetComponent<ReliquaryCarrier>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (IsCarryingReliquary())
            return;

        HandleKeyboardSelection();
        HandleScrollSelection();
    }

    private bool IsCarryingReliquary()
    {
        return reliquaryCarrier != null && reliquaryCarrier.IsCarrying.Value;
    }

    public void OnInventoryChanged()
    {
        int itemCount = inventory.GetItemCount();
        if (itemCount == 0)
            selectedIndex = -1;
        else if (selectedIndex < 0)
            selectedIndex = 0;
        else if (selectedIndex >= itemCount)
            selectedIndex = itemCount - 1;

        TryEquipSelectedSlot();
        inventory.RefreshOwnerUI(selectedIndex);
    }

    public bool TrySelectSlot(int index)
    {
        if (index < 0 || index >= inventory.GetItemCount())
            return false;

        selectedIndex = index;
        TryEquipSelectedSlot();
        inventory.RefreshOwnerUI(selectedIndex);
        return true;
    }

    private void TryEquipSelectedSlot()
    {
        if (!IsOwner || IsCarryingReliquary())
        {
            if (IsCarryingReliquary())
                HolsterWeaponRpc();
            return;
        }

        var items = inventory.GetItems();
        if (selectedIndex < 0 || selectedIndex >= items.Count)
        {
            HolsterWeaponRpc();
            return;
        }

        var itemData = inventory.GetItemData(items[selectedIndex].itemId);
        if (itemData != null && itemData.IsWeapon)
        {
            EquipWeaponRpc(items[selectedIndex].itemId);
        }
        else
        {
            HolsterWeaponRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void EquipWeaponRpc(string itemId)
    {
        EquipWeaponClientRpc(itemId);
    }

    [ClientRpc]
    private void EquipWeaponClientRpc(string itemId)
    {
        if (weapon == null)
            weapon = GetComponentInChildren<WeaponScript>(true);

        var itemData = inventory.GetItemData(itemId);
        if (itemData != null && itemData.IsWeapon)
            weapon.Equip(itemData.WeaponData);
    }

    [Rpc(SendTo.Server)]
    private void HolsterWeaponRpc()
    {
        HolsterWeaponClientRpc();
    }

    [ClientRpc]
    private void HolsterWeaponClientRpc()
    {
        if (weapon == null)
            weapon = GetComponentInChildren<WeaponScript>(true);

        if (weapon != null)
            weapon.Holster();
    }

    private void HandleKeyboardSelection()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        for (int i = 0; i < SlotKeys.Length && i < inventory.maxSlots; i++)
        {
            if (keyboard[SlotKeys[i]].wasPressedThisFrame)
                TrySelectSlot(i);
        }
    }

    private void HandleScrollSelection()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return;

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll > 0f)
            SelectPrevious();
        else if (scroll < 0f)
            SelectNext();
    }

    private void SelectNext()
    {
        int itemCount = inventory.GetItemCount();
        if (itemCount == 0)
            return;

        if (selectedIndex < 0)
            selectedIndex = 0;
        else
            selectedIndex = (selectedIndex + 1) % itemCount;

        TryEquipSelectedSlot();
        inventory.RefreshOwnerUI(selectedIndex);
    }

    private void SelectPrevious()
    {
        int itemCount = inventory.GetItemCount();
        if (itemCount == 0)
            return;

        if (selectedIndex < 0)
            selectedIndex = itemCount - 1;
        else
            selectedIndex = (selectedIndex - 1 + itemCount) % itemCount;

        TryEquipSelectedSlot();
        inventory.RefreshOwnerUI(selectedIndex);
    }
}
