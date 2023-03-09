using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private GameObject tutorialManager;

    [SerializeField]
    private TMP_Text moneyLabel;
    [SerializeField]
    private TMP_Text outputLabel;
    [SerializeField]
    private TMP_Text turnsRemainingLabel;
    [SerializeField]
    private TMP_Text debtLabel;
    [SerializeField]
    private CanvasRenderer debugPanel;

    //inventory
    [SerializeField]
    private GameObject inventoryPanel;
    [SerializeField]
    private TMP_Text activeDiceListLabel;

    //shop
    [SerializeField]
    private GameObject shopPanel;
    [SerializeField]
    private List<TMP_Text> dieForSale;

    //bank
    [SerializeField]
    private GameObject bankPanel;

    //payout guide
    [SerializeField]
    private GameObject guidePanel;

    [SerializeField]
    private InputAction rollDiceAction, enableDebugPanel, moveCamOut, moveCamIn, pause, inventory, shop, bank;

    private bool isRolling = false;
    private bool paused = false;

    //important variables for game logic
    public float money; //amount of money that the player has
    private int[] range; //holds the highest and lowest number that any of the player's dice can roll for optimization purposes while parsing results of a roll
    private int turnsRemaining; //tracks how many turns the player has left, decreasing with each roll
    private float debt; //stores how much debt the player has, currently is a static goal
    private float interestRate; //the rate of interest that the debt accrues each turn

    private List<Die> allDice = new List<Die>();
    private List<Die> rollingDice = new List<Die>();
    private List<Die> rolledDice = new List<Die>();

    private List<Die> DiceForSale = new List<Die>();

    /// <summary>
    /// Called before the first active frame
    /// </summary>
    public void Start()
    {
        // Init dice manager
        DiceManager.Init();

        // Press space to roll dice
        rollDiceAction.Enable();
        rollDiceAction.performed += (InputAction.CallbackContext obj) =>
        {
            RollButton();
        };

        enableDebugPanel.Enable();
        enableDebugPanel.performed += (InputAction.CallbackContext obj) =>
        {
            debugPanel.gameObject.SetActive(!debugPanel.gameObject.activeSelf);
        };

        moveCamOut.Enable();
        moveCamOut.performed += (InputAction.CallbackContext obj) =>
        {
            cam.gameObject.transform.position = cam.gameObject.transform.position * 2;
        };

        moveCamIn.Enable();
        moveCamIn.performed += (InputAction.CallbackContext obj) =>
        {
            cam.gameObject.transform.position = cam.gameObject.transform.position / 2;
        };

        pause.Enable();
        pause.performed += (InputAction.CallbackContext obj) =>
        {
            paused = !paused;
        };
        inventory.Enable();
        inventory.performed += (InputAction.CallbackContext obj) =>
        {
            //shows the active dice
            activeDiceListLabel.text = allDice.Count + " Dice<br>";
            for (int i = 0; i < allDice.Count; i++)
            {
                activeDiceListLabel.text += allDice[i].dieName + ": ";
                for (int j = 0; j < allDice[i].sides.Count; j++)
                {
                    activeDiceListLabel.text += " " + allDice[i].sides[j];
                }
                activeDiceListLabel.text += "<br>";
            }

            //TODO: show the inactive dice
            
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        };
        shop.Enable();
        shop.performed += (InputAction.CallbackContext obj) =>
        {
            bankPanel.SetActive(false);
            WriteShop();
            shopPanel.SetActive(!shopPanel.activeSelf);
        };
        bank.Enable();
        bank.performed += (InputAction.CallbackContext obj) =>
        {
            shopPanel.SetActive(false);
            bankPanel.SetActive(!bankPanel.activeSelf);
        };

        debugPanel.gameObject.SetActive(false);

        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        guidePanel.SetActive(false);
        bankPanel.SetActive(false);

        DiceForSale.Add(DiceManager.GetRandom());
        DiceForSale.Add(DiceManager.GetRandom());

        money = 20.0f; //sets money to 0
        SetMoneyLabel(); //updates the money label
        turnsRemaining = 20; //set the number of remaining turns to the default value
        SetTurnsRemainingLabel(); //updates the turns remaining label
        debt = 500.0f; //sets the debt to the default value
        SetDebtLabel(); //sets the debt label on the UI
        interestRate = 1.03f;
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        range = new int[2]; //sets up the range
        range[0] = 999999;
        range[1] = 0;
        CalculateRange(); //calculates the range
    }

    public void Update()
    {
        if (paused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Called every frame after update
    /// </summary>
    public void LateUpdate()
    {
        if (isRolling)
        {
            bool check = true;
            foreach (Die d in rollingDice)
            {
                if (d.rolling)
                {
                    check = false; break;
                }
            }
            if (check)
            {
                bool check2 = true;
                foreach (Die d in rollingDice)
                {
                    rolledDice.Add(d);
                    if (d.value == 0)
                    {
                        check2 = false;
                        d.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
                        d.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1f, 1f), 5f, Random.Range(-1f, 1f));
                    }
                }
                if (check2)
                {
                    FinishedRolling();
                    rollingDice.Clear();
                }
            }
        }
    }

    private void CalculateRange()
    { //used to calculate the range of possible rolls for optimization

        for (int i = 0; i < allDice.Count; i++)
        {
            if (allDice[i].sides[0] < range[0])
            {
                range[0] = allDice[i].sides[0];
            }
            if (allDice[i].sides[allDice[i].sides.Count - 1] > range[1])
            {
                range[1] = allDice[i].sides[allDice[i].sides.Count - 1];
            }
        }
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

        outputLabel.text = "";

        if (tutorialManager.GetComponent<Tutorial>().tutorialStage == 0)
        {
            tutorialManager.GetComponent<Tutorial>().NextStage();
        }
    }

    public void ShopButton()
    {
        shopPanel.SetActive(true);
        WriteShop();

        if (tutorialManager.GetComponent<Tutorial>().tutorialStage == 2)
        {
            tutorialManager.GetComponent<Tutorial>().NextStage();
        }
    }

    public void Buy(int selection)
    {
        if (money >= DiceForSale[selection].price)
        {
            allDice.Add(GameObject.Instantiate<Die>(DiceForSale[selection]));
            allDice[allDice.Count - 1].gameObject.SetActive(false);
            money -= DiceForSale[selection].price;
            SetMoneyLabel();
            dieForSale[selection].transform.parent.gameObject.SetActive(false);
        }

        if (inventoryPanel.activeSelf) {
            ViewButton();
        }
    }

    private void StockShop()
    {
        for (int i = 0; i < dieForSale.Count; i++)
        {
            DiceForSale.Add(DiceManager.GetRandom());
            dieForSale[i].transform.parent.gameObject.SetActive(true);
        }
        WriteShop();
    }

    public void WriteShop()
    {
        for (int j = 0; j < dieForSale.Count; j++)
        {
            dieForSale[j].text = DiceForSale[0].dieName + "<br>Price $" + DiceForSale[0].price + "<br>Payout: " + DiceForSale[0].earnings + "<br>";
            dieForSale[j].text += "Sides: ";
            for (int i = 0; i < DiceForSale[0].sides.Count; i++)
            {
                dieForSale[j].text += " " + DiceForSale[0].sides[i];
            }
        }
    }

    public void RestockShop()
    {
        if (money >= 100)
        {
            StockShop();
            money -= 100;
            SetMoneyLabel();
        }
    }

    public void Payoff(int amount)
    {
        if (amount == -1)
        {
            amount = (int)money;
        }
        if (amount <= money)
        {
            money -= amount;
            debt -= amount;
        }
        if (debt <= 0)
        {
            money += -debt;
            debt = 0;
            outputLabel.text = "<br>Game over:<br>You Win";
            CloseShopButton();
        }
        SetMoneyLabel();
        SetDebtLabel();
    }

    public void ViewButton()
    { //shows what dice the player has when the view button is pressed or when a new die is bought
        //shows the active dice
        activeDiceListLabel.text = allDice.Count + " Dice<br>";
        for (int i = 0; i < allDice.Count; i++)
        {
            activeDiceListLabel.text += allDice[i].dieName + ": ";
            for (int j = 0; j < allDice[i].sides.Count; j++)
            {
                activeDiceListLabel.text += " " + allDice[i].sides[j];
            }
            activeDiceListLabel.text += "<br>";
            Debug.Log(allDice[i].dieName);
        }

        //TODO: show the inactive dice

        inventoryPanel.SetActive(true);

        if (tutorialManager.GetComponent<Tutorial>().tutorialStage == 1)
        {
            tutorialManager.GetComponent<Tutorial>().NextStage();
        }
    }

    public void CloseInventoryButton()
    {
        inventoryPanel.SetActive(false);
    }

    public void CloseShopButton()
    {
        shopPanel.SetActive(false);
    }

    public void BankButton()
    {
        bankPanel.SetActive(true);
    }

    public void CloseBankButton()
    {
        bankPanel.SetActive(false);
    }

    public void GuideButton()
    {
        guidePanel.SetActive(true);

        if (tutorialManager.GetComponent<Tutorial>().tutorialStage == 3)
        {
            tutorialManager.GetComponent<Tutorial>().NextStage();
        }
    }

    public void CloseGuideButton()
    {
        guidePanel.SetActive(false);
    }

    private void CalculateInterest()
    {
        debt *= interestRate;
        SetDebtLabel();
    }

    private void SetMoneyLabel()
    { //sets the money label
        moneyLabel.text = "$" + Mathf.Round(money);
    }

    private void SetTurnsRemainingLabel()
    { //sets the turns remaining label
        turnsRemainingLabel.text = "Turns Remaining: " + turnsRemaining;
    }

    private void SetDebtLabel()
    {
        debtLabel.text = "Debt: " + Mathf.Round(debt);
    }

    public void SaveAndExit()
    {
        //TODO: save

        SceneManager.LoadScene(0, LoadSceneMode.Single); //exit
    }

    /// <summary>
    /// Rolls an individual die
    /// </summary>
    /// <param name="die"></param>
    private void RollDie(Die die)
    {
        rollingDice.Add(die);
        die.gameObject.SetActive(true);
        die.gameObject.transform.position = new Vector3(Random.Range(-20, 20), 40f, Random.Range(-20, 20));
        die.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
        die.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 0f), Random.Range(-30f, 30f));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Returns true if all dice have successfully finished rolling</returns>
    private bool FinishedRolling()
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
                outputLabel.text += "<br>";
                int instancePayout = 0;
                for (int j = 0; j < allDice.Count; j++) {
                    if (allDice[j].value == i) {
                        totalPayout += (int)Mathf.Round(allDice[j].earnings);
                        instancePayout += (int)Mathf.Round(allDice[j].earnings);
                        outputLabel.text += i + ": " + allDice[j].earnings + " ";
                    }
                }
                outputLabel.text += " = " + instancePayout;
                //outputLabel.text += "<br>" + numberOfInstances + " " + i + "'s : " + (i * (numberOfInstances - 1) * 10);
                //totalPayout += i * (numberOfInstances - 1) * 10;
            }
        }
        outputLabel.text += "<br>Total Payout: " + totalPayout;
        money += totalPayout;
        SetMoneyLabel();
        CalculateInterest();

        //checks if the player loses
        turnsRemaining--;
        SetTurnsRemainingLabel();
        if (turnsRemaining <= 0)
        {
            outputLabel.text += "<br>Game over:<br>You have Run out of time";
        }
        if ((20 - turnsRemaining) % 3 == 0) {
            StockShop();
        }

        isRolling = false;
        rolledDice.Clear();
        return true;
    }

    public void AddD6Debug()
    {
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        allDice[allDice.Count - 1].gameObject.SetActive(false);
    }

    public void AddMoneyDebug()
    {
        money += 100;
        SetMoneyLabel();
    }
}