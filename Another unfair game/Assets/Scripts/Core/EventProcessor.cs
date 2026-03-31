using UnityEngine;

public static class EventProcessor
{
    public static void ProcessEvent(EventID eventId, int option)
    {
        Debug.Log($"Event {eventId}. Selected option {option}");
    }
}
