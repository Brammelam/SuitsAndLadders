using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The Card class is used for card-specific things<br>
/// Mouse-events like hovering, dragging etc</br><br>
/// The class also holds the values for the cards themselves, like cost</br>
/// </summary>
public class Card : MonoBehaviour
{
    public int time, energy, work;
    public TextMeshPro cardCost, cardText;
    public int workDoneBuff, workDoneDebuff, energyGainBuff, timeCostReduction;
    public bool affectAll, playOnSelf;
    private PlayerScript playerEntity;
    private Entity entity;
    private GameManager gm;
    private CardManager cardManager;

    public bool hasBeenPlayed;

    public int handIndex;
    private bool isFocused = false;
    public bool playedByEnemy = false;

    public Vector3 scale, position, mousePositionOffset;

    private float scaleFactor = 1.5f;
    private int moveUpFactor = 3;

    private BezierArrows arrow;
    public Canvas cameraCanvas;
    public Canvas overlayCanvas;

    public void UpdateCardDescriptionText(int energyCost, int workValue, string text)
    {
        // Replace "#" with the energy cost and "?" with the work value
        cardText.text = text.Replace("#", energyCost.ToString()).Replace("?", workValue.ToString());
    }

    public void UpdateCardCostText(int timeCost, string text)
    {
        // Replace "$" with the time cost
        cardCost.text = text.Replace("$", timeCost.ToString());
    }

    private Vector3 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Start()
    {
        cardText = GetComponentsInChildren<TextMeshPro>()
            .FirstOrDefault(textComponent => textComponent.gameObject.name == "text");
        cardCost = GetComponentsInChildren<TextMeshPro>()
            .FirstOrDefault(textComponent => textComponent.gameObject.name == "cost");
        
        // Set the initial values of the cards based on their parameters
        UpdateCardCostText(Mathf.Abs(time), cardCost.text);
        UpdateCardDescriptionText(Mathf.Abs(energy), Mathf.Abs(work), cardText.text);

        gm = FindObjectOfType<GameManager>();
        cardManager = FindObjectOfType<CardManager>();
        playerEntity = FindObjectOfType<PlayerScript>();
        arrow = FindObjectOfType<BezierArrows>();
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                overlayCanvas = canvas;
            }
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                cameraCanvas = canvas;
            }
        }

        transform.localScale = transform.localScale * 0.07f;
        scale = transform.localScale;
    }
    private void OnMouseDown()
    {
        if (gm.currentTurnState == GameManager.TurnState.PlayerTurn)
        {
            arrow.SetClickedCard(this);
            cardManager.playerIsDragginCard = true;
        }
    }

    private void OnMouseUp()
    {        
        if (gm.currentTurnState == GameManager.TurnState.PlayerTurn && cardManager.playerIsDragginCard)
        {
            arrow.DisableClickedCard();

            // Check if the mouse is over an enemy object
            RaycastHit2D enemyHit = Physics2D.Raycast(GetMouseWorldPosition(), Vector2.zero, 0f);
            if (enemyHit.collider != null && enemyHit.collider.CompareTag("Enemy"))
            {
                if (playOnSelf)
                {
                    ReturnCardToHand();
                    return;
                }
                else
                {
                    entity = enemyHit.collider.GetComponentInParent<Entity>();
                }
            }
            // Check if the mouse is over the player object and break if we are not playing a buff or other PlayOnSelf type card
            // or if we are not playing an AoE card
            else if (gameObject.CompareTag("Player"))
            {
                if (!playOnSelf) 
                {
                    ReturnCardToHand();
                    return;
                }
            }

            if (!hasBeenPlayed)
            {
                // Calculate the base cost of the card (without effects)
                int baseEnergyCost = energy;
                int baseTimeEffect = time;

                // Get the total effect value from the player's buffs
                int energyBuffValue = playerEntity.GetEffectValue("EnergyBuff");
                int timeCostReductionValue = playerEntity.GetEffectValue("TimeCostReduction");

                // Calculate the actual cost of the card after applying buffs
                int actualEnergyCost = Mathf.Max(baseEnergyCost - energyBuffValue, 0);
                int actualTimeCost = Mathf.Max(baseTimeEffect - timeCostReductionValue, 0);

                // Check if the player has enough energy and time to play the card
                if ((actualTimeCost + gm.player.timeSpent) <= gm.maxTime && (gm.player.energy - actualEnergyCost) >= 0)
                {
                    hasBeenPlayed = true;
                    
                    if (playOnSelf)
                    {
                        gm.PlayCard(this, playerEntity, playerEntity);
                    } else
                    {
                        gm.PlayCard(this, playerEntity, entity);
                    }

                    // Apply the card's energy gain or reduction buffs to the player
                    if (workDoneBuff != 0)
                    {
                        playerEntity.ApplyEffect("WorkDoneBuff", workDoneBuff); // Increases work done (buff)
                    }
                    if (workDoneDebuff != 0)
                    {
                        playerEntity.ApplyEffect("WorkDoneDebuff", workDoneDebuff); // Reduces work done (debuff)
                    }
                    if (energyGainBuff != 0)
                    {
                        playerEntity.ApplyEffect("EnergyBuff", energyGainBuff); // Reduces energy cost (buff)
                    }
                    if (timeCostReduction != 0)
                    {
                        playerEntity.ApplyEffect("TimeCostReduction", timeCostReduction); // Reduces time cost (buff)
                    }
                }
                // Return card to hand if we cant afford to play it
                else
                {
                    ReturnCardToHand();
                }
            }
            // Catch edgecase where we play a HasBeenPlayedCard
            else
            {
                ReturnCardToHand();
            }
        }

        cardManager.playerIsDragginCard = false;
    }

    private void ReturnCardToHand()
    {
        transform.localScale = scale;
        transform.position = cardManager.cardSlots[handIndex].position;
    }

    private void OnMouseOver()
    {
        isFocused = true;
        if (!cardManager.playerIsDragginCard)
        {
            if (transform.localScale == scale)
            {
                this.GetComponent<SortingGroup>().sortingOrder = 100;
                transform.localScale = scale * scaleFactor;
                transform.position += Vector3.up * moveUpFactor;
            }
        }        
    }

    private void OnMouseExit()
    {
        isFocused = false;
        if (!cardManager.playerIsDragginCard && !hasBeenPlayed)
        {
            this.GetComponent<SortingGroup>().sortingOrder = 1;
            transform.localScale = scale;
            transform.position = cardManager.cardSlots[handIndex].position;
        }     
    }

    private void FixedUpdate()
    {
        if (!isFocused && !playedByEnemy) transform.position = cardManager.cardSlots[handIndex].position;
    }
}

