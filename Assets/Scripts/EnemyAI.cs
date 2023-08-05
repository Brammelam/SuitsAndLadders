using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// This EnemyAI inherits from the Entity class<br>
/// This is the base which all enemies inherit from</br><br>
/// Subcategories of enemies will have different levels of energy, AI behavior etc</br>
/// </summary>
public class EnemyAI : Entity
{
    public GameManager gameManager;
    public List<Card> enemyDeck = new List<Card>();
    public List<Card> enemyDiscardPile = new List<Card>();
    public List<Card> enemyHand = new List<Card>(5);

    public TextMeshProUGUI enemyEnergyText;
    public TextMeshProUGUI enemyWorkDoneText;

    public int enemyEnergy, enemyWorkDone, enemyTimeSpent;
    public bool turnFinished;

    private int cardLayerOrder = 15;

    public enum EnergyState
    {
        LowEnergy,
        HighEnergy,
    }

    public enum TimeState
    {
        LowTime,
        HighTime,
    }

    public EnergyState currentEnergyState;
    public TimeState currentTimeState;

    private void Start()
    {
        maxEnergy = 5;
        this.energy = maxEnergy;
        this.workDone = 0;

        this.energyText.text = this.energy.ToString();
        this.workDoneText.text = this.workDone.ToString();

        StartCoroutine(DrawEnemyHand());
    }

    private IEnumerator DrawEnemyHand()
    {
        for (int i = 0; i < 5; i++)
        {
            // Wait between each call
            yield return new WaitForSeconds(.3f);
            DrawEnemyCard();
        }
    }

    public void DrawEnemyCard()
    {
        // Break out if there are no cards in the deck and no cards in discard to shuffle
        if (enemyDeck.Count == 0 && enemyDiscardPile.Count == 0) return;

        // Also break out if we have a full hand of 5 cards
        if (enemyHand.Count >= 5) return;

        if (enemyDeck.Count >= 1)
        {
            Card randEnemyCard = enemyDeck[Random.Range(0, enemyDeck.Count)];

            randEnemyCard.gameObject.SetActive(true);
            randEnemyCard.hasBeenPlayed = false;

            enemyHand.Add(randEnemyCard);
            enemyDeck.Remove(randEnemyCard);
            return;

        } 
        else
        {
            ShuffleEnemyDeck();
            DrawEnemyCard();
        }
    }

    public void ShuffleEnemyDeck()
    {
        if (enemyDiscardPile.Count > 0)
        {
            foreach(Card card in enemyDiscardPile)
            {
                enemyDeck.Add(card);
            }
            enemyDiscardPile.Clear();
        }
    }

    public IEnumerator PlayTurn()
    {
        Debug.Log("Enemy has " + this.energy + " energy");        
        // Determine the current energy state
        if (gameManager.enemy.energy <= 3)
        {
            currentEnergyState = EnergyState.LowEnergy;
        }
        else
        {
            currentEnergyState = EnergyState.HighEnergy;
        }        

        // Determine the weights for each action
        Dictionary<Card, float> actionWeights = new Dictionary<Card, float>();

        foreach (Card card in enemyHand)
        {
            float energyWeight = 1.0f;

            switch (currentEnergyState)
            {
                case EnergyState.LowEnergy:
                    // We're more likely to play "Energy" cards in this state
                    if (card.energy < 0) // cards that increase energy
                    {
                        energyWeight = 1000.0f;
                    }
                    else
                    {
                        energyWeight = 0.01f;
                    }
                    break;

                case EnergyState.HighEnergy:
                    // We're more likely to play "Attack" cards in this state
                    if (card.energy > 0) // cards that decrease energy (presumably attack cards)
                    {
                        energyWeight = 1000.0f;
                    }
                    else
                    {
                        energyWeight = 0.01f;
                    }
                    break;
            }

            // Assign the weight from the energy state
            actionWeights[card] = energyWeight;
        }

        // Select an action based on these weights
        (Card selectedCard, int cardIndex) = WeightedRandom(actionWeights);

        // If a valid card is found, play it and move it to the discard pile
        if (selectedCard!=null)
        {
            Debug.Log("I'm going to play " + selectedCard.name + " this turn! Because my states are: " + currentEnergyState + " for energy. And I have " + gameManager.enemy.energy + " energy!");
            gameManager.PlayCard(selectedCard, this, this);
            StartCoroutine(MoveToEnemyDiscardPile(cardIndex));

            DisplayPlayedCard(selectedCard);
            gameManager.PlayEnemyAttackAnimation();
        }
        else
        {
            Debug.Log("All these cards sucked! Ending my turn!");
            turnFinished = true;
        }
        yield return new WaitForSeconds(3f);
    }

    public (Card, int) WeightedRandom(Dictionary<Card, float> weights)
    {
        int cardIndex = 0;
        float totalWeight = 0.0f;

        // Calculate the total weight of all playable items and find the item that the random value corresponds to
        foreach (KeyValuePair<Card, float> kvp in weights)
        {
            // Check if card can be played before anything else
            if ((this.energy >= kvp.Key.energy && kvp.Key.energy >= 0) || kvp.Key.energy < 0)
            {
                // Add this card's weight to the total weight
                totalWeight += kvp.Value;

                // Choose a random value between 0 and the total weight
                float randomValue = Random.value * totalWeight;

                if (randomValue <= kvp.Value)
                {
                    return (kvp.Key, cardIndex);
                }
            }
            cardIndex += 1;
        }

        // If we haven't returned by this point, there were no valid cards to play.
        return (null, -1);
    }

    private void DisplayPlayedCard(Card card)
    {
        StartCoroutine(DisplayedCardCoroutine(card));
    }

    private IEnumerator DisplayedCardCoroutine(Card card)
    {
        // Load the prefab from the resources folder
        GameObject cardPrefabResource = Resources.Load<GameObject>($"Cards/{card.name}");

        // Create a new parent object at the enemy's position
        GameObject cardParent = new GameObject("CardParent");
        cardParent.transform.position = this.transform.position - new Vector3(10,3);
        cardParent.transform.rotation = Quaternion.LookRotation(cardParent.transform.position - Camera.main.transform.position);

        // Instantiate the prefab as a child of the new parent
        GameObject cardInstance = Instantiate(cardPrefabResource, cardParent.transform);
        cardInstance.GetComponent<Card>().playedByEnemy = true;
        cardInstance.transform.localScale *= 1.5f;
        cardInstance.GetComponent<SortingGroup>().sortingOrder = cardLayerOrder;
        cardLayerOrder += 1;
        // Move the card upwards over time, this could be replaced with an animation or any effect you want
        for (float t = 0; t <= 5; t += Time.deltaTime)
        {
            cardParent.transform.position += new Vector3(0, 0.01f, 0);
            yield return null;
        }

        Destroy(cardInstance, 3f);
        Destroy(cardParent, 3f); 
    }

    // Moves the played card to the Enemy's discard pile
    IEnumerator MoveToEnemyDiscardPile(int cardIndex)
    {
        Card card = enemyHand[cardIndex];
        enemyHand.RemoveAt(cardIndex);
        enemyDiscardPile.Add(card);
        Debug.Log("Enemy hand is now at : " + enemyHand.Count + " and enemy discard pile is at " + enemyDiscardPile.Count);
        yield return null;
    }
}
