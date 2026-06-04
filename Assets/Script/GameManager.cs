 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        WaitingStartReveal,
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
    private ScoreSystem scoreSystem = new ScoreSystem();

    [Header("Cards Deck")]
    [SerializeField] private Deck deck;
    [SerializeField] private Transform cardToPlaceTrans;

    [Header("View")]
    [SerializeField] private PlayerView playerViewPrefab;
    [SerializeField] private Transform playersContainer;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private UIManager uIManager;

    private bool cardFromDiscardPile;
    private bool lastRoundTriggered;
    private int lastPlayerIndex;
    private bool showNextTurnButton;
    private bool showNewGameButton;
    private bool isStartPhase;
    private int revealCount;
    private List<PlayerView> playerViews = new();

    private GameState currentState;
    private TurnState turnState;
    private Card drawnCard;

    void Start()
    {
        InitGame();
    }

    private void InitGame()
    {
        lastRoundTriggered = false;
        lastPlayerIndex = -1;
        showNextTurnButton = false;
        showNewGameButton = false;
        revealCount = 0;
        
        isStartPhase = true;
        currentState = GameState.Setup;
        currentPlayerIndex = 0;
        cardFromDiscardPile = false;

        deck.BuildDeck();
        deck.ShuffleDeck();

        InitPlayers();

        DealCards();

        boardManager.ArrangeCards();
        boardManager.ArrangePlayers(nbPlayers);
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
            playerViews.Add(view);
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

    private void StartTurn()
    {
        if (isStartPhase)
        {
            turnState = TurnState.WaitingStartReveal;
            revealCount = 0;
            return;
        }

        turnState = TurnState.WaitingDrawChoice;
        drawnCard = null;

        Debug.Log("Player " + currentPlayerIndex + " turn");
    }

    private void NextPlayer()
    {
        currentPlayerIndex++;

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

        uIManager.UpdateDiscardButton(!cardFromDiscardPile);
        uIManager.ShowNextTurn(showNextTurnButton);
        uIManager.ShowNewGameButton(showNewGameButton);
    }

    private void HandleClick(Card _cardClicked)
    {
        switch (turnState)
        {
            case TurnState.WaitingStartReveal:
                HandleStartReveal(_cardClicked);
            break;
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

    private void HandleStartReveal(Card _c)
    {
        if (_c.owner != players[currentPlayerIndex]) return;
        if (_c.IsFaceUp) return;

        _c.FlipCard();
        revealCount++;

        if (revealCount >= 2)
        {
            NextPlayer();
            if (currentPlayerIndex == 0)
            {
                isStartPhase = false;
                DetermineFirstPlayer();
            }
            StartTurn();
        }
    }

    private void DetermineFirstPlayer()
    {
        int bestScore = 0;
        int bestId = 0;

        for (int i = 0; i < players.Count; i++)
        {
            int score = players[i].Score;
            if (score > bestScore) // the one with the highest score starts
            {
                bestScore = score;
                bestId = i;
            }
        }
        currentPlayerIndex = bestId;
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

        PlayerView currentPv = playerViews[currentPlayerIndex];
        boardManager.CheckColumnClear(currentPlayer, currentPv, deck);

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
        if (_card.IsFaceUp) return;

        Player currentPlayer = players[currentPlayerIndex];
        PlayerView currentPv = playerViews[currentPlayerIndex];

        currentPlayer.FlipCard(_card);

        boardManager.CheckColumnClear(currentPlayer, currentPv, deck);

        EndTurn();
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

        // Turn all the cards face up
        foreach (Player p in players)
        {
            p.FlipAllCard();
        }

        var scores = scoreSystem.CalculateRoundScores(players);
        Player triggeringPlayer = players[lastPlayerIndex];
        scoreSystem.CheckScore(scores, triggeringPlayer);

        foreach (var keyValuePair in scores)
        {
            keyValuePair.Key.AddScore(keyValuePair.Value);

            PlayerView pv = playersContainer
            .GetChild(players.IndexOf(keyValuePair.Key))
            .GetComponent<PlayerView>();

            pv.UpdateScore();

            Debug.Log($"<color=yellow>Player {keyValuePair.Key.name} round: {keyValuePair.Value} total: {keyValuePair.Key.TotalScore}");
        }

        CheckEndGame();
    }

    private void CheckEndGame()
    {
        foreach (Player p in players)
        {
            if (p.TotalScore >= 100)
            {
                EndGame();
            }
            else
            {
                showNextTurnButton = true;
            }
        }
    }

    private void EndGame()
    {
        Player winner = players.OrderBy(p => p.TotalScore).First();

        Debug.Log("GAME OVER - Winner is {winner.name} with {winner.TotalScore}");
        showNextTurnButton = false;
        showNewGameButton = true;
    }

    private void ResetGameState()
    {
        // Rest logic
        lastRoundTriggered = false;
        lastPlayerIndex = -1;
        revealCount = 0;
        isStartPhase = true;
        currentPlayerIndex = 0;
        cardFromDiscardPile = false;
        drawnCard = null;
    }

    private void ResetRound()
    {
        ResetGameState();

        // Reset UI
        showNextTurnButton = false;
        showNewGameButton = false;

        // Reset deck
        deck.ClearPiles();
        deck.BuildDeck();
        deck.ShuffleDeck();

        // Destoy gameobject
        foreach (Card c in playersContainer.GetComponentsInChildren<Card>())
        {
            Destroy(c.gameObject);
        }

        foreach (Player p in players)
        {
            p.ClearHand();
        }

        // Redistribute
        DealCards();
        boardManager.ArrangeCards();
        boardManager.ArrangePlayers(nbPlayers);
        StartDiscardPile();

        // Start
        StartTurn();
    }

    public void StartNewRound()
    {
        ResetRound();
    }

    public void ResetFullGame()
    {
        Debug.Log("FULL GAME RESET");

        // Reset score
        foreach (Player p in players)
        {
            p.ResetScore();
        }

        // Reset round
        ResetRound();
    }
}