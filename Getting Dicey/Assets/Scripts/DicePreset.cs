using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to define presets for dice. 
/// </summary>
[CreateAssetMenu(fileName = "Dice Preset", menuName = "Getting Dicey/Dice Preset")]
public class DicePreset : ScriptableObject
{
    [SerializeField]
    private DieBehavior prefab;

    [SerializeField]
    private List<int> values = new List<int>();

    public List<int> Values
    {
        get
        {
            return new List<int>(values);
        }
    }

    public int GetRandomRoll()
    {
        int index = Random.Range(0, values.Count);
        return values[index];
    }

    /// <summary>
    /// Creates a new die gameobject
    /// </summary>
    /// <returns>A new instance of the die defined by this preset.</returns>
    public DieBehavior GetDiceObject()
    {
        DieBehavior diceObject = GameObject.Instantiate<DieBehavior>(prefab);
        diceObject.Values = new List<int>(Values);
        return diceObject;
    }
}