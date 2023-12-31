using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// The CardManager is responsible for cardlogic<br>
/// Stuff like drawing cards, shuffling and applying effects from cards</br>
/// </summary>
public class CardManager : MonoBehaviour
{
    public PlayerDeck playerDeck = PlayerDeckHolder.Instance.playerDeck;

    public List<Card> hand;
    public List<Card> deck;
    public List<Card> discardPile;
    private GameObject cardSlotParent;
    private List<bool> availableCardSlots = new List<bool>();
    public List<Transform> cardSlots = new List<Transform>();
    private GameManager gameManager;
    private Dictionary<int, int> playerDeckComposition;
    public GameObject paperPilePrefab;
    public Transform paperPileParent;
    public bool playerIsDragginCard = false;

    public CardManager(List<Transform> cardSlots = null)
    {
        this.hand = new List<Card>(5);

        if (cardSlots != null)
        {
            this.cardSlots = cardSlots;
            this.availableCardSlots = new List<bool>(cardSlots.Count);
            for (int i = 0; i < cardSlots.Count; i++)
            {
                this.availableCardSlots.Add(true);
            }
        }
    }

    private void Start()
    {
        this.hand = new List<Card>(5);
        
        gameManager = FindObjectOfType<GameManager>();
        cardSlotParent = GameObject.FindGameObjectWithTag("CardSlotParent");

        foreach (Transform child in cardSlotParent.transform)
        {
            cardSlots.Add(child);
            child.gameObject.SetActive(false);
        }

        if (cardSlots != null)
        {
            this.availableCardSlots = new List<bool>(cardSlots.Count);
            for (int i = 0; i < cardSlots.Count; i++)
            {
                this.availableCardSlots.Add(true);
            }
        }

        this.deck = new List<Card>();
        LoadCardsFromPlayerDeck(playerDeck);

        StartCoroutine(DrawNewHand());
    }

    private void Update()
    {
    }

    private void LoadCardsFromPlayerDeck(PlayerDeck playerDeck)
    {
        CardDatabase cardDatabase = Resources.Load<CardDatabase>("CardDatabase");

        foreach (var entry in playerDeck.playerDeckEntries)
        {
            int cardID = entry.CardID;
            int cardCount = entry.CardCount;

            CardDatabaseEntry cardEntry = cardDatabase.cardEntries.Find(e => e.CardID == cardID);
            if (cardEntry != null)
            {
                GameObject cardPrefab = Resources.Load<GameObject>(cardEntry.PrefabResourcePath);
                if (cardPrefab != null)
                {
                    for (int i = 0; i < cardCount; i++)
                    {
                        Card instantiatedCard = Instantiate(cardPrefab).GetComponent<Card>();
                        instantiatedCard.gameObject.SetActive(false);
                        this.deck.Add(instantiatedCard);
                    }
                }
            }
        }
    }


    private IEnumerator DrawNewHand()
    {
        for (int i = 0; i < 5; i++)
        {
            // Wait between each call
            yield return new WaitForSeconds(.1f);

            DrawCard();
        }

        // When we finish drawing our hand, start the game
        gameManager.StartGame();
    }

    public void Shuffle()
    {
        if (discardPile.Count > 0)
        {
            foreach (Card card in discardPile)
            {
                card.transform.localScale = card.scale;
                this.deck.Add(card);
            }
            discardPile.Clear();
        }
    }

    public Vector3 GetCardSlotPosition(int handIndex)
    {
        return cardSlots[handIndex].position;
    }

    public void DrawCard()
    {
        // Break out if there are no cards in the deck and no cards in discard to shuffle
        if (deck.Count == 0 && discardPile.Count == 0) return;

        // Card limit reached
        if (hand.Count >= 10) return;

        if (deck.Count > 0)
        {
            Card randCard = deck[Random.Range(0, deck.Count)];
            AddToHand(randCard);
            deck.Remove(randCard);
        }
        else
        {
            Shuffle();
            DrawCard();
        }
    }

    public void AddToHand(Card card)
    {
        if (availableCardSlots != null)
        {
            for (int i = 0; i < availableCardSlots.Count; i++)
            {
                if (availableCardSlots[i])
                {
                    cardSlots[i].gameObject.SetActive(true);
                    card.gameObject.SetActive(true);

                    card.handIndex = i;
                    card.transform.position = cardSlots[i].position;
                    card.position = card.transform.position;
                    card.hasBeenPlayed = false;

                    availableCardSlots[i] = false;
                    hand.Add(card);
                    card.GetComponent<BoxCollider2D>().enabled = true;
                    return;
                }
            }
        }
        
    }

    public void RemoveFromHand(Card card)
    {
        hand.Remove(card);
        if (availableCardSlots != null)
        {
            availableCardSlots[card.handIndex] = true;
            cardSlots[card.handIndex].gameObject.SetActive(false);

            card.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    public void DisableCardVisuals(GameObject card)
    {
        // Disable the SpriteRenderer on the card
        SpriteRenderer cardSR = card.GetComponent<SpriteRenderer>();
        if (cardSR != null)
        {
            cardSR.enabled = false;
        }

        // Loop through each child of the card
        foreach (Transform child in card.transform)
        {
            // Disable the SpriteRenderer on the child
            SpriteRenderer childSR = child.GetComponent<SpriteRenderer>();
            if (childSR != null)
            {
                childSR.enabled = false;
            }

            // Disable the TextMesh on the child
            TextMeshPro childTM = child.GetComponent<TextMeshPro>();
            if (childTM != null)
            {
                childTM.enabled = false;
            }
        }
    }

    public void EnableCardVisuals(GameObject card)
    {
        // Enable the SpriteRenderer on the card
        SpriteRenderer cardSR = card.GetComponent<SpriteRenderer>();
        if (cardSR != null)
        {
            cardSR.enabled = true;
        }

        // Loop through each child of the card
        foreach (Transform child in card.transform)
        {
            // Enable the SpriteRenderer on the child
            SpriteRenderer childSR = child.GetComponent<SpriteRenderer>();
            if (childSR != null)
            {
                childSR.enabled = true;
            }

            // Enable the TextMesh on the child
            TextMeshPro childTM = child.GetComponent<TextMeshPro>();
            if (childTM != null)
            {
                childTM.enabled = true;
            }
        }
    }

    public void DisableAllCardVisuals()
    {
        foreach (var card in hand)
        {
            DisableCardVisuals(card.gameObject);
        }
    }

    public void EnableAllCardVisuals()
    {
        foreach (var card in hand)
        {
            EnableCardVisuals(card.gameObject);
        }
    }

    public IEnumerator SpawnMultiplePaperPiles(int numberOfPiles)
    {
        // Wait duration between each paper pile spawn
        float waitDuration = 0.2f;

        for (int i = 0; i < numberOfPiles; i++)
        {
            int offset = i * 2;
            SpawnPaperPile(offset);
            yield return new WaitForSeconds(waitDuration);
        }
    }

    void SpawnPaperPile(int offset)
    {
        // Create the paper pile
        GameObject paperPile = Instantiate(paperPilePrefab);

        // Choose a random X position for the paper pile
        float spawnX = Random.Range(transform.position.x - 2, transform.position.x + 2);
        paperPile.transform.position = new Vector3(spawnX, 15, offset);
    }
}