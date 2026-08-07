using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfo : MonoBehaviour
{
    [SerializeField] private Attack playerAttack;
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    private Health targetHealth;
    private Mana targetMana;
    private Level targetLevel;

    private void OnEnable()
    {
        playerAttack.OnTargetChanged += SetTarget;
        SetTarget(playerAttack.GetTarget());
    }
    private void OnDisable()
    {
        playerAttack.OnTargetChanged -= SetTarget;
        UnsubscribeFromCurrentTarget();
    }

    private void SetTarget(Transform target)
    {
        UnsubscribeFromCurrentTarget();
        if (target == null)
        {
            targetPanel.SetActive(false);
            return;
        }
        targetHealth = target.GetComponent<Health>();
        targetMana = target.GetComponent<Mana>();
        targetLevel = target.GetComponent<Level>();
        CharacterInfo characterInfo = target.GetComponent<CharacterInfo>();
        targetPanel.SetActive(true);
        if (characterInfo != null)
        {
            nameText.text = characterInfo.GetCharacterName();
            portraitImage.sprite = characterInfo.GetPortrait();
        }
        else
        {
            nameText.text = target.name;
            portraitImage.sprite = null;
        }
        if (targetLevel != null)
        {
            levelText.text = "Lv. " + targetLevel.GetLevel();
        }
        else
        {
            levelText.text = "";
        }
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(targetHealth.GetCurrentHealth(), targetHealth.GetMaxHealth());
        }
        if (targetMana != null)
        {
            targetMana.OnManaChanged += UpdateManaBar;
            UpdateManaBar(targetMana.GetCurrentMana(), targetMana.GetMaxMana());
        }
    }
    private void UnsubscribeFromCurrentTarget()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHealthBar;
            targetHealth = null;
        }
        if (targetMana != null)
        {
            targetMana.OnManaChanged -= UpdateManaBar;
            targetMana = null;
        }
        targetLevel = null;
    }
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthBar.fillAmount = (float)currentHealth / maxHealth;
    }
    private void UpdateManaBar(int currentMana, int maxMana)
    {
        manaBar.fillAmount = (float)currentMana / maxMana;
    }
}
