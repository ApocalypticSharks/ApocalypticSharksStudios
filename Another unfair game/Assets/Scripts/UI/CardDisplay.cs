// CardDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour
{
    //[Header("UI References")]
    //public Image cardImage;
    //public Image cardBack;
    //public TMP_Text valueText;
    //public TMP_Text nameText;
    //public GameObject curseIndicator;
    //public GameObject blessingIndicator;

    //[Header("Animation")]
    //public float moveSpeed = 10f;
    //public float rotationSpeed = 180f;

    //private CardSO cardData;
    //private bool isFaceUp = true;
    //private bool isInHand = true;
    //private Vector3 targetPosition;
    //private Quaternion targetRotation;

    //public void Initialize(CardSO data, bool faceUp = true)
    //{
    //    cardData = data;
    //    isFaceUp = faceUp;

    //    UpdateDisplay();

    //    // Начальная позиция
    //    targetPosition = transform.localPosition;
    //    targetRotation = transform.localRotation;
    //}

    //private void UpdateDisplay()
    //{
    //    if (cardData == null) return;

    //    if (isFaceUp)
    //    {
    //        cardImage.sprite = cardData.cardSprite;
    //        cardBack.gameObject.SetActive(false);

    //        valueText.text = cardData.GetValueText();
    //        nameText.text = cardData.cardName;

    //        // Индикаторы особых свойств
    //        curseIndicator.SetActive(cardData.isCursed);
    //        blessingIndicator.SetActive(cardData.isBlessed);
    //    }
    //    else
    //    {
    //        cardBack.gameObject.SetActive(true);
    //        cardImage.sprite = null;

    //        valueText.text = "";
    //        nameText.text = "";

    //        curseIndicator.SetActive(false);
    //        blessingIndicator.SetActive(false);
    //    }
    //}

    //public void SetTargetPosition(Vector3 position)
    //{
    //    targetPosition = position;
    //}

    //public void SetTargetRotation(Quaternion rotation)
    //{
    //    targetRotation = rotation;
    //}

    //public void FlipCard(bool faceUp)
    //{
    //    isFaceUp = faceUp;
    //    UpdateDisplay();

    //    // Анимация переворота
    //    StartCoroutine(FlipAnimation());
    //}

    //private void Update()
    //{
    //    // Плавное движение к целевой позиции
    //    transform.localPosition = Vector3.Lerp(
    //        transform.localPosition,
    //        targetPosition,
    //        Time.deltaTime * moveSpeed
    //    );

    //    transform.localRotation = Quaternion.Lerp(
    //        transform.localRotation,
    //        targetRotation,
    //        Time.deltaTime * rotationSpeed
    //    );
    //}

    //// Клик по карте
    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (GameManager.Instance.currentGameState == GameState.Battle)
    //    {            
    //        if (!isInHand || !isFaceUp) return;

    //        if (eventData.button == PointerEventData.InputButton.Left)
    //        {
    //            // Левый клик - разыграть карту
    //            GameManager.Instance.deckManager.PlayCard(cardData);
    //        }
    //        else if (eventData.button == PointerEventData.InputButton.Right)
    //        {
    //            // Правый клик - сжечь карту (за спичку)
    //            TryBurnCard();
    //        }
    //    }
    //    if (GameManager.Instance.currentGameState == GameState.Shop)
    //    {
    //        GameManager.Instance.deckManager.AddCardToDeck(cardData);
    //    }
    //}

    //private void TryBurnCard()
    //{
    //    if (cardData.matchstickCost <= GameManager.Instance.playerMatchsticks)
    //    {
    //        GameManager.Instance.SpendMatchstick(cardData.matchstickCost);
    //        GameManager.Instance.deckManager.DiscardCard(cardData, GameManager.Instance.deckManager.GetPlayerHand());
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        Debug.Log("Not enough matchsticks!");
    //    }
    //}

    //// Анимация переворота
    //private System.Collections.IEnumerator FlipAnimation()
    //{
    //    float duration = 0.3f;
    //    float elapsed = 0f;

    //    Vector3 startScale = transform.localScale;
    //    Vector3 targetScale = new Vector3(0.1f, startScale.y, startScale.z);

    //    // Сжимаем
    //    while (elapsed < duration / 2)
    //    {
    //        elapsed += Time.deltaTime;
    //        float t = elapsed / (duration / 2);
    //        transform.localScale = Vector3.Lerp(startScale, targetScale, t);
    //        yield return null;
    //    }

    //    // Меняем сторону
    //    UpdateDisplay();

    //    // Разжимаем
    //    elapsed = 0;
    //    while (elapsed < duration / 2)
    //    {
    //        elapsed += Time.deltaTime;
    //        float t = elapsed / (duration / 2);
    //        transform.localScale = Vector3.Lerp(targetScale, startScale, t);
    //        yield return null;
    //    }
    //}
}