using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillToLearnUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;

    private Ability ability;
    private SkillsToLearnPanel parentPanel;

    public void Initialize(Ability abilityToLearn, SkillsToLearnPanel panel)
    {
        ability = abilityToLearn;
        parentPanel = panel;

        CacheReferences();
        UpdateView();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        parentPanel?.TryLearnAbility(ability);
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

        if (costText == null)
        {
            Transform costTransform = transform.Find("Cost");
            costText = costTransform != null ? costTransform.GetComponent<TMP_Text>() : null;
        }
    }

    private void UpdateView()
    {
        if (ability == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = ability.GetIcon();
            iconImage.enabled = ability.GetIcon() != null;
        }

        if (nameText != null)
        {
            nameText.text = ability.GetAbilityName();
        }

        if (costText != null)
        {
            costText.text = "Level " + ability.GetRequiredLevel();
        }
    }
}
