using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventSO", menuName = "Blackjack Rogue/Event")]
public class EventSO : ScriptableObject
{
    [Header("Event Identity")]
    public EventID eventID;
    public string eventName;
    [TextArea(3, 5)]
    public string description;

    [Header("Visuals")]
    public Sprite backgroundImage;

    [Header("Availability")]
    public List<FloorSO> availableOnFloors = new List<FloorSO>();
    public int weight = 100; // Вес для случайного выбора

    [Header("Choices")]
    public bool choice1IsActive;
    public string choice1Text;
    public bool choice2IsActive;
    public string choice2Text;
    public bool choice3IsActive;
    public string choice3Text;

    [Header("Repeatability")]
    public bool isOneTime = false;
    public bool isRepeatable = true;
    public int cooldownFloors = 3;
}

public enum EventID
{
    Collector,
    CardTransformer
}
