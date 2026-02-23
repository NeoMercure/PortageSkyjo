using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    private Player player;

    public void Init(Player _playerData)
    {
        player = _playerData;
        ArrangeCards();
    }

    // Display Card in grid
    public void ArrangeCards()
    {
        int columns = 4;
        float spacingX = 1.05f; // 1.1
        float spacingY = 1.6f; // 1.6

        for (int i = 0; i < player.Hand.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;

            Vector3 position = new Vector3(column * spacingX, -row * spacingY, 0);
            player.Hand[i].transform.SetParent(gridParent);
            player.Hand[i].transform.localPosition = position;
        }
    }
}