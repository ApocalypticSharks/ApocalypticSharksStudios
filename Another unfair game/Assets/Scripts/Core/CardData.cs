using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
    public CardSO data;
    public Image rankImage;
    public Image rankSecondImage;
    public Image rankImageReversed;
    public Image rankSecondImageReversed;
    public Image suitImage;

    public void Initialize()
    {
        rankImage.sprite = data.RankSprite;
        rankImageReversed.sprite = data.RankSprite;
        suitImage.sprite = data.suitSprite;
        if (data.rank == CardRank.Ten)
        {
            rankSecondImage.sprite = data.RankSecondarySprite;
            rankSecondImageReversed.sprite = data.RankSecondarySprite;
        }
        else 
        {
            rankSecondImage.sprite = null;
            rankSecondImageReversed.sprite = null;
        }
    }
}
