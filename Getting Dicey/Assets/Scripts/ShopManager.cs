using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private List<TMP_Text> dieShopLabels;
    [SerializeField]
    private TMP_Text restockText;
    private List<DieDef> diceForSale = new List<DieDef>();
    private float restockPrice = 50f;

    public void RestockShop()
    {
        if (GameManager.instance.CanAfford(restockPrice))
        {
            StockShop();
            GameManager.instance.AdjustMoney(-restockPrice);
            GameManager.instance.SetMoneyLabel();
            restockPrice += 25f * Mathf.Pow(2, GameManager.instance.loopNum - 1);
            restockText.text = "Restock Shop: $" + restockPrice;
        }
    }

    public void Buy(int selection)
    {
        if (GameManager.instance.CanAfford(diceForSale[selection].cost))
        {
            GameManager.instance.AddDie(DiceManager.GetDie(diceForSale[selection].index));
            GameManager.instance.AdjustMoney(-diceForSale[selection].cost);
            GameManager.instance.SetMoneyLabel();
            dieShopLabels[selection].transform.parent.gameObject.SetActive(false);
        }
    }

    public void StockShop()
    {
        diceForSale.Clear();
        for (int i = 0; i < dieShopLabels.Count; i++)
        {
            diceForSale.Add(DiceManager.GetRandomDef());
            dieShopLabels[i].transform.parent.gameObject.SetActive(true);
        }
        WriteShop();
    }

    public void WriteShop()
    {
        restockText.text = "Restock Shop: $" + restockPrice;
        for (int j = 0; j < dieShopLabels.Count; j++)
        {
            dieShopLabels[j].text = diceForSale[j].dieName + "<br>Price $" + diceForSale[j].cost + "<br>Payout: " + diceForSale[j].earnings + "<br>";
            dieShopLabels[j].text += "Sides: ";
            for (int i = 0; i < diceForSale[j].sideNums.Count; i++)
            {
                dieShopLabels[j].text += " " + diceForSale[j].sideNums[i];
            }
        }
    }
}
