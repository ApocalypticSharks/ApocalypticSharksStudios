using UnityEngine;

public class SkillsToLearnPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform skillsContainer;
    [SerializeField] private GameObject skillPrefab;

    private Trainer currentTrainer;
    private GameObject currentLearner;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        Hide();
    }

    public void Open(Trainer trainer, GameObject learner)
    {
        currentTrainer = trainer;
        currentLearner = learner;

        panelRoot.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void TryLearnAbility(Ability ability)
    {
        if (currentTrainer == null || currentLearner == null)
        {
            return;
        }

        if (currentTrainer.TryLearnAbility(currentLearner, ability))
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (skillsContainer == null || skillPrefab == null || currentTrainer == null || currentLearner == null)
        {
            return;
        }

        ClearSkills();

        foreach (Ability ability in currentTrainer.GetAvailableAbilities())
        {
            if (!currentTrainer.CanLearnAbility(currentLearner, ability))
            {
                continue;
            }

            GameObject skillObject = Instantiate(skillPrefab, skillsContainer);
            SkillToLearnUI skillUI = skillObject.GetComponent<SkillToLearnUI>();

            if (skillUI == null)
            {
                skillUI = skillObject.AddComponent<SkillToLearnUI>();
            }

            skillUI.Initialize(ability, this);
        }
    }

    private void ClearSkills()
    {
        for (int i = skillsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(skillsContainer.GetChild(i).gameObject);
        }
    }
}
