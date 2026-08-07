using System.Collections.Generic;
using System;
using UnityEngine;

public class KnownAbilities : MonoBehaviour
{
    private readonly List<Ability> abilities = new();

    public event Action<Ability> OnAbilityLearned;

    public IReadOnlyList<Ability> GetAbilities()
    {
        return abilities;
    }

    public bool HasAbility(Ability ability)
    {
        return abilities.Contains(ability);
    }

    public void LearnAbility(Ability ability)
    {
        if (ability == null || HasAbility(ability))
        {
            return;
        }

        abilities.Add(ability);
        Debug.Log("Learned ability: " + ability.GetAbilityName());
        OnAbilityLearned?.Invoke(ability);
    }
}
