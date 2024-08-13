using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedGameData
{
    public int coins;
    public int clients;
    public List<int> unlockedCars;
}
public class Progress : MonoBehaviour
{
    public SavedGameData savedGameData;
    public static Progress instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        if (savedGameData.unlockedCars.Count == 0)
            savedGameData.unlockedCars.Add(0);
        Yandex.instance.GetDataToLoad();
    }

    public void SaveData()
    { 
        Yandex.instance.SaveData(savedGameData);
    }

    public void LoadData(string data)
    {
        Yandex.instance.LoadData(data, ref savedGameData);
        UIDataScript.instance.UpdateUIData();
    }

    public void AddReward(int value)
    { 
        savedGameData.coins += value;
        SaveData();
        UIDataScript.instance.UpdateUIData();
    }
}
