// UIManager.cs
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static UnityEngine.EventSystems.EventTrigger;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject BattlePanel;
    public GameObject ShopPanel;
    public GameObject StateSwitchPanel;
    public Transform ActiveUpgradeContainer;
    public Transform HeadContainer;
    public Transform ArmorContainer;
    public Transform LegsContainer;
    public Transform LeftHandContainer;
    public Transform RightHandContainer;
    public Transform LeftRingContainer;
    public Transform RightRingContainer;
    public Transform NeckLaceContainer;
    public TMP_Text GoldAmount;
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ChangePanel(GameState gameState)
    {
        BattlePanel.SetActive(new GameState[] { GameState.BattleStart,
            GameState.BattlePlayerTurn,
            GameState.BattleEnemyTurn,
            GameState.BattleResults,
            GameState.BattleEnd }.Contains(gameState));
        ShopPanel.SetActive(gameState == GameState.Shop);
        StateSwitchPanel.SetActive(false);
    }

    public void ChangeGoldValue()
    {
        GoldAmount.text = PlayerManager.Instance.gold.ToString();
    }
}