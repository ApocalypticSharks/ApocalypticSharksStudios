using HUDIndicator;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    [SerializeField] private List<GameObject> _questTargets = new List<GameObject>();
    public GameObject questTarget, targetArrow;
    private int _questIndex = 0;
    public bool npcIsNear, questsComplited, isMusicStay;
    public Animator cameraFade, textFade, hintFade, musicVolume;
    void Start()
    {

    }
    public void NextQuest()
    { 
        _questIndex++;
        questTarget.GetComponent<IndicatorOnScreen>().enabled = false;
        questTarget.GetComponent<IndicatorOffScreen>().enabled = false;
        if (_questIndex < _questTargets.Count)
        {
            questTarget = _questTargets[_questIndex];
            questTarget.GetComponent<IndicatorOnScreen>().enabled = true;
            questTarget.GetComponent<IndicatorOffScreen>().enabled = true;
        }
        else 
        {
            cameraFade.SetTrigger("FadeOut");
            if(!isMusicStay)
                musicVolume.SetTrigger("MusicOff");
            textFade.SetTrigger("FadeIn");
            hintFade.SetTrigger("FadeIn");
            questsComplited = true;
        }
    }

    public void SetInitialQuest()
    {
        questTarget = _questTargets[_questIndex];
        questTarget.GetComponent<IndicatorOnScreen>().enabled = true;
        questTarget.GetComponent<IndicatorOffScreen>().enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent<DialogSystem>(out DialogSystem ds) && collision.gameObject == questTarget)
        {
            npcIsNear = true;
            ds.InteractReadiness(true);
        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent<DialogSystem>(out DialogSystem ds))
        {
            npcIsNear = false;
            ds.InteractReadiness(false);
        }
    }
}
