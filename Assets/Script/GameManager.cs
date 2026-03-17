using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum CardColor
{
    Blue,
    Cyan,
    Green,
    Yellow,
    Red,
    White, // Default color
}

public enum GameState
{
    Setup,
    RevealStartCards,
    PlayerTurn,
    EndRound,
    EndGame,
}

// Class lunch the game : Init it, turn by turn, global state (win, lose, pause, etc.) Flow, rules, turn
public class GameManager : MonoBehaviour
{
    private enum TurnState
    {
        WaitingDrawChoice,
        WaitingReplaceChoice,
        WaitingFlipChoice,
        TurnEnd,
    }

    // first, we define the number of players before adding a menu to choose it.
    // After it will be with menu to choose 2 or more players
    [Header("Player")]
    [Range(2,8)]
    public int nbPlayers = 2;
    private readonly List<Player> players = new();
    private int currentPlayerIndex;

    [Header("Distance from the center")]
    public float radius = 8f; // 7.5 bien

    [Header("Cards Deck")]
    [SerializeField] private Deck deck;
    [SerializeField] private Transform cardToPlaceTrans;

    [Header("View")]
    [SerializeField] private PlayerView playerViewPrefab;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private GameObject discardButton;

    private bool cardFromDiscardPile;
    private bool lastRoundTriggered = false;
    private int lastPlayerIndex = -1;

    private GameState currentState;
    private TurnState turnState;
    private Card drawnCard;

    void Start()
    {
        currentState = GameState.Setup;
        currentPlayerIndex = 0;
        cardFromDiscardPile = false;
        InitGame();
    }

    private void InitGame()
    {
        deck.BuildDeck();
        deck.ShuffleDeck();

        InitPlayers();

        DealCards();

        ArrangeCard();
        ArrangePlayer();

        // RevealInitialCards();
        StartDiscardPile();

        StartTurn();
    }

    private void StartDiscardPile() 
    {
        Card firstCard = deck.Draw();

        if(firstCard == null) return;

        firstCard.FlipCard();
        deck.Discard(firstCard);
    }

    private void ArrangeCard()
    {
        foreach (PlayerView _view in playersContainer.GetComponentsInChildren<PlayerView>())
        {
            _view.ArrangeCards();
        }
    }

    // Display player grid in circle around center
    private void ArrangePlayer()
    {
        int count = playersContainer.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform playerView = playersContainer.GetChild(i);

            float angle = i * Mathf.PI * 2 / nbPlayers;

            angle = angle - Mathf.PI / 2;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0
            ) * radius;

            playerView.localPosition = pos;

