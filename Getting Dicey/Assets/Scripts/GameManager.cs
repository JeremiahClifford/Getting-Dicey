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
    private bool isRolling = false;

    public float Money
    {
        get { return money; }
        set { money = value; }
    }

    private List<Die> allDice = new List<Die>();
    private List<Die> activeDice = new List<Die>();
    private List<Die> rolledDice = new List<Die>();

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

        Die die = Resources.Load<Die>("Prefabs/d6");
        allDice.Add(GameObject.Instantiate<Die>(die));
        allDice.Add(GameObject.Instantiate<Die>(die));
        foreach (Die i in allDice)
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

        if (isRolling)
        {
            for (int i = activeDice.Count - 1; i >= 0; i--)
            {
                if (!activeDice[i].rolling)
                {
                    rolledDice.Add(activeDice[i]);
                    activeDice.RemoveAt(i);
                }
            }
            if (activeDice.Count == 0)
            {
                FinishedRolling();
                isRolling = false;
                rolledDice.Clear();
            }
        }
    }

    /// <summary>
    /// Called when the roll button is pressed
    /// </summary>
    public void RollButton()
    {
        if (!isRolling)
        {
            isRolling = true;
            foreach (Die i in allDice)
            {
                RollDie(i);
            }
        }
    }

    /// <summary>
    /// Rolls an individual die
    /// </summary>
    /// <param name="die"></param>
    private void RollDie(Die die)
    {
        activeDice.Add(die);
        die.gameObject.SetActive(true);
        die.gameObject.transform.position = new Vector3(0f, 35f, -55f);
        die.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        die.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 35f);
    }

    /// <summary>
    /// Called when the dice have finished rolling
    /// </summary>
    private void FinishedRolling()
    {
        foreach (Die d in rolledDice)
        {
            Money += d.value;
        }
    }
}
