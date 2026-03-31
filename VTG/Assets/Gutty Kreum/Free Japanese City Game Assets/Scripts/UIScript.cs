using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public Animator blackScreenAnimtor;
    public QuestSystem questSystem;
    public void BlackScreenFadeIn()
    {
        blackScreenAnimtor.SetTrigger("FadeIn");
        questSystem.SetInitialQuest();
    }
}
