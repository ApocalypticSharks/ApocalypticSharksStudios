using UnityEngine;

[CreateAssetMenu(fileName = "Reward", menuName = "Scriptable Objects/Reward")]
public class Reward : ScriptableObject
{
    [SerializeField] private int experienceReward;
    [SerializeField] Loot loot;

    public int GetExperienceReward()
    {
        return experienceReward;
    }
    public Loot GetLoot()
    {
        return loot;
    }
}
