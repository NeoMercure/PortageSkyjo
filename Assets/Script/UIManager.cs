using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject discardButton;
    [SerializeField] private GameObject nextTurnButton;
    [SerializeField] private GameObject newGameButton;

    public void UpdateDiscardButton(bool _canDiscard)
    {
        discardButton.SetActive(_canDiscard);
    }

    public void ShowNextTurn(bool _show)
    {
        nextTurnButton.SetActive(_show);
    }

    public void ShowNewGameButton(bool _show)
    {
        newGameButton.SetActive(_show);
    }

    public void UpdatePlayerScore(PlayerView _view)
    {
        _view.UpdateScore();
    }
}