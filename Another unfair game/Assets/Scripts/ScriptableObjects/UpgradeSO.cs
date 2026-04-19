using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Blackjack Rogue/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public const int MaxLevel = 5;

    [SerializeField]
    public string Name;
    [SerializeField]
    public string Description;
    [SerializeField]
    public EffectType EffectType;
    [SerializeField]
    public int Cost;
    [SerializeField]
    public int Rarity;
    [SerializeField]
    public Sprite Sprite;

    [Header("Effects")]
    public List<EffectStruct> onPlayEffects;
    public List<EffectStruct> onWinEffects;
    public List<EffectStruct> onBustEffects;
    public List<EffectStruct> onLoseEffects;
    public List<EffectStruct> onDiscardEffects;
}
