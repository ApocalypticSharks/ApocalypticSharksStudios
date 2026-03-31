using Unity.UI;
using UnityEngine;
using UnityEngine.UI;

public class GameStateSwitchManager : MonoBehaviour
{
    public Button newBattle;
    public Button toShop;

    private void Awake()
    {
        newBattle.onClick.AddListener(() => GameStateManager.Instance.MoveToNextGameState(GameState.BattleStart));
        toShop.onClick.AddListener(() => GameStateManager.Instance.MoveToNextGameState(GameState.Shop));
    }
}
