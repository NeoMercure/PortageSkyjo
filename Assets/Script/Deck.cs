using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private readonly List<Card> drawPile = new List<Card>();
    private readonly List<Card> discardPile = new List<Card>();

    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform discardPilePos;

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
            card.Init(_data);
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

    public void Discard(Card card)
    {
        discardPile.Add(card);
        card.transform.position = discardPilePos.position;
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
}