using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    [SerializeField] private string characterName;
    [SerializeField] private Sprite portrait;

    private void OnEnable()
    {
        portrait = GetComponent<Sprite>();
    }

    public string GetCharacterName() => characterName;
    public Sprite GetPortrait() => portrait;
}
