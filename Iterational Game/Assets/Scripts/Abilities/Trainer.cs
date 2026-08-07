using System.Collections.Generic;
using UnityEngine;

public class Trainer : MonoBehaviour, IInteractable
{
    [SerializeField] private List<Ability> availableAbilities;
    [SerializeField] private SkillsToLearnPanel skillsToLearnPanel;

    public IReadOnlyList<Ability> GetAvailableAbilities()
    {
        return availableAbilities;
    }

    public bool CanLearnAbility(GameObject learner, Ability ability)
    {
        if (learner == null || ability == null)
        {
            return false;
        }

        Level level = learner.GetComponent<Level>();
        KnownAbilities knownAbilities = learner.GetComponent<KnownAbilities>();

        if (level == null || knownAbilities == null)
        {
            return false;
        }

        return level.GetLevel() >= ability.GetRequiredLevel() && !knownAbilities.HasAbility(ability);
    }

    public bool TryLearnAbility(GameObject learner, Ability ability)
    {
        if (!CanLearnAbility(learner, ability))
        {
            return false;
        }

        learner.GetComponent<KnownAbilities>().LearnAbility(ability);
        return true;
    }

    public int LearnAvailableAbilities(GameObject learner)
    {
        int learnedCount = 0;

        foreach (Ability ability in availableAbilities)
        {
            if (TryLearnAbility(learner, ability))
            {
                learnedCount++;
            }
        }

        return learnedCount;
    }

    public void Interact(GameObject interactor)
    {
        SkillsToLearnPanel panel = skillsToLearnPanel != null
            ? skillsToLearnPanel
            : FindFirstObjectByType<SkillsToLearnPanel>(FindObjectsInactive.Include);

        if (panel == null)
        {
            Debug.LogWarning("No SkillsToLearnPanel found");
            return;
        }

        panel.Open(this, interactor);
    }
}
