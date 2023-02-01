using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text moneyLabel;

    private float money;
    public float Money
    {
        get { return money; }
        set { money = value; }
    }

    private List<DieBehavior> dice = new List<DieBehavior>();

    /// <summary>
    /// Called before the first active frame
    /// </summary>
    public void Start()
    {
        DicePreset dicePreset = Resources.Load<DicePreset>("DicePresets/D6Preset");
        dice.Add(dicePreset.GetDiceObject());
        dice.Add(dicePreset.GetDiceObject());
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        moneyLabel.text = "$" + Money;
    }

    /// <summary>
    /// Called when the roll button is pressed.
    /// </summary>
    public void Roll()
    {
        List<int> results = new List<int>();
    }
}
