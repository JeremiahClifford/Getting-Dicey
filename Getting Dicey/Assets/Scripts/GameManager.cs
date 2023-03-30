using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private enum GameState
    {
        Standing,
        Sitting,
        Shop,
        Bank,
        GameOver
    }

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private CameraController camController;
    [SerializeField]
    private GameObject tutorialManager;

    [SerializeField]
    private TMP_Text moneyLabel;
    [SerializeField]
    private TMP_Text outputLabel;
    [SerializeField]
    private TMP_Text turnsRemainingLabel;
    [SerializeField]
    private TMP_Text debtLabel, interestLabel;
    [SerializeField]
    private CanvasRenderer debugPanel;
    [SerializeField]
    private GameObject standingUI, sittingUI, shopUI, bankUI, gameOverUI;
    [SerializeField]
    private ShopManager shop;

    // Number effect
    [SerializeField]
    private NumberEffect numberEffect;
    private float tempMoney;

    //inventory
    [SerializeField]
    private GameObject inventoryPanel;
    [SerializeField]
    private TMP_Text activeDiceListLabel;

    //payout guide
    [SerializeField]
    private GameObject guidePanel;

    [SerializeField]
    private InputAction rollDiceAction, enableDebugPanel, moveCamOut, moveCamIn, pause, inventory, openShop, bank, mouseClick;

    private bool isRolling = false, canRoll = true;
    private bool paused = false;

    //important variables for game logic
    public float money; //amount of money that the player has
    private int[] range; //holds the highest and lowest number that any of the player's dice can roll for optimization purposes while parsing results of a roll
    private int turnsRemaining; //tracks how many turns the player has left, decreasing with each roll
    private float debt; //stores how much debt the player has, currently is a static goal
    private float tempDebt; //stores how much debt the player has, currently is a static goal
    private float interestRate; //the rate of interest that the debt accrues each turn
    public int loopNum = 1; // How many times the game has been beaten
    private float baseDebt = 200f;
    private int baseTurnsRemaining = 6;
    private float baseInterest = 1.005f;

    private int bankCycle;
    private int bankCycleLength = 4;

    //stores the level the player is own, ie the number of times that they have payed off their debt
    private int debtIterations;
    [SerializeField]
    private string[] levelText;

    private List<Die> allDice = new List<Die>();
    private List<Die> rollingDice = new List<Die>();
    private List<Die> rolledDice = new List<Die>();

    private GameState state = GameState.Standing;

    public static GameManager instance;

    /// <summary>
    /// Called before the first active frame
    /// </summary>
    public void Start()
    {
        instance = this;

        // Init dice manager
        DiceManager.Init();

        cam.gameObject.transform.position = new Vector3(0, 65, -175);
        cam.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

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
        openShop.Enable();
        openShop.performed += (InputAction.CallbackContext obj) =>
        {
            state = GameState.Shop;
        };
        bank.Enable();
        bank.performed += (InputAction.CallbackContext obj) =>
        {
            state = GameState.Bank;
        };
        mouseClick.Enable();
        mouseClick.performed += (InputAction.CallbackContext obj) =>
        {
            if (state == GameState.Shop) 
            {
                shop.MouseClickToBuy();
            }
        };

        debugPanel.gameObject.SetActive(false);

        inventoryPanel.SetActive(false);
        guidePanel.SetActive(false);

        shop.StockShop();

        debtIterations = 0;
        outputLabel.text += levelText[debtIterations];

        bankCycle = 0;

        money = 150f;
        SetMoneyLabel(); //updates the money label
        turnsRemaining = baseTurnsRemaining; //set the number of remaining turns to the default value
        SetTurnsRemainingLabel(); //updates the turns remaining label
        debt = baseDebt; //sets the debt to the default value
        interestRate = baseInterest;
        SetDebtLabel(); //sets the debt label on the UI
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        allDice.Add(DiceManager.GetDie(DiceManager.DieIndex.D6));
        foreach (Die d in allDice)
        {
            d.gameObject.SetActive(false);
        }
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

        // State Machine
        switch (state)
        {
            case GameState.Standing:
                if (!camController.isAnimating())
                {
                    standingUI.gameObject.SetActive(true);
                }
                else
                {
                    standingUI.gameObject.SetActive(false);
                }
                sittingUI.gameObject.SetActive(false);
                shopUI.gameObject.SetActive(false);
                bankUI.gameObject.SetActive(false);
                gameOverUI.gameObject.SetActive(false);
                break;
            case GameState.Sitting:
                if (!camController.isAnimating() && canRoll)
                {
                    sittingUI.gameObject.SetActive(true);
                }
                else
                {
                    sittingUI.gameObject.SetActive(false);
                }
                standingUI.gameObject.SetActive(false);
                shopUI.gameObject.SetActive(false);
                bankUI.gameObject.SetActive(false);
                gameOverUI.gameObject.SetActive(false);
                break;
            case GameState.Shop:
                if (!camController.isAnimating())
                {
                    shopUI.gameObject.SetActive(true);
                }
                else
                {
                    shopUI.gameObject.SetActive(false);
                }
                sittingUI.gameObject.SetActive(false);
                standingUI.gameObject.SetActive(false);
                bankUI.gameObject.SetActive(false);
                gameOverUI.gameObject.SetActive(false);
                break;
            case GameState.Bank:
                if (!camController.isAnimating())
                {
                    bankUI.gameObject.SetActive(true);
                }
                else
                {
                    bankUI.gameObject.SetActive(false);
                }
                sittingUI.gameObject.SetActive(false);
                shopUI.gameObject.SetActive(false);
                standingUI.gameObject.SetActive(false);
                gameOverUI.gameObject.SetActive(false);
                break;
            case GameState.GameOver:
                gameOverUI.gameObject.SetActive(true);
                sittingUI.gameObject.SetActive(false);
                shopUI.gameObject.SetActive(false);
                standingUI.gameObject.SetActive(false);
                bankUI.gameObject.SetActive(false);
                break;
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
        if (!isRolling && canRoll)
        {
            isRolling = true;
            canRoll = false;
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
        state = GameState.Shop;
        camController.MoveTo(new Vector3(175, 75, -150), new Vector3(0, 90, 0), 1f);

        if (tutorialManager.GetComponent<Tutorial>().tutorialStage == 2)
        {
            tutorialManager.GetComponent<Tutorial>().NextStage();
        }
    }

    public void AdjustMoney(float adjustment)
    {
        money += adjustment;
    }

    public bool CanAfford(float cost)
    {
        if (money >= cost)
        {
            return true;
        }
        return false;
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

    public void BankButton()
    {
        state = GameState.Bank;
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

    public void SitButton()
    {
        state = GameState.Sitting;
        camController.MoveTo(new Vector3(0, 77.5f, -57), new Vector3(62, 0, 0), 0.5f);
    }

    public void StandButton()
    {
        state = GameState.Standing;
        camController.MoveTo(new Vector3(0, 65, -175), new Vector3(0, 0, 0), 0.5f);
    }

    private void CalculateInterest()
    {
        tempDebt = debt * interestRate - debt;
        tempDebt = Mathf.Clamp(tempDebt, 0, float.MaxValue);
        tempDebt = Mathf.Round(tempDebt);
        SetDebtLabel();
    }

    public void SetMoneyLabel()
    { //sets the money label
        moneyLabel.text = "$" + Mathf.Round(money);
        if (tempMoney != 0)
        {
            moneyLabel.text += " <#FFBC00><size=22>+ $" + Mathf.Round(tempMoney);
        }
    }

    public void SetTurnsRemainingLabel()
    { //sets the turns remaining label
        turnsRemainingLabel.text = "Turns Remaining: " + turnsRemaining;
    }

    public void SetDebtLabel()
    {
        debtLabel.text = "Debt: " + Mathf.Round(debt);
        tempDebt = Mathf.Clamp(tempDebt, 0, float.MaxValue);
        if (tempDebt < 0.001)
        {
            tempDebt = 0f;
        }

        if (tempDebt != 0)
        {
            debtLabel.text += " <color=red><size=22>+ $" + Mathf.Round(tempDebt);
        }

        interestLabel.text = "Interest: " + Mathf.Round((interestRate - 1) * 100) + "%";
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
        die.gameObject.transform.position = new Vector3(Random.Range(-20, 20), 50f, Random.Range(-20, 20));
        die.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(-200f, 200f), Random.Range(-200f, 200f), Random.Range(-200f, 200f));
        die.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 0f), Random.Range(-50f, 50f));
    }

    /// <summary>
    /// 
    /// </summary>
    private void FinishedRolling()
    {
        List<DiceMatch> diceMatches = new List<DiceMatch>();
        //parses the results, calculating the total payout and writing it to the display
        for (int i = 0; i <= 20; i++)
        {
            int numberOfInstances = 0;
            List<Die> d = new List<Die>();
            for (int j = 0; j < rolledDice.Count; j++)
            {
                if (rolledDice[j].value == i)
                {
                    numberOfInstances++;
                    d.Add(rolledDice[j]);
                }
            }
            if (numberOfInstances > 1)
            {
                DiceMatch match = new DiceMatch();
                match.dice = d;
                diceMatches.Add(match);
            }
        }
        DoPayoutEffects(diceMatches);
        /*
        outputLabel.text += "<br>Total Payout: " + totalPayout;
        money += totalPayout;
        SetMoneyLabel();
        CalculateInterest();
        */

        //checks if the player loses
        turnsRemaining--;
        SetTurnsRemainingLabel();
        shop.UpdateRestock();

        isRolling = false;
        rolledDice.Clear();

        bankCycle++;
        if (bankCycle == bankCycleLength) {
            BankButton();
            bankCycle = 0;
        }
    }
    public void AddDie(Die die)
    {
        allDice.Add(die);
        allDice[allDice.Count - 1].gameObject.SetActive(false);
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

    private void DoPayoutEffects(List<DiceMatch> matches)
    {
        int i = 0;
        for (i = 0; i < matches.Count; i++)
        {
            StartCoroutine(DisplayMatch(matches[i], 0.7f * i));
        }
        StartCoroutine(FinishMatches((i + 1) * 0.7f));
    }

    private IEnumerator DisplayMatch(DiceMatch match, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (Die d in match.dice)
        {
            NumberEffect effect = Instantiate<NumberEffect>(numberEffect);
            effect.gameObject.transform.position = d.gameObject.transform.position;
            effect.SetAmount(d.payout);

            tempMoney += d.payout;
            SetMoneyLabel();
        }
    }

    private IEnumerator FinishMatches(float delay)
    {
        yield return new WaitForSeconds(delay);
        canRoll = true;
        if (turnsRemaining <= 0)
        {
            if (debt == 0 || money + tempMoney >= debt + tempDebt)
            {
                debtIterations++;
                if (debtIterations >= levelText.Length) {
                    debtIterations = levelText.Length - 1;
                }
                money = money + tempMoney;
                debt = debt + tempDebt;
                outputLabel.text = "You Win!";
                if (money >= debt)
                {
                    outputLabel.text += "<br>You pay off the last of your debt with some of the cash you have on hand.";
                    money -= debt;
                }
                loopNum++;
                debt = baseDebt * Mathf.Pow(2, loopNum - 1);
                interestRate = 1 + ((baseInterest - 1) * loopNum);
                turnsRemaining = baseTurnsRemaining + (2 * (loopNum - 1));
                tempDebt = 0;
                tempMoney = 0;
                money = money / 2;
                outputLabel.text += levelText[debtIterations];
                bankCycle = 0;
                //outputLabel.text += "<br><br>But unfortunately it looks like you've managed rack up even more debt.";
                //outputLabel.text += "<br>You now owe " + debt + " at " + Mathf.Round((interestRate - 1) * 100) + "% interest!";
                //outputLabel.text += "<br>You have " + turnsRemaining + " turns to pay it off!";
                //outputLabel.text += "<br>You also have to pay taxes so you lose half your current money!";
                /*
                if (loopNum >= 3)
                {
                    int diceLost = Random.Range(loopNum, loopNum + 2 * (loopNum - 1));
                    int realDiceLost = 0;
                    for (int i = 0; i < diceLost && allDice.Count >= 4 * (loopNum / 2); i++)
                    {
                        realDiceLost++;
                        int index = Random.Range(0, allDice.Count);
                        Destroy(allDice[index]);
                    }
                    if (realDiceLost > 0)
                    {
                        outputLabel.text += "<br>You dropped your dice and lost " + realDiceLost + " of them!";
                    }
                }
                */
                SetMoneyLabel();
                SetDebtLabel();
            }
            else
            {
                outputLabel.text = "You failed to pay off your debt";
                state = GameState.GameOver;
            }
            outputLabel.gameObject.SetActive(true);
        }
        else
        {
            CalculateInterest();
        }
        if (tempMoney != 0)
        {
            float decrease = tempMoney / 20;
            int i = 0;
            for (i = 0; i < 20; i++)
            {
                StartCoroutine(DecreaseTempMoney(0.025f * i, decrease));
            }
        }
        if (tempDebt != 0)
        {
            float decrease = tempDebt / 20;
            int i = 0;
            for (i = 0; i < 20; i++)
            {
                StartCoroutine(DecreaseTempDebt((0.025f * i) + 1f, decrease));
            }
        }
    }

    private IEnumerator DecreaseTempMoney(float delay, float decrease)
    {
        yield return new WaitForSeconds(delay);
        if (tempMoney < decrease)
        {
            money += tempMoney;
            tempMoney = 0;
        }
        else
        {
            money += decrease;
            tempMoney -= decrease;
        }
        SetMoneyLabel();
    }

    private IEnumerator DecreaseTempDebt(float delay, float decrease)
    {
        yield return new WaitForSeconds(delay);
        if (tempDebt < decrease)
        {
            debt += tempDebt;
            tempDebt = 0;
        }
        else
        {
            debt += decrease;
            tempDebt -= decrease;
        }
        SetDebtLabel();
    }

    private struct DiceMatch
    {
        public List<Die> dice;
    }
}