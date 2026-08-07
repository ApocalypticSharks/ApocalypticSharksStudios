using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Mana playerMana;
    [SerializeField] private Level playerLevel;
    [SerializeField] private CharacterInfo playerInfo;

    [SerializeField] private Image healthBar;
    [SerializeField] private Image manaBar;
    [SerializeField] private Image experienceBar;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;

    private void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateHealthBar;
        playerMana.OnManaChanged += UpdateManaBar;
        playerLevel.OnExperienceChanged += UpdateExperienceBar;

        UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        UpdateManaBar(playerMana.GetCurrentMana(), playerMana.GetMaxMana());
        UpdateExperienceBar(
            playerLevel.GetExperience(),
            playerLevel.GetExperienceToNextLevel(),
            playerLevel.GetLevel()
        );

        UpdateCharacterInfo();
    }

    private void OnDisable()
    {
        playerHealth.OnHealthChanged -= UpdateHealthBar;
        playerMana.OnManaChanged -= UpdateManaBar;
        playerLevel.OnExperienceChanged -= UpdateExperienceBar; 
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    private void UpdateManaBar(int currentMana, int maxMana)
    {
        manaBar.fillAmount = (float)currentMana / maxMana;
    }
    
    private void UpdateExperienceBar(int experience, int experienceToNextLevel, int level)
    {
        experienceBar.fillAmount = (float)experience / experienceToNextLevel;
        levelText.text = level.ToString();
    }

    private void UpdateCharacterInfo()
    {
        portraitImage.sprite = playerInfo.GetPortrait();
        nameText.text = playerInfo.GetCharacterName();
    }

}
