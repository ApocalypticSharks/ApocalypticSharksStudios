using UnityEngine;

public class RewardComponent : MonoBehaviour
{
    [SerializeField] private Reward reward;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += GiveReward;
    }

    private void OnDisable()
    {
        health.OnDied -= GiveReward;
    }

    private void GiveReward(GameObject receiver)
    {
        receiver.GetComponent<Level>()?.AddExperience(GetExperienceReward());
    }

    public Reward GetReward()
    {
        return reward;
    }

    public int GetExperienceReward()
    {
        return reward != null ? reward.GetExperienceReward() : 0;
    }

    public Loot GetLoot()
    {
        return reward != null ? reward.GetLoot() : null;
    }
}
