using System.Collections.Generic;
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
    // first, we define the number of players before adding a menu to choose it.
    // After it will be with menu to choose 2 or more players
    public int nbPlayers = 2;
    private readonly List<Player> players = new();
    private int currentPlayerIndex;

    [Header("Distance from the center")]
    public float radius = 8f; // 7.5 bien

    [Header("Cards Deck")]
    [SerializeField] private Deck deck;

    [Header("View")]
    [SerializeField] private PlayerView playerViewPrefab;
    [SerializeField] private Transform playersContainer;

    private GameState currentState;

    void Start()
    {
        currentState = GameState.Setup;
        currentPlayerIndex = 0;
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

    private void NextPlayer()
    {
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }
    }
}