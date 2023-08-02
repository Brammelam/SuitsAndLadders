using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Player Deck", menuName = "Custom/Player Deck")]
public class PlayerDeck : ScriptableObject
{
    public List<int> unlockedCardIDs = new List<int>();

    public List<PlayerDeckEntry> playerDeckEntries = new List<PlayerDeckEntry>();
}
