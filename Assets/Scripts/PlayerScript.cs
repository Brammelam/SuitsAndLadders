using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// This PlayerScript inherits from the Entity class<br>
/// We default to 8 energy for now</br>
/// </summary>
/// 

public class PlayerScript : Entity
{
    // Start is used instead of Awake
    private void Start()
    {
        this.energy = 8;
        this.workDone = 0;

        this.energyText.text = this.energy.ToString();
        this.workDoneText.text = this.workDone.ToString();
    }
}
