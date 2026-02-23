using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// This class don't hold game's rules but only the properties of the card and the method to flip it. (e.g. the value, the color, the sprite, animation, etc.)
public class Card : MonoBehaviour
{
    [Header("Card properties")]
    [SerializeField] private TextMeshPro valueText;
    [SerializeField] private SpriteRenderer cardSpriteRenderer;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private GameObject cardFront;

    public bool IsFaceUp {get; private set;}
    public int Value => cardData != null ? cardData.value : 0;

    [Header("Runtime data")]
    public CardSO cardData {get; private set;}

    public void Init(CardSO _data)
    {
        cardData = _data;
        IsFaceUp = false;
        UpdateVisual();
    }
    
   // Change the properties of the card based on the CardSO
    public void UpdateVisual()
    {
        if (cardData == null) return;

        valueText.text = cardData.value.ToString();
        switch (cardData.color)
        {
            case CardColor.Blue:
                cardSpriteRenderer.color = Color.blue;
                break;
            case CardColor.Cyan:
                cardSpriteRenderer.color = Color.cyan;
                break;
            case CardColor.Green:
                cardSpriteRenderer.color = Color.green;
                break;
            case CardColor.Yellow:
                cardSpriteRenderer.color = Color.yellow;
                break;
            case CardColor.Red:
                cardSpriteRenderer.color = Color.red;
                break;
        }
    }

    // Flips the card to show the front
    public void FlipCard()
    {
        IsFaceUp = true;
        cardBack.SetActive(false);
        cardFront.SetActive(true);
    }
}