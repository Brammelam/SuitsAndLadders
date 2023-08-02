using UnityEngine;
using System.IO;

public class DeckDataManager : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/player_deck_data.json";
        LoadPlayerDeckData();
    }

    public void SavePlayerDeckData()
    {
        PlayerDeck playerDeck = PlayerDeckHolder.Instance.playerDeck;
        PlayerDeckData data = new PlayerDeckData();
        data.unlockedCardIDs = playerDeck.unlockedCardIDs;
        data.playerDeckEntries = playerDeck.playerDeckEntries;
        string jsonData = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, jsonData);
    }

    public void LoadPlayerDeckData()
    {
        if (File.Exists(savePath))
        {
            string jsonData = File.ReadAllText(savePath);
            PlayerDeckData data = JsonUtility.FromJson<PlayerDeckData>(jsonData);

            PlayerDeck playerDeck = PlayerDeckHolder.Instance.playerDeck;
            playerDeck.unlockedCardIDs = data.unlockedCardIDs;
            playerDeck.playerDeckEntries = data.playerDeckEntries;
        }
    }
}
