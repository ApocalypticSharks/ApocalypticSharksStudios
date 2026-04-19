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
        string title = data.data.Name;
        string body = data.data.Description;
        if (GameStateManager.Instance != null && GameStateManager.Instance.currentGameState == GameState.Shop)
        {
            if (data.isInInventory)
            {
                title = $"{data.data.Name} (Lv.{data.EffectiveLevel})";
                body += $"\nSell: {UpgradeData.GetSellPrice(data.data, data.level)} gold";
            }
            else
            {
                title = $"{data.data.Name} → Lv.{data.level}";
                body += $"\nPrice: {UpgradeData.GetBuyPrice(data.data, data.level)} gold";
            }
        }
        else
            title = $"{data.data.Name} (Lv.{data.EffectiveLevel})";

        UIManager.Instance.ShowTooltip(title, body, eventData.position);
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
                    PlayerManager.Instance.GetGold(UpgradeData.GetSellPrice(data.data, data.level));
                    Destroy(gameObject);
                }
                else
                {
                    int price = UpgradeData.GetBuyPrice(data.data, data.level);
                    if (PlayerManager.Instance.gold < price)
                        return;

                    UpgradeData existing = UpgradeData.FindOwnedUpgrade(data.data);
                    if (existing != null)
                    {
                        PlayerManager.Instance.SpendGold(price);
                        existing.level = data.level;
                        Destroy(gameObject);
                    }
                    else
                    {
                        PlayerManager.Instance.SpendGold(price);
                        GameStateManager.Instance.upgrades.Add(data);
                        transform.SetParent(UIManager.Instance.ActiveUpgradeContainer);
                        data.isInInventory = true;
                    }
                }
                break;
        }
    }
}
