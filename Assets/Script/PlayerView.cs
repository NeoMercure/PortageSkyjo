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
        int rowCount = 3;
        int columnCount = player.Hand.Count / rowCount;
        
        float spacingX = 1.05f; // 1.1
        float spacingY = 1.6f; // 1.6

        for (int i = 0; i < player.Hand.Count; i++)
        {
            int row = i / columnCount;
            int column = i % columnCount;

            Vector3 position = new Vector3(column * spacingX, -row * spacingY, 0);

            Card card = player.Hand[i];
            card.transform.SetParent(gridParent, false);
            card.transform.localPosition = position;
        }
    }
}