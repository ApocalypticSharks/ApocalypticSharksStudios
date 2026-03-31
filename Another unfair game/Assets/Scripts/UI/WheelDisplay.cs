using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelDisplay : MonoBehaviour
{
    public WheelSegmentData segmentData;
    public bool isActiveToClick;
    public bool isCompleted;

    public void ApplyData()
    {
        gameObject.GetComponent<Image>().sprite = segmentData.icon;
    }

    //public void OnPointerClick(PointerEventData eventData) 
    //{
                
    //    if (!isCompleted && isActiveToClick && eventData.button == PointerEventData.InputButton.Left) 
    //    {
    //        isCompleted = true;
    //        switch (segmentData.type)
    //        {                
    //            case SegmentType.Battle:
    //                GameManager.Instance.ChangeGameState(GameState.Battle);
    //            break;
    //            case SegmentType.Shop:
    //                GameManager.Instance.ChangeGameState(GameState.Shop);
    //                break;
    //            case SegmentType.Event:
    //                GameManager.Instance.ChangeGameState(GameState.Event);
    //                break;
    //            case SegmentType.RestSite:
    //                GameManager.Instance.ChangeGameState(GameState.RestSite);
    //                break;
    //        }
    //    }
    //}
}
