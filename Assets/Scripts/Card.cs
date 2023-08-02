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
    private PlayerScript playerEntity;
    private GameManager gm;

    public bool hasBeenPlayed;

    public int handIndex;

    public Vector3 scale, position, mousePositionOffset;

    public BoxCollider2D dropZone;

    private bool isDragging;

    private float scaleFactor = 1.5f;
    private int moveUpFactor = 3;

    private bool isPlayerTurn = false;

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

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
    }

    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
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
        playerEntity = FindObjectOfType<PlayerScript>();

        dropZone = GameObject.FindGameObjectWithTag("DropZone").GetComponent<BoxCollider2D>();
        transform.localScale = transform.localScale * 0.07f;
        scale = transform.localScale;

        isDragging = false;
    }

    private void OnMouseDown()
    {
        if (isPlayerTurn)
        {
            mousePositionOffset = gameObject.transform.position - GetMouseWorldPosition();
        }
    }

    private void OnMouseDrag()
    {
        if (isPlayerTurn)
        {
            if(!isDragging) isDragging = true;
            transform.position = GetMouseWorldPosition() + mousePositionOffset;
        }
    }

    private void OnMouseUp()
    {
        if (isPlayerTurn)
        {
            // check if we released in the DropZone
            if (dropZone.OverlapPoint(GetMouseWorldPosition()))
            {
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

                    // Check if the player has enough energy and time to play the card
                    if ((actualTimeCost + gm.player.timeSpent) <= gm.maxTime && (gm.player.energy - actualEnergyCost) >= 0)
                    {
                        hasBeenPlayed = true;
                        gm.PlayCard(this, true);
                    }
                    else
                    {
                        ReturnCardToHand();
                    }
                }
            }

            else
            {
                ReturnCardToHand();
            }

            isDragging = false;
        }
    }

    private void ReturnCardToHand()
    {
        transform.localScale = scale;
        transform.position = position;
    }

    private void OnMouseOver()
    {
        if (!isDragging)
        {
            if (transform.localScale == scale)
            {
                this.GetComponent<SortingGroup>().sortingOrder = 100;
                transform.localScale = scale * scaleFactor;
                transform.position += Vector3.up * moveUpFactor;
            }
        }
        else
        {
            transform.localScale = scale * scaleFactor;
        }
    }

    private void OnMouseExit()
    {
        this.GetComponent<SortingGroup>().sortingOrder = 1;
        if (!isDragging && !hasBeenPlayed)
        {
            transform.localScale = scale;
            transform.position = gm.cardSlots[handIndex].position;
        }     
    }
}

