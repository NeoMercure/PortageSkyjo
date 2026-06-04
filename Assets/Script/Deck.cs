using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private readonly List<Card> drawPile = new List<Card>();
    private readonly List<Card> discardPile = new List<Card>();

    public Card TopDiscard => discardPile.Count > 0 ? discardPile[^1] : null;

    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform discardParent;

    [Header("Card Data")]
    [SerializeField] private CardSO minus2;
    [SerializeField] private CardSO minus1;
    [SerializeField] private CardSO zero;
    [SerializeField] private List<CardSO> positiveCards;

    public int CardsCount => drawPile.Count;
    public int DiscardCount => discardPile.Count;

    // Creation of all the game cards
    public void BuildDeck()
    {
        drawPile.Clear();

        AddCards(minus2, 5);
        AddCards(minus1, 10);
        AddCards(zero, 15);

        foreach (CardSO _cardData in positiveCards)
        {
            AddCards(_cardData, 10);
        }
    }

    // Card creation function
    private void AddCards(CardSO _data, int _amount)
    {
        for (int i = 0; i < _amount; i++)
        {
            Card card = Instantiate(cardPrefab, this.transform);
            card.name = "Card" + _data.value.ToString();
            card.Init(_data);
            card.clickableType = ClickableType.DrawPile;
            drawPile.Add(card);
        }
    }

    // Draw a card from the deck, which is the top card of the deck (the last card in the list)
    public Card Draw()
    {
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("Draw pile is empty");
            return null;
        }

        Card card = drawPile[^1]; // ^1 = last element
        drawPile.RemoveAt(drawPile.Count - 1);
        return card;
    }

    public void Discard(Card _card)
    {
        discardPile.Add(_card);
        _card.clickableType = ClickableType.DiscardPile;

        if (!_card.IsFaceUp)
        {
            _card.FlipCard();
        }

        _card.transform.SetParent(discardParent);
        _card.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        UpdatePosZCardDiscardpile();
    }

    private void UpdatePosZCardDiscardpile()
    {
        if (discardPile.Count > 0)
        {
            for (int i = 0; i < discardPile.Count; i++)
            {
                discardPile[i].transform.localPosition = new Vector3(0, 0, 1);
            }
    
            discardPile[^1].transform.localPosition = Vector3.zero;
        }
    }

    // Draw Card from DiscardPile
    public Card DrawDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("Discard pile is empty");
            return null;
        }

        Card card = discardPile[^1];
        discardPile.RemoveAt(discardPile.Count - 1);
        return card;
    }

    // Shuffle the deck using the Fisher-Yates algorithm
    public void ShuffleDeck()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]); // deconstructing tuples : swaps two values ​​without a temp var
        }
    }

    // Remove all cards
    public void ClearPiles()
    {
        foreach (Card c in drawPile)
        {
            Destroy(c.gameObject);
        }

        foreach (Card c in discardPile)
        {
            Destroy(c.gameObject);
        }

        // Clear piles
        discardPile.Clear();
        drawPile.Clear();
    }
}