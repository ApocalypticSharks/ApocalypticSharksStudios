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
    [Tooltip("Corner suit pips (e.g. SuitImage (1) / (2)); same sprite as base standard card.")]
    [SerializeField] private Image suitImageCorner1;
    [SerializeField] private Image suitImageCorner2;

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
            SetSuitImagesActive(false);
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
        ApplyCornerSuitSprites(data.suitSprite);
        ApplyCenterDisplaySprite();
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

    void ApplyCornerSuitSprites(Sprite suit)
    {
        bool show = suit != null;
        void Set(Image img)
        {
            if (img == null)
                return;
            img.sprite = suit;
            img.enabled = show;
        }
        Set(suitImageCorner1);
        Set(suitImageCorner2);
    }

    /// <summary>Center (Action): Standard cards show suit; special cards show <see cref="CardSO.actionSprite"/>.</summary>
    void ApplyCenterDisplaySprite()
    {
        if (suitImage == null || data == null)
            return;
        Sprite s = data.cardType == CardType.Standard
            ? data.suitSprite
            : (data.actionSprite != null ? data.actionSprite : data.suitSprite);
        suitImage.sprite = s;
        suitImage.enabled = s != null;
    }

    void SetSuitImagesActive(bool active)
    {
        if (suitImage != null)
            suitImage.enabled = active;
        if (suitImageCorner1 != null)
            suitImageCorner1.enabled = active;
        if (suitImageCorner2 != null)
            suitImageCorner2.enabled = active;
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