            RescaleGrid();
        }
    }

    private void RescaleGrid()
    {
        // Rescale Grids depending to current Player
        for (int i = 0; i < players.Count; i++)
        {
            if (i != currentPlayerIndex)
            {
                playersContainer.GetChild(i).localScale = Vector3.one * 0.8f;
            }
            else
            {
                playersContainer.GetChild(i).localScale = Vector3.one;
            }
        }
    }

    // Create all Player and add them to the list
    private void InitPlayers()
    {
        for (int i = 0; i < nbPlayers; i++)
        {
            Player player = new Player();
            player.name = i.ToString();
            players.Add(player);

            PlayerView view = Instantiate(playerViewPrefab, playersContainer);
            view.Init(player); 
        }
    }

    // Deal Cards to players
    private void DealCards()
    {
        const int cardsPerPlayer = 12;
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            foreach (Player _p in players)
            {
                Card card = deck.Draw();
                _p.AddCard(card);
            }
        }
    }

    // Before start partie, all players need to reveal 2 cards. For now [0] [1] but after, 2 cards they want to reveal
    private void RevealInitialCards()
    {
        foreach (Player _p in players)
        {
            _p.FlipCard(_p.Hand[0]);
            _p.FlipCard(_p.Hand[1]);
        }
    }

    private void StartTurn()
    {
        turnState = TurnState.WaitingDrawChoice;
        drawnCard = null;

        Debug.Log("Player " + currentPlayerIndex + " turn");
    }

    private void NextPlayer()
    {
        currentPlayerIndex++;
        // Debug.Log($"<color=red>player++ {currentPlayerIndex}");

        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider == null) return;

            Card card = hit.collider.GetComponent<Card>();
            if (card == null) return;

            HandleClick(card);
        }

        if (cardFromDiscardPile)
        {
            discardButton.SetActive(false);
        }
        else
        {
            discardButton.SetActive(true);
        }
    }

    private void HandleClick(Card _cardClicked)
    {
        switch (turnState)
        {
            case TurnState.WaitingDrawChoice:
                HandleDrawChoice(_cardClicked);
            break;
            case TurnState.WaitingReplaceChoice:
                HandleReplace(_cardClicked);
            break;
            case TurnState.WaitingFlipChoice:
                HandleFlip(_cardClicked);
            break;
        }
    }

    // Draw a card from drawpile or discardpile and show it
    private void HandleDrawChoice(Card _card)
    {
        if (_card.clickableType == ClickableType.DrawPile)
        {
            drawnCard = deck.Draw();
            drawnCard.FlipCard();
            cardFromDiscardPile = false;
        }
        else if (_card.clickableType == ClickableType.DiscardPile)
        {
            drawnCard = deck.DrawDiscardPile();
            // Since we just drew from the discardpile, we cannot discard it !
            cardFromDiscardPile = true;
        }

        if (drawnCard != null)
        {
            drawnCard.transform.position = cardToPlaceTrans.position;
            turnState = TurnState.WaitingReplaceChoice;
        }
    }

    // Replace one of the player's cards with the one drawn
    private void HandleReplace(Card _card)
    {
        if (_card.clickableType != ClickableType.PlayerCard) return;
        Player currentPlayer = players[currentPlayerIndex];

        if (_card.owner != currentPlayer) return;

        // Get idex from card drawn
        int id = currentPlayer.GetCardIndex(_card);

        // Discard old player's card
        deck.Discard(_card);

        // Replace player's card
        drawnCard.owner = currentPlayer;
        currentPlayer.AddCardAt(id, drawnCard);

        drawnCard = null;

        // Debug.Log($"Player {currentPlayerIndex} is replacing card at index {id} with {drawnCard.name}");

        // Update visual
        PlayerView pv = playersContainer.GetChild(currentPlayerIndex).GetComponent<PlayerView>();
        pv.ArrangeCards();

        CheckColumnClear();

        EndTurn();
    }

    public void DiscardDrawnCard()
    {
        if (turnState != TurnState.WaitingReplaceChoice) return;

        deck.Discard(drawnCard);
        drawnCard = null;

        turnState = TurnState.WaitingFlipChoice;
    }

    private void HandleFlip(Card _card)
    {
        if (_card.clickableType != ClickableType.PlayerCard) return;
        if (_card.owner != players[currentPlayerIndex]) return;

        _card.FlipCard();

        CheckColumnClear();

        EndTurn();
    }

    private void CheckColumnClear()
    {
        int rowCount = 3;
        int columnCount = players[currentPlayerIndex].Hand.Count / rowCount;
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
                Card currentCard = players[currentPlayerIndex].Hand[index];
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
            players[currentPlayerIndex].RemoveCard(card);
            // Add them in discardPile
            deck.Discard(card);
        }

        // Update visual
        PlayerView pv = playersContainer.GetChild(currentPlayerIndex).GetComponent<PlayerView>();
        pv.ArrangeCards();
    }

    private bool IsRoundFinished()
    {
        foreach (Card card in players[currentPlayerIndex].Hand)
        {
            if (!card.IsFaceUp) return false;
        }
        return true;
    }

    private void EndTurn()
    {
        cardFromDiscardPile = false;
        Player currentPlayer = players[currentPlayerIndex];

        if (IsRoundFinished() && !lastRoundTriggered)
        {
            lastRoundTriggered = true;
            lastPlayerIndex = currentPlayerIndex;

            Debug.Log($"<color=green> Last round triggered by player {currentPlayerIndex}");
        }

        NextPlayer();

        if (lastRoundTriggered && currentPlayerIndex == lastPlayerIndex)
        {
            EndRun();
            return;
        }

        StartTurn();
    }

    private void EndRun()
    {
        Debug.Log($"<color=magenta> Round Finished");

        foreach (Player player in players)
        {
            player.FlipAllCard();
            player.allScore += player.Score;
            Debug.Log($"Player score : {player.allScore}");
        }
    }
}