using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityHotbarSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hotkeyText;

    private AbilityHotbar hotbar;
    private int slotIndex;

    public void Initialize(Ability ability, string hotkeyLabel, AbilityHotbar ownerHotbar, int abilitySlotIndex)
    {
        hotbar = ownerHotbar;
        slotIndex = abilitySlotIndex;

        CacheReferences();
        HideTextElements();

        if (iconImage != null)
        {
            iconImage.sprite = ability.GetIcon();
            iconImage.enabled = ability.GetIcon() != null;
        }

        SetCooldown(0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        hotbar?.UseSlot(slotIndex);
    }

    public void SetCooldown(float normalizedCooldown)
    {
        if (cooldownImage == null)
        {
            return;
        }

        cooldownImage.fillAmount = normalizedCooldown;
        cooldownImage.enabled = normalizedCooldown > 0f;
    }

    private void CacheReferences()
    {
        if (iconImage == null)
        {
            Transform iconTransform = transform.Find("Icon");
            iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;
        }

        if (nameText == null)
        {
            Transform nameTransform = transform.Find("Name");
            nameText = nameTransform != null ? nameTransform.GetComponent<TMP_Text>() : null;
        }

        if (hotkeyText == null)
        {
            Transform costTransform = transform.Find("Cost");
            hotkeyText = costTransform != null ? costTransform.GetComponent<TMP_Text>() : null;
        }
    }

    private void HideTextElements()
    {
        if (nameText != null)
        {
            nameText.gameObject.SetActive(false);
        }

        if (hotkeyText != null)
        {
            hotkeyText.gameObject.SetActive(false);
        }
    }
}
