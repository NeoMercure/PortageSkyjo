using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO", order = 1)]
public class CardSO : ScriptableObject
{
    public int value;
    public CardColor color;
}