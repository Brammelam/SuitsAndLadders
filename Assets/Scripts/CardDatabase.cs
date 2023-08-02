using System.Collections.Generic;
using UnityEngine;

// Card database entry structure
[System.Serializable]
public class CardDatabaseEntry
{
    public int CardID;
    public string CardName;
    public string Description;
    public string PrefabResourcePath;
}

// Card database scriptable object
[CreateAssetMenu(fileName = "New Card Database", menuName = "Custom/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<CardDatabaseEntry> cardEntries = new List<CardDatabaseEntry>();
}

// Card loader script
public class CardLoader : MonoBehaviour
{
    public CardDatabase cardDatabase;

    private Dictionary<int, GameObject> cardPrefabsCache = new Dictionary<int, GameObject>();

    // Function to load a card prefab by CardID
    public GameObject LoadCardPrefab(int cardID)
    {
        if (cardPrefabsCache.TryGetValue(cardID, out GameObject cachedCardPrefab))
        {
            return cachedCardPrefab; // Return cached card if already loaded
        }
        else
        {
            CardDatabaseEntry cardEntry = cardDatabase.cardEntries.Find(entry => entry.CardID == cardID);
            if (cardEntry != null)
            {
                GameObject cardPrefab = Resources.Load<GameObject>(cardEntry.PrefabResourcePath);
                cardPrefabsCache.Add(cardID, cardPrefab); // Cache the loaded card for future use
                return cardPrefab;
            }
            else
            {
                Debug.LogError("Card with ID " + cardID + " not found in the database!");
                return null;
            }
        }
    }
}