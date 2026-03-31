using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeDisplay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private UpgradeData data;
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
