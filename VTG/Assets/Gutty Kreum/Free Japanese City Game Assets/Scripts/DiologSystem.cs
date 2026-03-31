using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [SerializeField] private List<string> _diologLines = new List<string>();
    private int _currentLineIndex = 0;
    public Action callFirstLineAction;
    [SerializeField] private TextMeshProUGUI _textContainer;
    [SerializeField] private GameObject _interactButton;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InteractReadiness(bool isReady)
    { 
        _interactButton.SetActive(isReady);
    }
    public void Talk(QuestSystem questSystem, ref bool isInteracting)
    {
        _textContainer.gameObject.SetActive(true);
        if (_currentLineIndex != _diologLines.Count)
        {
            _textContainer.text = _diologLines[_currentLineIndex];
            _currentLineIndex++;
        }
        else
        { 
            _textContainer.gameObject.SetActive(false);
            questSystem.NextQuest();
            InteractReadiness(false);
            isInteracting = false;
            questSystem.npcIsNear = false;
        }
           

    }
}
