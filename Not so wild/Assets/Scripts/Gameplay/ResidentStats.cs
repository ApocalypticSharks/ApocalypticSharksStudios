using UnityEngine;

namespace NotSoWild.Gameplay
{
    public sealed class ResidentStats
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;

        public int Health = 80;
        public int Mood = 70;
        public int Stress = 20;

        public ResidentStats()
        {
        }

        public ResidentStats(int health, int mood, int stress)
        {
            Health = health;
            Mood = mood;
            Stress = stress;
            Clamp();
        }

        public void Clamp()
        {
            Health = Mathf.Clamp(Health, MinValue, MaxValue);
            Mood = Mathf.Clamp(Mood, MinValue, MaxValue);
            Stress = Mathf.Clamp(Stress, MinValue, MaxValue);
        }

        public void Apply(int healthDelta, int moodDelta, int stressDelta)
        {
            Health += healthDelta;
            Mood += moodDelta;
            Stress += stressDelta;
            Clamp();
        }

        public bool ShouldLeaveTown => Mood <= 0 || Health <= 0;
    }
}
