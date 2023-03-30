using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private List<ShopSlot> slots = new List<ShopSlot>();
    [SerializeField]
    private TMP_Text restockText, restockTime;
    private float restockPrice = 50f;
    public int diceBought = 0;
    private int turnsToRestock = 2;

    // The percent increase of dice each round
    private float diceCostMultPerRound = 0.1f;

    // The base cost increase per dice bought
    private float diceCostIncreasePerBuy = 10;

    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit))
        {
            foreach (ShopSlot slot in slots)
            {
                if (hit.collider == slot.hoverCollider)
                {
                    slot.SetHover(true);
                }
                else
                {
                    slot.SetHover(false);
                }
            }
        }
        else
        {
            foreach (ShopSlot slot in slots)
            {
                slot.SetHover(false);
            }
        }
    }

    public void MouseClickToBuy()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit))
        {
            foreach (ShopSlot slot in slots)
            {
                if (hit.collider == slot.hoverCollider)
                {
                    slot.Buy();
                }
            }
        }
    }

    public void RestockShop()
    {
        if (GameManager.instance.CanAfford(restockPrice))
        {
            StockShop();
            GameManager.instance.AdjustMoney(-restockPrice);
            GameManager.instance.SetMoneyLabel();
            restockPrice += 25f;
            restockText.text = "Restock Shop: $" + restockPrice;
        }
    }

    public void StockShop()
    {
        restockTime.text = turnsToRestock + " turns until restock";
        for (int i = 0; i < slots.Count; i++)
        {
            DieDef def = DiceManager.GetRandomDef();
            float cost = (def.cost + (diceCostIncreasePerBuy * diceBought)) * (1 + (diceCostMultPerRound * (GameManager.instance.loopNum - 1)));
            slots[i].SetDie(def, def.dieName, cost, def.payout);
        }
    }

    public void UpdateRestock()
    {
        turnsToRestock--;
        if (turnsToRestock <= 0)
        {
            StockShop();
            turnsToRestock = 2 + GameManager.instance.loopNum - 1;
        }
        restockTime.text = turnsToRestock + " turns until restock";
    }

    /*
    public void WriteShop()
    {
        restockText.text = "Restock Shop: $" + restockPrice;
        for (int j = 0; j < dieShopLabels.Count; j++)
        {
            dieShopLabels[j].text = diceForSale[j].dieName + "<br>Price $" + diceForSale[j].cost * (1 + ((diceBought * (GameManager.instance.loopNum)) * 0.1f)) + "<br>Payout: " + diceForSale[j].earnings + "<br>";
            dieShopLabels[j].text += "Sides: ";
            for (int i = 0; i < diceForSale[j].sideNums.Count; i++)
            {
                dieShopLabels[j].text += " " + diceForSale[j].sideNums[i];
            }
        }
    }
    */
}
