using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text moneyLabel;

    [SerializeField]
    private InputAction rollDiceAction;

    private float money;

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
        rollDiceAction.Enable();
        rollDiceAction.performed += (InputAction.CallbackContext obj) =>
        {
            RollButton();
        };

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
    public void RollButton()
    {
        foreach (Die i in dice)
        {
            RollDie(i);
        }
    }

    private void RollDie(Die die)
    {
        die.gameObject.SetActive(true);
        die.gameObject.transform.position = new Vector3(0f, 35f, -55f);
        die.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        die.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 35f);
    }
}
