using UnityEngine;

public class MenuScript : MonoBehaviour
{
    // get references to UI buttons here (if needed)
    // Example for Play and Quit buttons
    [SerializeField] private GameObject[] mainMenuButtons;
    // Container for Host and Join buttons
    [SerializeField] private GameObject hostNumberDropdown;

    private int selectedNumberOfPlayers = 2; // Default to 2 players

    private void Start()
    {
        // Initialize menu state
        hostNumberDropdown.SetActive(false); // Hide host/join buttons initially
    }

    // quit button function
    public void QuitGame()
    {
        Application.Quit();
    }

    // play button function : open 2 other buttons "Host Game" and "Join Game"
    public void PlayGame()
    {
        // Hide main menu buttons
        foreach (var button in mainMenuButtons)
        {
            button.SetActive(false);
        }

        // Show host and join game buttons
        hostNumberDropdown.SetActive(true);
    }

    // Get back to main menu from host/join menu
    public void BackToMainMenu()
    {
        // Show main menu buttons
        foreach (var button in mainMenuButtons)
        {
            button.SetActive(true);
        }

        // Hide host and join game buttons
        hostNumberDropdown.SetActive(false);
    }

    // Function to set number of players from dropdown
    public void SetNumberOfPlayers(int number)
    {
        selectedNumberOfPlayers = number + 2;
        // Debug.Log($"<Color=magenta>Number of players set to: {selectedNumberOfPlayers}");
    }

    // host game button function
    public void HostGame()
    {
        // Setup hosting a game : number of players, game settings, etc.
    }

    // join game button function
    public void JoinGame()
    {
        // Setup joining a game : input IP address, connect to host, etc.
    }
}