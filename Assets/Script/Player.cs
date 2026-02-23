using System.Collections.Generic;
using UnityEngine;

// Deal with cards (Add, remove) and the player's score. Player is a logical entity that holds the cards in hand and the score,
// but it doesn't deal with the visual representation of the cards, which is handled by the Card class.
public class Player
{
    public string name = "aaa";
    private readonly List<Card> hand = new();
    // Same list! Under interface and read-only. The player remains in control of their hand
    public IReadOnlyList<Card> Hand => hand;
    public int Score => CalculateScore();

    // Add a card to the player's hand
    public void AddCard(Card _card)
    {
        if (_card == null) return;
        hand.Add(_card);
    }

    // Remove a card from the player's hand : Add in discardPile
    public void RemoveCard(Card _card)
    {
        hand.Remove(_card);
    }
    
    // Flip a card in the player's hand
    public void FlipCard(Card _card)
    {
        // We can only flip a card if it's not already face up
        if (hand.Contains(_card) && !_card.IsFaceUp)
        {
            _card.FlipCard();
        }
    }

    // Calculate the player's score based on the cards in their hand and only if they are facingUp
    private int CalculateScore()
    {
        int score = 0;
        
        foreach (Card _card in hand)
        {
            if (_card.IsFaceUp)
            {
                score += _card.Value;
            }
        }

        return score;
    }

    public void SetPlayerName(string _str)
    {
        name = _str;
    }
}