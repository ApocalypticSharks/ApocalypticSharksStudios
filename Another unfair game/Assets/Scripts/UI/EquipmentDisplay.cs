using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentDisplay : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private EquipmentData data;
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (GameStateManager.Instance.currentGameState)
        {
            case GameState.Shop:
                if (data.isInInventory)
                {
                    GameStateManager.Instance.equipment.Remove(data);
                    PlayerManager.Instance.GetGold(data.data.Cost / 2);
                    Destroy(gameObject);
                }
                else 
                {
                    GameStateManager.Instance.equipment.Add(data);
                    switch (data.data.Type)
                    {
                        case EquipmentType.Helmet:
                            transform.SetParent(UIManager.Instance.HeadContainer);
                            break;
                        case EquipmentType.Armor:
                            transform.SetParent(UIManager.Instance.ArmorContainer);
                            break;
                        case EquipmentType.Legs:
                            transform.SetParent(UIManager.Instance.LegsContainer);
                            break;
                        case EquipmentType.Hand:
                            if (UIManager.Instance.LeftHandContainer.childCount == 0)
                                transform.SetParent(UIManager.Instance.LeftHandContainer);
                            else
                                transform.SetParent(UIManager.Instance.RightHandContainer);
                            break;
                        case EquipmentType.Ring:
                            if (UIManager.Instance.LeftRingContainer.childCount == 0)
                                transform.SetParent(UIManager.Instance.LeftRingContainer);
                            else
                                transform.SetParent(UIManager.Instance.RightRingContainer);
                            break;
                        case EquipmentType.Necklace:
                            transform.SetParent(UIManager.Instance.NeckLaceContainer);
                            break;
                    }
                    PlayerManager.Instance.SpendGold(data.data.Cost);
                    data.isInInventory = true;
                }
                break;
        }
    }

    private void BuyItem()
    {
        
    }
}
