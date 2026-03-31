using UnityEngine.UI;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

public class WheelUI : MonoBehaviour
{
    [SerializeField]
    private Button wheelButton;
    [SerializeField]
    public List<RectTransform> wheelSegments;
    private void Start()
    {
        wheelButton.onClick.AddListener(() => WheelManager.Instance.WheelSpin());
        WheelManager.Instance.onWheelInitialized += OnWheelInitialized;
        WheelManager.Instance.onWheelSpinned += OnWheelSpinned;
        WheelManager.Instance.Initialize();
    }
    private void OnEnable()
    {
        foreach (var segment in wheelSegments)
        {
            segment.GetComponent<WheelDisplay>().isActiveToClick = false;
        }
        wheelButton.interactable = true;
    }
    private void OnWheelSpinned()
    {
        wheelButton.interactable = false;
        int selectedIndex = WheelManager.Instance.selectedSegments[0]; // Assuming the first index is the main selected segment
        GetComponent<WheelAnimation>().RotateToSelectedSegment(selectedIndex);
        foreach (var index in WheelManager.Instance.selectedSegments)
        {
            wheelSegments[index].GetComponent<Image>().color = Color.white;
            wheelSegments[index].GetComponent<WheelDisplay>().isActiveToClick = true;
        }
    }

    private void OnWheelInitialized()
    {
        for (int i = 0; i < wheelSegments.Count; i++)
        {
            wheelSegments[i].GetComponent<WheelDisplay>().segmentData = WheelManager.Instance.currentWheelSegments[i];
            wheelSegments[i].GetComponent<WheelDisplay>().ApplyData();
        }
    }
}
