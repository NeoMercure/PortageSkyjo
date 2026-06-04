using System.Collections.Generic;
using System.Linq;

public class ScoreSystem
{
    public Dictionary<Player, int> CalculateRoundScores(List<Player> _players)
    {
        Dictionary<Player, int> scores = new();

        foreach (Player p in _players)
        {
            int score = 0;

            foreach (Card c in p.Hand) // all cards, even face down
            {
                score += c.Value;
            }

            scores[p] = score;
        }
        return scores;
    }

    public Dictionary<Player, int> CalculateScore(List<Player> _players)
    {
        Dictionary<Player, int> scores = new();

        foreach (Player p in _players)
        {
            int score = 0;

            foreach (Card c in p.Hand)
            {
                if (c.IsFaceUp)
                {
                    score += c.Value;
                }
            }

            scores[p] = score;
        }
        return scores;
    }


    // if the player who finished the round first has the fewest points, he double them
    public void CheckScore(Dictionary<Player, int> _scores, Player _triggeringPlayer)
    {
        int minScore = _scores.Values.Min();

        if (_scores[_triggeringPlayer] > minScore)
        {
            _scores[_triggeringPlayer] *= 2;
        }
    }
}