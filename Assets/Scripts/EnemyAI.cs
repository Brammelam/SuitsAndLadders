using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
        this.energy = 5;
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
        // Determine the current energy state
        if (gameManager.enemy.energy <= 3)
        {
            currentEnergyState = EnergyState.LowEnergy;
        }
        else
        {
            currentEnergyState = EnergyState.HighEnergy;
        }

        // Determine the current time state
        if ((gameManager.player.timeSpent - gameManager.enemy.timeSpent) <= 2)
        {
            currentTimeState = TimeState.LowTime;
        }
        else
        {
            currentTimeState = TimeState.HighTime;
        }

        Debug.Log("My energystate is: " + currentEnergyState + " and my timestate is: " + currentTimeState);

        // Determine the weights for each action
        Dictionary<Card, float> actionWeights = new Dictionary<Card, float>();
        float minWorkEnergy = float.MaxValue;

        foreach (Card card in enemyHand)
        {
            float energyWeight = 1.0f;
            float timeWeight = 1.0f;

            if (card.work > 0 && card.energy < minWorkEnergy)
            {
                minWorkEnergy = card.energy;
            }


            switch (currentEnergyState)
            {
                case EnergyState.LowEnergy:
                    // We're more likely to play "Energy" cards in this state
                    energyWeight = -card.energy * 1000.0f + 0.01f;
                    break;

                case EnergyState.HighEnergy:
                    // We're more likely to play "Productivity" cards in this state
                    // However, if we don't have enough energy to play any work card,
                    // we should consider playing an energy card
                    if (gameManager.enemy.energy < minWorkEnergy && card.energy < 0)
                    {
                        energyWeight = -card.energy * 1000.0f + 0.01f;
                    }
                    else
                    {
                        energyWeight = card.work * 2.0f + 0.01f;
                    }
                    break;

            }

            switch (currentTimeState)
            {
                case TimeState.LowTime:
                    // We're less likely to play cards that take a lot of time in this state
                    timeWeight = (gameManager.maxTime - card.time) * 2.0f + 0.01f;
                    break;

                case TimeState.HighTime:
                    // We're more likely to play cards that take a lot of time in this state
                    timeWeight = card.time * 2.0f + 0.01f;
                    break;
            }

            // Combine the weights from the energy and time states
            actionWeights[card] = energyWeight * timeWeight;

        }

        // Select an action based on these weights
        (Card selectedCard, int cardIndex) = WeightedRandom(actionWeights);

        // If a valid card is found, play it and move it to the discard pile
        if (selectedCard!=null)
        {
            gameManager.PlayCard(selectedCard, false);

            DisplayPlayedCard(selectedCard);
            StartCoroutine(MoveToEnemyDiscardPile(cardIndex));
            gameManager.PlayEnemyAttackAnimation();
            Debug.Log("I'm going to play " + selectedCard.name + " this turn! Because my states are: " + currentEnergyState + " for energy and " + currentTimeState + " for time! And I have " + gameManager.enemy.energy + " energy!");
        }
        else
        {
            Debug.Log("All these cards sucked! Ending my turn!");
            gameManager.EndEnemyTurn();
            yield break;
        }

        yield return new WaitForSeconds(2f);

        if (gameManager.player.timeSpent > gameManager.enemy.timeSpent)
        {
            StartCoroutine(PlayTurn());
        }
        else
        {
            Debug.Log("Time's up, ending my turn!");
            gameManager.EndEnemyTurn();
            yield break;
        }
    }

    public (Card, int) WeightedRandom(Dictionary<Card, float> weights)
    {
        float totalWeight = 0.0f;

        // Calculate the total weight of all items
        foreach (float weight in weights.Values)
        {
            totalWeight += weight;
        }

        // Choose a random value between 0 and the total weight
        float randomValue = Random.value * totalWeight;

        int cardIndex = 0;
        // Find the item that this random value corresponds to
        foreach (KeyValuePair<Card, float> kvp in weights)
        {
            // Check if card can be played before anything else
            if ((gameManager.enemy.energy >= kvp.Key.energy && kvp.Key.energy >= 0) || kvp.Key.energy < 0)
            {
                if (randomValue < kvp.Value)
                {
                    return (kvp.Key, cardIndex);
                }
                else
                {
                    // Subtract this item's weight from the random value
                    randomValue -= kvp.Value;
                }
            }
            else
            {
                // Skip the current card and adjust the total weight to reflect the skipped card
                totalWeight -= kvp.Value;
                randomValue = Random.value * totalWeight;
            }
            cardIndex += 1;
        }

        // If we haven't returned by this point, something has gone wrong,
        // e.g. because of floating point precision errors.
        // In this case, we just return null
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
        cardParent.transform.position = this.transform.position;
        cardParent.transform.rotation = Quaternion.LookRotation(cardParent.transform.position - Camera.main.transform.position);

        // Instantiate the prefab as a child of the new parent
        GameObject cardInstance = Instantiate(cardPrefabResource, cardParent.transform);

        // Move the card upwards over time, this could be replaced with an animation or any effect you want
        for (float t = 0; t <= 3; t += Time.deltaTime)
        {
            cardParent.transform.position += new Vector3(0, 0.01f, 0);
            yield return null;
        }

        Destroy(cardInstance);
        Destroy(cardParent);
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
