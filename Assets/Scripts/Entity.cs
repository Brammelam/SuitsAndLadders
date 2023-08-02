using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// This is the base class for Player and all Enemies, PlayerScript and EnemyAI inherit from this class
/// </summary>
public class Entity : MonoBehaviour
{
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI workDoneText;
    public Transform effectTransform;
    public int energy, workDone;
    public int timeSpent;

    protected Dictionary<string, int> effects = new Dictionary<string, int>();
    private Dictionary<string, GameObject> effectIcons = new Dictionary<string, GameObject>();

    // Assigns UI elements to the entity, atm just Energy and WorkDone
    protected virtual void Awake()
    {
        FindAndAssignUITexts();

        timeSpent = 0;
    }

    protected void FindAndAssignUITexts()
    {
        energyText = GetComponentsInChildren<TextMeshProUGUI>()
            .FirstOrDefault(textComponent => textComponent.gameObject.name == "energyText");

        workDoneText = GetComponentsInChildren<TextMeshProUGUI>()
            .FirstOrDefault(textComponent => textComponent.gameObject.name == "workText");
        
        effectTransform = GetComponentsInChildren<Transform>()
            .FirstOrDefault(textComponent => textComponent.gameObject.name == "effects");
    }

    public void UpdateInfo(int _energy, int _workDone)
    {
        energy -= _energy;
        workDone += _workDone;

        energyText.text = energy.ToString();
        workDoneText.text = workDone.ToString();
    }

    public virtual void ApplyEffect(string effectType, int effectValue)
    {
        if (effects.ContainsKey(effectType))
        {
            effects[effectType] += effectValue;
            UpdateEffectCounter(effectType);

            // Update the buff indicator with the new value
            ShowBuffIndicator(effectType);
        }
        else
        {
            effects.Add(effectType, effectValue);
        }

        // Add buff indicator based on the effect type
        if (effectType == "EnergyBuff" || effectType == "EnergyDebuff")
        {
            ShowBuffIndicator("EnergyBuff");
        }
    }

    public virtual void RemoveEffect(string effectType)
    {
        if (effects.ContainsKey(effectType))
        {
            effects.Remove(effectType);
        }
    }

    public virtual int GetEffectValue(string effectType)
    {
        return effects.ContainsKey(effectType) ? effects[effectType] : 0;
    }

    protected void ShowBuffIndicator(string effectType)
    {
        GameObject effectIconPrefab;
        int buffValue = GetEffectValue(effectType);

        // Determine the appropriate prefab based on the buffType
        switch (effectType)
        {
            case "EnergyBuff":
                effectIconPrefab = Resources.Load<GameObject>("Items/energyIcon");
                break;
            // Add cases for other buff types and their corresponding icons
            default:
                effectIconPrefab = null;
                break;
        }

        if (effectIconPrefab != null)
        {
            if (effectIcons.TryGetValue(effectType, out GameObject existingIcon))
            {
                // Buff icon already exists, update the counter if needed
                TextMeshPro counterText = existingIcon.GetComponentInChildren<TextMeshPro>();
                if (buffValue >= 2)
                {
                    counterText.text = buffValue.ToString();
                    counterText.gameObject.SetActive(true);
                }
                else
                {
                    // Hide the counter if the buff value is less than 2
                    counterText.gameObject.SetActive(false);
                }
            }
            else
            {
                // Buff icon doesn't exist, create a new one
                GameObject effectIcon = Instantiate(effectIconPrefab);
                effectIcon.transform.SetParent(effectTransform);

                if (buffValue >= 2)
                {
                    // Enable the counter text component
                    TextMeshPro counterText = effectIcon.GetComponentInChildren<TextMeshPro>();
                    counterText.text = buffValue.ToString();
                    counterText.gameObject.SetActive(true);
                }

                effectIcons[effectType] = effectIcon;
            }
        }
    }

    protected void RemoveBuffIndicator(string effectType)
    {
        if (effectIcons.TryGetValue(effectType, out GameObject effectIcon))
        {
            Destroy(effectIcon);
            effectIcons.Remove(effectType);
        }
    }

    protected void UpdateEffectCounter(string effectType)
    {
        if (effectIcons.ContainsKey(effectType))
        {
            TextMeshPro counterText = effectIcons[effectType].GetComponentInChildren<TextMeshPro>();
            int buffValue = GetEffectValue(effectType);
            counterText.text = buffValue.ToString();
            counterText.gameObject.SetActive(buffValue >= 2);
        }
    }
}
