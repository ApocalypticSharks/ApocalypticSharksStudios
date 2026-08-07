using UnityEngine;
using System;

public class Mana : MonoBehaviour
{
    [SerializeField] private int maxMana = 100;
    [SerializeField] private int currentMana;

    public event Action<int, int> OnManaChanged;

    private void Awake()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void UseMana(int mana)
    {
        currentMana -= mana;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public bool CanUseMana(int mana)
    {
        return currentMana >= mana;
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public int GetMaxMana()
    {
        return maxMana;
    }
}