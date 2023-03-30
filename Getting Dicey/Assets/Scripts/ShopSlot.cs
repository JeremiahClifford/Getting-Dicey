using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSlot : MonoBehaviour
{
    private Die currentDie;
    private DieDef currentDef;

    [SerializeField]
    private GameObject dieHolder;

    [SerializeField]
    private TMP_Text dieNameLabel, costLabel, payoutLabel;

    [SerializeField]
    private ShopManager shopManager;

    public float dieCost;

    public BoxCollider hoverCollider;

    private Vector3 angularVelocity = new Vector3(0, 25f, 15f);

    private bool sold = false;

    private void Update()
    {
        float delta = Time.deltaTime;
        if (currentDie)
        {
            currentDie.GetComponent<Rigidbody>().rotation = Quaternion.Euler(currentDie.GetComponent<Rigidbody>().rotation.eulerAngles + angularVelocity * delta);
        }
    }

    public void SetDie(DieDef def, string name, float cost, float payout)
    {
        if (currentDie)
        {
            Destroy(currentDie.gameObject);
        }
        Die d = GameObject.Instantiate<Die>(def.prefab, dieHolder.transform);
        d.dieName = def.dieName;
        d.sides = def.sideNums;
        d.transform.localPosition = new Vector3(0, 0, 0);
        d.transform.localScale = new Vector3(1, 1, 1);
        d.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        Destroy(d.gameObject.GetComponent<Collider>());
        dieNameLabel.text = name;
        costLabel.text = "Cost: " + cost;
        payoutLabel.text = "Payout: " + payout;
        dieNameLabel.color = new Color(255, 255, 255, 255);
        costLabel.color = new Color(255, 255, 255, 255);
        payoutLabel.color = new Color(255, 255, 255, 255);

        currentDef = def;
        currentDie = d;
        dieCost = cost;
        sold = false;
    }

    public void Buy()
    {
        if (GameManager.instance.CanAfford(dieCost) && currentDie)
        {
            GameManager.instance.AddDie(DiceManager.GetDie(currentDef.index));
            GameManager.instance.AdjustMoney(-dieCost);
            GameManager.instance.SetMoneyLabel();
            dieNameLabel.text = "SOLD";
            dieNameLabel.color = new Color(255, 176, 0, 255);
            costLabel.text = "";
            payoutLabel.text = "";
            shopManager.diceBought++;
            Destroy(currentDie.gameObject);
            currentDef = null;
            sold = true;
        }
    }

    public void SetHover(bool hovered)
    {
        if (!sold)
        {
            if (hovered)
            {
                if (GameManager.instance.CanAfford(dieCost))
                {
                    dieNameLabel.color = new Color(0, 255, 0, 255);
                    costLabel.color = new Color(0, 255, 0, 255);
                    payoutLabel.color = new Color(0, 255, 0, 255);
                }
                else
                {
                    dieNameLabel.color = new Color(255, 0, 0, 255);
                    costLabel.color = new Color(255, 0, 0, 255);
                    payoutLabel.color = new Color(255, 0, 0, 255);
                }
            }
            else
            {
                dieNameLabel.color = new Color(255, 255, 255, 255);
                costLabel.color = new Color(255, 255, 255, 255);
                payoutLabel.color = new Color(255, 255, 255, 255);
            }
        }
    }
}