using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text moneyLabel;

    private float money;
    private int index;

    public float Money
    {
        get { return money; }
        set { money = value; }
    }

    private List<Die> dice = new List<Die>();

    /// <summary>
    /// Called before the first active frame
    /// </summary>
    public void Start()
    {
        DicePreset dicePreset = Resources.Load<DicePreset>("DicePresets/D6Preset");
        dice.Add(dicePreset.GetDiceObject());
        dice.Add(dicePreset.GetDiceObject());
        foreach (Die i in dice)
        {
            i.gameObject.SetActive(false);
        }
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
        dice[index].gameObject.SetActive(true);
        dice[index].gameObject.transform.position = new Vector3(0f, 0f, 0f);
        dice[index].gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        index++;
        if (index >= dice.Count)
        {
            index = 0;
        }
    }
}
