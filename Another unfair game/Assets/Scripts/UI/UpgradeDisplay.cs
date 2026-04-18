using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeDisplay : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private UpgradeData data;

    private void Awake()
    {
        if (data == null)
            data = GetComponent<UpgradeData>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data?.data == null || UIManager.Instance == null)
            return;
        UIManager.Instance.ShowTooltip(data.data.Name, data.data.Description, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance?.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (GameStateManager.Instance.currentGameState)
        {
            case GameState.Shop:
                if (data.isInInventory)
                {
                    GameStateManager.Instance.upgrades.Remove(data);
                    PlayerManager.Instance.GetGold(data.data.Cost / 2);
                    Destroy(gameObject);
                }
                else 
                {
                    GameStateManager.Instance.upgrades.Add(data);
                    transform.SetParent(UIManager.Instance.ActiveUpgradeContainer);
                    PlayerManager.Instance.SpendGold(data.data.Cost);
                    data.isInInventory = true;
                }
                break;
        }
    }
}
