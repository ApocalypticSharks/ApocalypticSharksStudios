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
        if (data == null)
            return;
        if (rankImage != null)
            rankImage.sprite = data.RankSprite;
        if (rankImageReversed != null)
            rankImageReversed.sprite = data.RankSprite;
        if (suitImage != null)
            suitImage.sprite = data.suitSprite;
        if (data.rank == CardRank.Ten)
        {
            if (rankSecondImage != null)
                rankSecondImage.sprite = data.RankSecondarySprite;
            if (rankSecondImageReversed != null)
                rankSecondImageReversed.sprite = data.RankSecondarySprite;
        }
        else
        {
            if (rankSecondImage != null)
                rankSecondImage.sprite = null;
            if (rankSecondImageReversed != null)
                rankSecondImageReversed.sprite = null;
        }
    }
}
