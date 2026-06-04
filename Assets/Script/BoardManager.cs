using System.Collections.Generic;
using UnityEngine;

// Plateau d'un seul joueur
public class BoardManager : MonoBehaviour
{
    [SerializeField] private Transform playersContainer;
    [SerializeField] private float radius = 8f;

    // Arrange all players in circle
    public void ArrangePlayers(int _playerCount)
    {
        for (int i = 0; i < playersContainer.childCount; i++)
        {
            Transform playerView = playersContainer.GetChild(i);

            float angle = i * Mathf.PI * 2 / _playerCount - Mathf.PI / 2;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0
            ) * radius;

            playerView.localPosition = pos;
        }
    }

    // Scale current player's game bigger than the others
    public void Rescale(int _currentPlayerIndex)
    {
        for (int i = 0; i < playersContainer.childCount; i++)
        {
            playersContainer.GetChild(i).localScale =
                (i == _currentPlayerIndex) ? Vector3.one : Vector3.one * 0.8f;
        }
    }

    // Arrange the cards in a grid
    public void ArrangeCards()
    {
        foreach (PlayerView view in playersContainer.GetComponentsInChildren<PlayerView>())
        {
            view.ArrangeCards();
        }
    }

    // Look at column to check if it has the same cards

    public void CheckColumnClear(Player _player, PlayerView _pv, Deck _deck)
    {
        int rowCount = 3;
        int columnCount = _player.Hand.Count / rowCount;

        List<Card> cardsToRemove = new List<Card>();

        // each column
        for (int col = 0; col < columnCount; col++)
        {
            bool sameNumber = true;
            Card firstCard = null;
            List<Card> columnCards = new List<Card>();

            // each row
            for (int row = 0; row < rowCount; row++)
            {
                int index = row * columnCount + col;
                Card currentCard = _player.Hand[index];
                columnCards.Add(currentCard);

                if (!currentCard.IsFaceUp)
                {
                    sameNumber = false;
                    break;
                }

                if (row == 0)
                {
                    firstCard = currentCard;
                }
                else
                {
                    if (currentCard.Value != firstCard.Value)
                    {
                        sameNumber = false;
                        break;
                    }
                }
            }

            if (sameNumber)
            {
                Debug.Log($"{col} same value");
                
                cardsToRemove.AddRange(columnCards);
            }
        }

        foreach (Card card in cardsToRemove)
        {
            _player.RemoveCard(card);
            // Add them in discardPile
            _deck.Discard(card);
        }

        // Update visual
        _pv.ArrangeCards();
    }
}