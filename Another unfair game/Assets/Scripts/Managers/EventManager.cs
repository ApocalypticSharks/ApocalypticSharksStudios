using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    public List<EventSO> allEvents = new List<EventSO>();
    [SerializeField]
    private List<EventSO> eventsForFloor = new List<EventSO>();
    public EventSO currentEvent;

    public Action OnNewEventStarted;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Initialize()
    {
        AddEventsOnFloor();
    }

    public void AddEventsOnFloor()
    {
        foreach (var e in allEvents) 
        {
            if (e.availableOnFloors.Contains(GameStateManager.Instance.currentFloor))
            {
                for (int i = 0; i < e.weight; i++)
                {
                    eventsForFloor.Add(e);
                }
            }
        }
    }

    public void StartNewEvent()
    {
        var randomEvent = UnityEngine.Random.Range(0, eventsForFloor.Count);
        currentEvent = eventsForFloor[randomEvent];
        OnNewEventStarted?.Invoke();
    }
}
