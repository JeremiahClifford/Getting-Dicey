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
    private TMP_Text outputLabel;
    [SerializeField]
    private TMP_Text newDiceCostLabel;
    [SerializeField]
    private TMP_Text turnsRemainingLabel;

    [SerializeField]
    private InputAction rollDiceAction;

    Die d6;

    private bool isRolling = false;

    //important variables for game logic
    public int money; //amount of money that the player has
    private int[] range; //holds the highest and lowest number that any of the player's dice can roll for optimization purposes while parsing results of a roll
    private int newDieCost; //tracks the cost of a new die, increasing with each purchase
    private int turnsRemaining; //tracks how many turns the player has left, decreasing with each roll
    private int debt; //stores how much debt the player has, currently is a static goal
    private int[] possibleSideNumbers; //stores how many sides any given dice can have

    private List<Die> allDice = new List<Die>();
    private List<Die> rollingDice = new List<Die>();
    private List<Die> rolledDice = new List<Die>();

    public List<o_Die> activeDice = new List<o_Die>(); //stores the dice that the player rolls
    public List<o_Die> inactiveDice = new List<o_Die>(); //stores the dice that the player owns but does not roll, currently unused

    /// <summary>
    /// Called before the first active frame
    /// </summary>
    public void Start()
    {
        // Press space to roll dice
        rollDiceAction.Enable();
        rollDiceAction.performed += (InputAction.CallbackContext obj) =>
        {
            RollButton();
        };

        // Creates starting inventory
        //Die die = Resources.Load<Die>("Prefabs/d6");
        d6 = Resources.Load<Die>("Prefabs/d6");
        //allDice.Add(GameObject.Instantiate<Die>(die));
        //allDice.Add(GameObject.Instantiate<Die>(die));

        money = 0; //sets money to 0
        SetMoneyLabel(); //updates the money label
        newDieCost = 30; //sets the new die cost to the default value
        SetNewDiceCostLabel();//updates the new die cost label
        turnsRemaining = 20; //set the number of remaining turns to the default value
        SetTurnsRemainingLabel(); //updates the turns remaining label
        debt = 500; //sets the debt to the default value
        possibleSideNumbers = new int[] {2, 4, 6, 8, 10, 12, 20}; //sets the list of possible side numbers
        for (int i = 0; i < 3; i++) { //adds the default number of dice (3) of random side numbers to the players active dice
            //activeDice.Add(new o_Die(possibleSideNumbers[Random.Range(0, possibleSideNumbers.Length)]));
            allDice.Add(GameObject.Instantiate<Die>(d6));
            allDice[i].gameObject.SetActive(false);
        }
        range = new int[2]; //sets up the range
        range[0] = 999999;
        range[1] = 0;
        CalculateRange(); //calculates the range
        //the below section adds the players active dice to the default text of the display at the start
        outputLabel.text += "<br>Your Active Dice:<br>";
        for (int i = 0; i < activeDice.Count; i++) {
            outputLabel.text += "d" + activeDice[i].sides.Length + ":";
            for (int j = 0; j < activeDice[i].sides.Length; j++) {
                outputLabel.text += " " + activeDice[i].sides[j];
            }
            outputLabel.text += "<br>";
        }
    }


    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        moneyLabel.text = "$" + money;

        if (isRolling)
        {
            for (int i = rollingDice.Count - 1; i >= 0; i--)
            {
                if (!rollingDice[i].rolling)
                {
                    rolledDice.Add(rollingDice[i]);
                    rollingDice.RemoveAt(i);
                }
            }
            if (rollingDice.Count == 0)
            {
                Debug.Log("Test");
                FinishedRolling();
                isRolling = false;
                rolledDice.Clear();
            }
        }
    }

    private void CalculateRange() { //used to calculate the range of possible rolls for optimization
        /*
        for (int i = 0; i < activeDice.Count; i++) {
            if (activeDice[i].sides[0] < range[0]) {
                range[0] = activeDice[i].sides[0];
            }
            if (activeDice[i].sides[activeDice[i].sides.Length-1] > range[1]) {
                range[1] = activeDice[i].sides[activeDice[i].sides.Length-1];
            }
        }
        */
        range[0] = 1;
        range[1] = 6;
    }

    /// <summary>
    /// Called when the roll button is pressed
    /// </summary>
    public void RollButton() //rolls all of the dice and parses the results when the roll button is pressed
    {
        if (!isRolling)
        {
            isRolling = true;
            foreach (Die i in allDice)
            {
                RollDie(i);
            }
        }

        //variables to hold info during parsing
        List<int> results = new List<int>();
        int totalPayout = 0;

        //rolls all the dice and saves their results to the results list
        for (int i = 0; i < activeDice.Count; i++) {
            results.Add(activeDice[i].Roll());
        }
        
        //displays the results to the screen
        outputLabel.text = "Results:";
        for (int i = 0; i < results.Count; i++) {
            outputLabel.text += " " + results[i];
        }

        //parses the results, calculating the total payout and writing it to the display
        for (int i = range[0]; i < range[1]+1; i++) {
            int numberOfInstances = 0;
            for (int j = 0; j < results.Count; j++) {
                if (results[j] == i) {
                    numberOfInstances++;
                }
            }
            if (numberOfInstances > 1) {
                outputLabel.text += "<br>" + numberOfInstances + " " + i + "'s : " + (i * (numberOfInstances - 1) * 10);
                totalPayout += i * (numberOfInstances - 1) * 10;
            }
        }
        outputLabel.text += "<br>Total Payout: " + totalPayout;
        money += totalPayout;
        SetMoneyLabel();

        //checks if the player wins or loses
        if (money >= debt) {
            outputLabel.text += "<br>Game over:<br>You Win";
        } else {
            turnsRemaining--;
            SetTurnsRemainingLabel();
            if (turnsRemaining <= 0) {
                outputLabel.text += "<br>Game over:<br>You have Run out of time";
            }
        }
    }

    public void BuyButton() { //adds a new dice if the player has enough money when the buy button is pressed
        if (money >= newDieCost) {
            allDice.Add(GameObject.Instantiate<Die>(d6));
            allDice[allDice.Count-1].gameObject.SetActive(false);
            //activeDice.Add(new o_Die(possibleSideNumbers[Random.Range(0, possibleSideNumbers.Length)]));
            money -= newDieCost;
            newDieCost += 10;
            SetMoneyLabel();
            SetNewDiceCostLabel();
            ViewButton();
        } else {
            outputLabel.text = "Insufficient Money to purchase a new die.";
        }
    }

    public void ViewButton() { //shows what dice the player has when the view button is pressed or when a new die is bought
        outputLabel.text = "Your Active Dice:<br>";
        for (int i = 0; i < activeDice.Count; i++) {
            outputLabel.text += "d" + activeDice[i].sides.Length + ":";
            for (int j = 0; j < activeDice[i].sides.Length; j++) {
                outputLabel.text += " " + activeDice[i].sides[j];
            }
            outputLabel.text += "<br>";
        }
        /*
        outputLabel.text += "Your Inactive Dice:<br>";
        for (int i = 0; i < inactiveDice.Count; i++) {
            outputLabel.text += "d" + inactiveDice[i].sides.Length + ":";
            for (int j = 0; j < inactiveDice[i].sides.Length; j++) {
                outputLabel.text += " " + inactiveDice[i].sides[j];
            }
            outputLabel.text += "<br>";
        }
        */
    }

    private void SetMoneyLabel() { //sets the money label
        moneyLabel.text = "$" + money;
    }

    private void SetNewDiceCostLabel() { //sets the new die cost label
        newDiceCostLabel.text = "New Dice Cost: " + newDieCost;
    }

    private void SetTurnsRemainingLabel() { //sets the turns remaining label
        turnsRemainingLabel.text = "Turns Remaining: " + turnsRemaining;
    }

    /// <summary>
    /// Rolls an individual die
    /// </summary>
    /// <param name="die"></param>
    private void RollDie(Die die)
    {
        rollingDice.Add(die);
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
        List<int> results = new List<int>();
        int totalPayout = 0;

        foreach (Die d in rolledDice)
        {
            results.Add(d.value);
        }

        //displays the results to the screen
        outputLabel.text = "Results:";
        for (int i = 0; i < results.Count; i++)
        {
            outputLabel.text += " " + results[i];
        }

        //parses the results, calculating the total payout and writing it to the display
        for (int i = range[0]; i < range[1] + 1; i++)
        {
            int numberOfInstances = 0;
            for (int j = 0; j < results.Count; j++)
            {
                if (results[j] == i)
                {
                    numberOfInstances++;
                }
            }
            if (numberOfInstances > 1)
            {
                outputLabel.text += "<br>" + numberOfInstances + " " + i + "'s : " + (i * (numberOfInstances - 1) * 10);
                totalPayout += i * (numberOfInstances - 1) * 10;
            }
        }
        outputLabel.text += "<br>Total Payout: " + totalPayout;
        money += totalPayout;
        SetMoneyLabel();

        //checks if the player wins or loses
        if (money >= debt)
        {
            outputLabel.text += "<br>Game over:<br>You Win";
        }
        else
        {
            turnsRemaining--;
            SetTurnsRemainingLabel();
            if (turnsRemaining <= 0)
            {
                outputLabel.text += "<br>Game over:<br>You have Run out of time";
            }
        }
    }
}
