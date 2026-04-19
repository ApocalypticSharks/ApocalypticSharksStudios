using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
    public CardSO data;
    [SerializeField] private bool _faceUp = true;
    public Image rankImage;
    public Image rankSecondImage;
    public Image rankImageReversed;
    public Image rankSecondImageReversed;
    public Image suitImage;

    public bool IsFaceUp => _faceUp;

    /// <summary>Face-down shows <c>BackImage</c> (if present) and hides rank/suit. Used for the dealer hole card.</summary>
    public void SetFaceUp(bool faceUp)
    {
        _faceUp = faceUp;
        Transform root = transform;
        Transform frontT = root.Find("FrontImage");
        Transform backT = root.Find("BackImage");
        if (frontT != null && backT != null)
        {
            frontT.gameObject.SetActive(faceUp);
            backT.gameObject.SetActive(!faceUp);
        }

        if (faceUp)
        {
            Initialize();
        }
        else
        {
            if (rankImage != null) rankImage.enabled = false;
            if (rankSecondImage != null) rankSecondImage.enabled = false;
            if (rankImageReversed != null) rankImageReversed.enabled = false;
            if (rankSecondImageReversed != null) rankSecondImageReversed.enabled = false;
            if (suitImage != null) suitImage.enabled = false;
        }
    }

    public void Initialize()
    {
        if (data == null)
            return;

        Image cardFace = ResolveCardFaceImage();
        if (cardFace != null && data.cardSprite != null)
            cardFace.sprite = data.cardSprite;

        if (rankImage != null)
            rankImage.sprite = data.RankSprite;
        if (rankImageReversed != null)
            rankImageReversed.sprite = data.RankSprite;
        if (suitImage != null)
            suitImage.sprite = data.suitSprite;
        bool showSecondDigit = data.rank == CardRank.Ten && data.RankSecondarySprite != null;
        if (rankSecondImage != null)
        {
            rankSecondImage.sprite = showSecondDigit ? data.RankSecondarySprite : null;
            rankSecondImage.enabled = showSecondDigit;
        }
        if (rankSecondImageReversed != null)
        {
            rankSecondImageReversed.sprite = showSecondDigit ? data.RankSecondarySprite : null;
            rankSecondImageReversed.enabled = showSecondDigit;
        }
    }

    /// <summary>Battle cards use a child "FrontImage"; shop offer uses a root <see cref="Image"/>.</summary>
    private Image ResolveCardFaceImage()
    {
        Transform front = transform.Find("FrontImage");
        if (front != null)
        {
            Image img = front.GetComponent<Image>();
            if (img != null)
                return img;
        }

        return GetComponent<Image>();
    }
}
