using System.Collections.Generic;

[System.Serializable]
public class PlayerDeckData
{
    public List<int> unlockedCardIDs = new List<int>();
    public List<PlayerDeckEntry> playerDeckEntries = new List<PlayerDeckEntry>();
}