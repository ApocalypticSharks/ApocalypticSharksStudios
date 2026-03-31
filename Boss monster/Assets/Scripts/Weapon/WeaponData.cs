using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
    [SerializeField] private int ammoAmount, fireSpread, fireRate, meleeRate, damage, reloadSpeed, meleeDamage;
    [SerializeField] private string name;
    [SerializeField] private Sprite sprite;
    [SerializeField] private bool auto, isMelee;

    public string Name
        { get { return name; } }
    public Sprite Sprite
        { get { return sprite; } }
    public int AmmoAmount
        { get { return ammoAmount; } }
    public int FireSpread
        { get { return fireSpread; } }
    public int MeleeRate
        { get { return meleeRate; } }
    public int FireRate
        { get { return fireRate; } }
    public int Damage
        { get { return damage; } }
    public int MeleeDamage
    { get { return meleeDamage; } }
    public int ReloadSpeed
        { get { return reloadSpeed; } }
    public bool Auto
        { get { return auto; } }
    public bool IsMelee
        { get { return isMelee; } }
}
