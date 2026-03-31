using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    public TMP_Text eventText;
    [Header("Buttons")]
    public Button option1;
    public TMP_Text option1Text;
    public Button option2;
    public TMP_Text option2Text;
    public Button option3;
    public TMP_Text option3Text;
    private void Awake()
    {
        option1.onClick.AddListener(() => ProcessOption1());
        option2.onClick.AddListener(() => ProcessOption2());
        option3.onClick.AddListener(() => ProcessOption3());
    }
    void Start()
    {
        EventManager.Instance.Initialize();
        EventManager.Instance.OnNewEventStarted += OnNewEventStarted;
        EventManager.Instance.StartNewEvent();
    }

    public void OnNewEventStarted()
    {
        eventText.text = EventManager.Instance.currentEvent.description;
        option1.gameObject.SetActive(EventManager.Instance.currentEvent.choice1IsActive);
        option1Text.text = EventManager.Instance.currentEvent.choice1Text;
        option2.gameObject.SetActive(EventManager.Instance.currentEvent.choice2IsActive);
        option2Text.text = EventManager.Instance.currentEvent.choice2Text;
        option3.gameObject.SetActive(EventManager.Instance.currentEvent.choice3IsActive);
        option3Text.text = EventManager.Instance.currentEvent.choice3Text;
    }

    public void ProcessOption1()
    {
        EventProcessor.ProcessEvent(EventManager.Instance.currentEvent.eventID, 1);
    }
    public void ProcessOption2()
    {
        EventProcessor.ProcessEvent(EventManager.Instance.currentEvent.eventID, 2);
    }
    public void ProcessOption3()
    {
        EventProcessor.ProcessEvent(EventManager.Instance.currentEvent.eventID, 3);
    }
}
