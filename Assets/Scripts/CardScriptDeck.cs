using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CardScriptDeck : MonoBehaviour
{
    private Vector3 scale;
    private float scaleFactor = 2f;
    public int cardID;

    public CardDisplay cardDisplay;
    public Transform cardDisplayArea;
    public Transform cardRemovalArea;

    public bool removed;


    // Start is called before the first frame update
    void Start()
    {
        scale = transform.localScale;
        cardDisplayArea = GameObject.FindGameObjectWithTag("DisplayArea").GetComponent<Transform>();
        cardRemovalArea = GameObject.FindGameObjectWithTag("RemovalArea").GetComponent<Transform>();
    }

    private void OnMouseUp()
    {
        if (!removed)
        {
            // Loop through each row to find an available slot
            for (int i = 1; i <= 10; i++)
            {
                Transform row = cardRemovalArea.Find("RRow" + i); // Find the row by name (e.g., "Row1", "Row2", etc.)

                if (row != null && row.childCount < 5)
                {
                    this.transform.SetParent(row);
                    this.removed = true;
                    return; // Exit the loop after placing the card in the first available row
                }
            }
        }
        else
        {
            // Loop through each row to find an available slot
            for (int i = 1; i <= 4; i++)
            {
                Transform row = cardDisplayArea.Find("DRow" + i); // Find the row by name (e.g., "Row1", "Row2", etc.)

                if (row != null && row.childCount < 5)
                {
                    this.transform.SetParent(row);
                    this.removed = false;
                    return; // Exit the loop after placing the card in the first available row
                }
            }
        }
        
    }

    private void OnMouseOver()
    {
        if (transform.localScale == scale)
        {
            this.GetComponent<SortingGroup>().sortingOrder = 100;
            transform.localScale = scale * scaleFactor;
        }   
    }
    private void OnMouseExit()
    {
        this.GetComponent<SortingGroup>().sortingOrder = 1;
        transform.localScale = scale;
    }
}
