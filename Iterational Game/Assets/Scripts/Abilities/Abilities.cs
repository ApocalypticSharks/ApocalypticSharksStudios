using UnityEngine;
[CreateAssetMenu(fileName = "Ability", menuName = "Scriptable Objects/Ability")]
public class Ability : ScriptableObject
{
    [SerializeField] private string abilityName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int requiredLevel;
    [SerializeField] private int manaCost;
    [SerializeField] private float cooldown;
    [SerializeField] private int damage;
    public string GetAbilityName() => abilityName;
    public Sprite GetIcon() => icon;
    public int GetRequiredLevel() => requiredLevel;
    public int GetManaCost() => manaCost;
    public float GetCooldown() => cooldown;
    public int GetDamage() => damage;
}
