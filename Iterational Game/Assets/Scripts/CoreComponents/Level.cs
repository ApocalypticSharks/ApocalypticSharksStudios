using UnityEngine;
using System;

public class Level : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int experience = 0;
    [SerializeField] private int experienceToNextLevel = 100;

    public event Action<int, int, int> OnExperienceChanged;
    
    public void AddExperience(int amount)
    {
        experience += amount;

        if (experience >= experienceToNextLevel)
        {
            var overflow = experience - experienceToNextLevel;
            LevelUp();
            AddExperience(overflow);
            return;
        }

        OnExperienceChanged?.Invoke(experience, experienceToNextLevel, level);
    }
    private void LevelUp()
    {
        level++;
        experience = 0;
        experienceToNextLevel *= 2;
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetExperience()
    {
        return experience;
    }
    
    public int GetExperienceToNextLevel()
    {
        return experienceToNextLevel;
    }
}
