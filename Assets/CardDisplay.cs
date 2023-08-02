using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardDisplay : MonoBehaviour
{
    public Transform cardDisplayArea;
    public Transform cardRemovalArea;

    private void Start()
    {
        PlayerDeck playerDeck = PlayerDeckHolder.Instance.playerDeck;
        List<int> unlockedCardIDs = playerDeck.unlockedCardIDs;

        int currentDiscardRow = 0;
        int cardsPerRow = 5;
        int cardCountInCurrentRow = 0;

        // Display unlocked cards in the removal area
        foreach (int cardID in unlockedCardIDs)
        {
            CardDatabaseEntry cardEntry = GetCardEntryByID(cardID);
            if (cardEntry != null)
            {
                GameObject cardPrefab = Resources.Load<GameObject>(cardEntry.PrefabResourcePath);
                if (cardPrefab != null)
                {
                    InstantiateCard(cardPrefab, cardRemovalArea, cardID, ref currentDiscardRow, ref cardCountInCurrentRow, cardsPerRow);
                }
            }
        }

        int currentDisplayRow = 0;
        int cardCount = 0;

        // Display player's standard deck in the display area
        foreach (var entry in playerDeck.playerDeckEntries)
        {
            int cardID = entry.CardID;
            int count = entry.CardCount;

            CardDatabaseEntry cardEntry = GetCardEntryByID(cardID);
            if (cardEntry != null)
            {
                GameObject cardPrefab = Resources.Load<GameObject>(cardEntry.PrefabResourcePath);
                if (cardPrefab != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        InstantiateCard(cardPrefab, cardDisplayArea, cardID, ref currentDisplayRow, ref cardCount, cardsPerRow);
                    }
                }
            }
        }
    }

    private CardDatabaseEntry GetCardEntryByID(int cardID)
    {
        CardDatabase cardDatabase = Resources.Load<CardDatabase>("CardDatabase");
        return cardDatabase.cardEntries.Find(e => e.CardID == cardID);
    }

    private void InstantiateCard(GameObject cardPrefab, Transform parent, int cardID, ref int currentRow, ref int cardCountInCurrentRow, int cardsPerRow)
    {
        GameObject instantiatedCard = Instantiate(cardPrefab);
        RectTransform rectTransform = instantiatedCard.AddComponent<RectTransform>();
        CardScriptDeck cardScript = instantiatedCard.AddComponent<CardScriptDeck>();
        cardScript.cardDisplay = this;

        // Set the card's ID
        cardScript.cardID = cardID;

        // Remove the Card script from the instantiated card
        Destroy(instantiatedCard.GetComponent<Card>());

        // Get the BoxCollider2D component attached to the GameObject
        BoxCollider2D boxCollider = instantiatedCard.GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            // Set the offset to (0, 0)
            boxCollider.offset = Vector2.zero;

            // Set the size to (70, 100)
            boxCollider.size = new Vector2(70f, 100f);
        }

        // Set the card's parent to the current row
        string rowPrefix = parent == cardRemovalArea ? "RRow" : "DRow";
        cardScript.removed = parent == cardRemovalArea ? true : false;
        Transform rowTransform = parent.Find(rowPrefix + (currentRow + 1));
        if (rowTransform != null)
        {
            instantiatedCard.transform.SetParent(rowTransform, false);
            cardCountInCurrentRow++;

            // Move to the next row if all slots in the current row are filled
            if (cardCountInCurrentRow >= cardsPerRow)
            {
                currentRow++;
                cardCountInCurrentRow = 0;
            }
        }
    }

    public void SaveDeckBuilderCards()
    {
        PlayerDeck playerDeck = PlayerDeckHolder.Instance.playerDeck;

        // Clear the existing player deck composition
        playerDeck.playerDeckEntries.Clear();

        // Get the number of rows in the card display area
        int numRows = cardDisplayArea.childCount;

        for (int i = 0; i < numRows; i++)
        {
            Transform rowTransform = cardDisplayArea.GetChild(i);
            int numCardsInRow = rowTransform.childCount;

            for (int j = 0; j < numCardsInRow; j++)
            {
                Transform cardTransform = rowTransform.GetChild(j);

                // Get the CardScriptDeck component attached to the card
                CardScriptDeck cardScript = cardTransform.GetComponent<CardScriptDeck>();
                if (cardScript != null)
                {
                    // Get the card's ID from the CardScriptDeck component
                    int cardID = cardScript.cardID;

                    // Check if the card already exists in the player deck entries
                    int existingIndex = playerDeck.playerDeckEntries.FindIndex(entry => entry.CardID == cardID);

                    // If the card exists, update its count, else add a new entry
                    if (existingIndex >= 0)
                    {
                        playerDeck.playerDeckEntries[existingIndex].CardCount++;
                    }
                    else
                    {
                        PlayerDeckEntry newEntry = new PlayerDeckEntry { CardID = cardID, CardCount = 1 };
                        playerDeck.playerDeckEntries.Add(newEntry);
                    }
                }
            }
        }

        // Save the updated player deck data
        DeckDataManager deckDataManager = FindObjectOfType<DeckDataManager>();
        if (deckDataManager != null)
        {
            deckDataManager.SavePlayerDeckData();
        }
        else
        {
            Debug.LogError("DeckDataManager not found in the scene.");
        }
    }

    public void LoadNextScene()
    {
        StartCoroutine(ChangeSceneAfterDelay());
    }

    private IEnumerator ChangeSceneAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("1-1");

    }
}

