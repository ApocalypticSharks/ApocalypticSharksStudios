using System.Collections.Generic;
using UnityEngine;

public class AbilityHotbar : MonoBehaviour
{
    private const int MaxSlots = 10;

    [SerializeField] private KnownAbilities knownAbilities;
    [SerializeField] private Mana mana;
    [SerializeField] private Attack attack;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private float abilityRange = 5f;

    private readonly List<Ability> slottedAbilities = new();
    private readonly List<AbilityHotbarSlotUI> slotUIs = new();
    private readonly float[] cooldowns = new float[MaxSlots];

    private void Awake()
    {
        if (knownAbilities == null)
        {
            knownAbilities = GetComponent<KnownAbilities>();
        }

        if (mana == null)
        {
            mana = GetComponent<Mana>();
        }

        if (attack == null)
        {
            attack = GetComponent<Attack>();
        }
    }

    private void OnEnable()
    {
        if (knownAbilities != null)
        {
            knownAbilities.OnAbilityLearned += AddAbility;
        }

        Rebuild();
    }

    private void OnDisable()
    {
        if (knownAbilities != null)
        {
            knownAbilities.OnAbilityLearned -= AddAbility;
        }
    }

    private void Update()
    {
        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (cooldowns[i] <= 0f)
            {
                continue;
            }

            cooldowns[i] = Mathf.Max(0f, cooldowns[i] - Time.deltaTime);
            UpdateSlotCooldown(i);
        }
    }

    public void UseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slottedAbilities.Count)
        {
            return;
        }

        if (cooldowns[slotIndex] > 0f)
        {
            return;
        }

        Ability ability = slottedAbilities[slotIndex];

        if (mana != null && !mana.CanUseMana(ability.GetManaCost()))
        {
            Debug.Log("Not enough mana for " + ability.GetAbilityName());
            return;
        }

        if (!ApplyAbilityEffect(ability))
        {
            return;
        }

        mana?.UseMana(ability.GetManaCost());
        cooldowns[slotIndex] = ability.GetCooldown();
        UpdateSlotCooldown(slotIndex);
    }

    private void Rebuild()
    {
        ClearSlots();

        if (knownAbilities == null)
        {
            return;
        }

        foreach (Ability ability in knownAbilities.GetAbilities())
        {
            AddAbility(ability);
        }
    }

    private void AddAbility(Ability ability)
    {
        if (ability == null || slottedAbilities.Contains(ability) || slottedAbilities.Count >= MaxSlots)
        {
            return;
        }

        int slotIndex = slottedAbilities.Count;
        slottedAbilities.Add(ability);
        CreateSlotUI(ability, slotIndex);
    }

    private void CreateSlotUI(Ability ability, int slotIndex)
    {
        if (slotsContainer == null || skillPrefab == null)
        {
            return;
        }

        GameObject slotObject = Instantiate(skillPrefab, slotsContainer);
        AbilityHotbarSlotUI slotUI = slotObject.GetComponent<AbilityHotbarSlotUI>();

        if (slotUI == null)
        {
            slotUI = slotObject.AddComponent<AbilityHotbarSlotUI>();
        }

        slotUI.Initialize(ability, GetHotkeyLabel(slotIndex), this, slotIndex);
        slotUIs.Add(slotUI);
    }

    private bool ApplyAbilityEffect(Ability ability)
    {
        if (ability.GetDamage() <= 0)
        {
            Debug.Log("Used ability: " + ability.GetAbilityName());
            return true;
        }

        Transform target = attack != null ? attack.GetTarget() : null;

        if (target == null)
        {
            Debug.Log("No target for " + ability.GetAbilityName());
            return false;
        }

        if (Vector2.Distance(transform.position, target.position) > abilityRange)
        {
            Debug.Log("Target is too far for " + ability.GetAbilityName());
            return false;
        }

        Health targetHealth = target.GetComponent<Health>();

        if (targetHealth == null || targetHealth.IsDead)
        {
            return false;
        }

        targetHealth.TakeDamage(ability.GetDamage(), gameObject);
        Debug.Log("Used ability: " + ability.GetAbilityName());
        return true;
    }

    private void UpdateSlotCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotUIs.Count)
        {
            return;
        }

        Ability ability = slottedAbilities[slotIndex];
        float normalizedCooldown = ability.GetCooldown() > 0f ? cooldowns[slotIndex] / ability.GetCooldown() : 0f;
        slotUIs[slotIndex].SetCooldown(normalizedCooldown);
    }

    private void ClearSlots()
    {
        slottedAbilities.Clear();
        slotUIs.Clear();

        if (slotsContainer == null)
        {
            return;
        }

        for (int i = slotsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slotsContainer.GetChild(i).gameObject);
        }
    }

    private string GetHotkeyLabel(int slotIndex)
    {
        return slotIndex == 9 ? "0" : (slotIndex + 1).ToString();
    }
}
